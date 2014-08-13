using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using PlayerIOClient;

namespace BitSend
{
    internal class SendManager
    {
        private readonly Connection _connection;
        private readonly object _lockObj = new object();
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);

        private bool[] _check;
        private List<int> _chunk;
        private bool _isEnabled = true;
        private int _lastPos;
        private int _offsetAdd;
        private int _pointer;

        public SendManager(Connection connection)
        {
            this._connection = connection;
        }

        public bool IsEnabled
        {
            get { return this._isEnabled; }
            set
            {
                this._isEnabled = value;

                this.Send(value
                    ? Packet.Hey
                    : Packet.Bai);
            }
        }

        public event StatusEventHandler Status;

        protected virtual void OnStatus(int current, int total)
        {
            StatusEventHandler handler = this.Status;
            if (handler != null) handler(current, total);
        }

        private List<int> Parse(byte[] bytes)
        {
            var chunk = new List<int>();

            int readPointer = 0;
            var a = new BitArray(bytes);

            while (readPointer < a.Length)
            {
                int writeCount = a.Length - readPointer;
                if (writeCount > 30) writeCount = 30;

                int writePointer = 0;
                var vector = new BitVector32();

                // Copy the data
                while (writePointer < writeCount)
                    vector[1 << writePointer++] = a[readPointer++];

                // Add whitespace
                while (writePointer++ <= 30)
                    vector[1 << writePointer] = true;

                chunk.Add(vector.Data);
            }

            return chunk;
        }

        public void Send(Packet packet)
        {
            lock (this._lockObj)
            {
                this.SendPacket((int)packet);
            }
        }

        private void SendPacket(int packet)
        {
            this._connection.Send("c", packet, 0, 0);
            WaitOne();
        }

        public bool Send(byte[] bytes)
        {
            lock (this._lockObj)
            {
                // Mark the start of the chunk.
                this.SendPacket((int)Packet.StartChunk);

                this._offsetAdd = 0;
                this._chunk = this.Parse(bytes);
                while (this._chunk.Count > 0)
                {
                    // Reset variables
                    this._check = new bool[this._chunk.Count];
                    this._pointer = 0;
                    this._lastPos = 0;

                    // Send packets
                    while (this._pointer < this._chunk.Count)
                    {
                        if (!this._connection.Connected || !this.IsEnabled)
                            return false;

                        this.Status(this._pointer, this._chunk.Count);
                        this.SendPacket(this._chunk[this._pointer++]);
                    }

                    // Wait until the last message arrives
                    this._resetEvent.WaitOne(1000);

                    // Repair if necessary
                    this._chunk = GetRepairChunk(this._chunk, this._check, ref this._offsetAdd);
                }

                // Mark the end of the chunk.
                this.SendPacket((int)Packet.EndChunk);
            }

            return true;
        }

        public void HandlePacket(int packet)
        {
            if (this._chunk == null) return;
            for (int i = this._lastPos; i <= this._pointer - 1; i++)
            {
                if (this._chunk[i] == packet && !this._check[i])
                {
                    this._lastPos = i;
                    this._check[i] = true;

                    // If this was the last message sent, stop waiting.
                    if (this._chunk.Count - 1 == i)
                        this._resetEvent.Set();

                    return;
                }
            }
        }

        private static void WaitOne()
        {
            Thread.Sleep(10);
        }

        private static List<int> GetRepairChunk(List<int> chunk, bool[] check, ref int offsetAdd)
        {
            var repairChunk = new List<int>();
            for (int i = 0; i < chunk.Count; i++)
            {
                if (check[i]) continue;

                int repairPacket = chunk[i];
                repairChunk.Add(repairPacket);
                int repairPos = i + offsetAdd--;
                repairChunk.Add(repairPos);

                const int maxPos = 1 << 31 - 33;
                if (repairPos > maxPos)
                    throw new InternalBufferOverflowException("Reached the maximum chunk size limit.");
            }
            offsetAdd += chunk.Count;
            return repairChunk;
        }
    }
}