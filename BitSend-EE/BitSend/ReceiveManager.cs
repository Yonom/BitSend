using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace BitSend
{
    internal class ReceiveManager
    {
        private readonly Dictionary<int, Chunk> _chunks = new Dictionary<int, Chunk>();

        // Inserts all missed packets
        private static void RepairChunk(Chunk chunk)
        {
            for (int i = chunk.Count - 1; i >= 0; i--)
            {
                int pointer = chunk[i];
                if (!pointer.IsDataPacket())
                {
                    if (pointer < 0 || pointer >= i - 1) // i - 1 because every restore message is two packets long
                        throw new InvalidDataException("Received invalid pointer.");

                    var packet = chunk[i - 1];
                    chunk.RemoveAt(i);
                    chunk.RemoveAt(i - 1);
                    chunk.Insert(pointer, packet); // This insert makes up for the skiping of the pointer
                }
            }
        }

        private static byte[] ParseChunk(Chunk chunk)
        {
            RepairChunk(chunk);

            int writePointer = 0;
            var a = new BitArray(chunk.Count * 30); // Max possible amount (30 bits per packet)

            foreach (int packet in chunk)
            {
                int readCount = 30;
                int readPointer = 0;
                var vector = new BitVector32(packet);

                // Find the starting point of the data
                while (vector[1 << readCount--]) { }

                // Copy the data
                while (readPointer <= readCount)
                    a[writePointer++] = vector[1 << readPointer++];
            }

            return a.ToByteArray().Take(writePointer + 1 >> 3).ToArray();
        }

        public void HandlePacket(int userId, int packet)
        {
            if (this._chunks.ContainsKey(userId))
                this._chunks[userId].Add(packet);
        }

        public void StartChunk(int userId)
        {
            var chunk = new Chunk();
            this._chunks.Add(userId, chunk);
        }

        public byte[] EndChunk(int userId)
        {
            if (!this._chunks.ContainsKey(userId))
                return new byte[0];
            return ParseChunk(this._chunks[userId]);
        }
    }
}