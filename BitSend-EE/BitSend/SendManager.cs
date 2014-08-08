using System.Collections;
using System.Collections.Specialized;
using System.Threading;

namespace BitSend
{
    internal class SendManager
    {
        private readonly object _lockObj = new object();
        private readonly BitSendClient _client;

        private Chunk _chunk;
        private bool[] _check;
        private int _pointer;
        private int _lastPos;

        public SendManager(BitSendClient client)
        {
            this._client = client;
        }

        private Chunk Parse(byte[] bytes)
        {
            var chunk = new Chunk();

            var readPointer = 0;
            var a = new BitArray(bytes);

            while (readPointer < a.Length)
            {
                var writeCount = a.Length - readPointer;
                if (writeCount > 30)
                    writeCount = 30;

                var writePointer = 0;
                var vector = new BitVector32();

                // Copy the data
                while (writePointer < writeCount)
                {
                    vector[1 << writePointer] = a[readPointer];

                    readPointer++;
                    writePointer++;
                }

                // Add whitespace
                writePointer++; // Leave one bit free
                while (writePointer <= 31)
                {
                    vector[1 << writePointer] = true;
                    writePointer++;
                }

                chunk.Add((ChunkPacket)vector.Data);
            }

            return chunk;
        }

        public void Send(byte[] bytes)
        {
            this.SendChunk(this.Parse(bytes));
        }

        private void SendChunk(Chunk chunk)
        {
            lock (this._lockObj)
            {
                var offsetAdd = 0;
                while (this._chunk == null || this._chunk.Count > 0)
                {
                    this._check = new bool[chunk.Count];
                    this._chunk = chunk;
                    this._pointer = 0;
                    this._lastPos = 0;

                    while (this._pointer < this._chunk.Count)
                    {
                        _client.SendPacket((int)this._chunk[_pointer]);
                        _pointer++;
                        Thread.Sleep(5);
                    }

                    Thread.Sleep(500);

                    var repairChunk = new Chunk();
                    for (var i = 0; i < this._chunk.Count; i++)
                    {
                        if (this._check[i]) continue;
                        var data = this._chunk[i];

                        var repairPos = (ChunkPacket)i + offsetAdd--;
                        if (data.GetPacketType() == ChunkPacket.Data)
                            repairPos |= ChunkPacket.Data;
                        repairChunk.Add(repairPos);

                        var repairPacket = this._chunk[i] & ~ChunkPacket.Data; // Remove the data flag
                        repairChunk.Add(repairPacket);
                    }
                    offsetAdd += chunk.Count;
                    chunk = repairChunk;
                }

                _client.SendPacket((int)Packet.BreakChunk);
            }
            this._chunk = null;
        }

        public void HandlePacket(ChunkPacket packet)
        {
            if (this._chunk == null) return;
            for (var i = _lastPos; i <= _pointer; i++)
            {
                if (this._chunk[i] == packet && !this._check[i])
                {
                    _lastPos = i;
                    this._check[i] = true;
                    return;
                }
            }
        }
    }
}