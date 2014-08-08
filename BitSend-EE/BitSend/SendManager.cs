using System.Collections;
using System.Collections.Specialized;
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
        private Chunk _chunk;
        private int _lastPos;
        private int _pointer;
        private int _offsetAdd;

        public SendManager(Connection connection)
        {
            this._connection = connection;
        }

        private Chunk Parse(byte[] bytes)
        {
            var chunk = new Chunk();

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

        public void Send(byte[] bytes)
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
                        this.SendPacket(this._chunk[this._pointer++]);

                    // Wait until the last message arrives
                    this._resetEvent.WaitOne(1000);

                    // Repair if necessary
                    this._chunk = GetRepairChunk(this._chunk, this._check, ref this._offsetAdd);
                }

                // Mark the end of the chunk.
                this.SendPacket((int)Packet.EndChunk);
            }
        }

        public void HandlePacket(int packet)
        {
            if (this._chunk == null) return;
            for (int i = this._lastPos; i <= this._pointer; i++)
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

        private static Chunk GetRepairChunk(Chunk chunk, bool[] check, ref int offsetAdd)
        {
            var repairChunk = new Chunk();
            for (int i = 0; i < chunk.Count; i++)
            {
                if (check[i]) continue;

                int repairPacket = chunk[i];
                repairChunk.Add(repairPacket);
                int repairPos = i + offsetAdd--;
                repairChunk.Add(repairPos);
            }
            offsetAdd += chunk.Count;
            return repairChunk;
        }
    }
}