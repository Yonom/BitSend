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

        private Chunk GetChunk(int userId)
        {
            if (this._chunks.ContainsKey(userId))
                return this._chunks[userId];

            var chunk = new Chunk();
            this._chunks.Add(userId, chunk);
            return chunk;
        }

        private static void RepairChunk(Chunk chunk)
        {
            // Inserts all missed packets
            for (int i = chunk.Count - 1; i >= 0; i--)
            {
                ChunkPacket pointerPacket = chunk[i];
                ChunkPacket type = pointerPacket.GetPacketType();
                if (type == ChunkPacket.Restore)
                {
                    var pointer = (int)pointerPacket;
                    if (pointer < 0 || pointer >= i - 1) // i - 1 because every restore message is two packets long
                        throw new InvalidDataException("Received invalid pointer.");

                    ChunkPacket packet = chunk[i - 1];

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

            foreach (ChunkPacket packet in chunk)
            {
                int readCount = 30;
                int readPointer = 0;
                var vector = new BitVector32((int)packet);

                // Find the starting point of the data
                while (vector[1 << readCount--]) { }

                // Copy the data
                while (readPointer <= readCount)
                    a[writePointer++] = vector[1 << readPointer++];
            }

            return a.ToByteArray().Take(writePointer + 1 >> 3).ToArray();
        }

        public void HandlePacket(int userId, ChunkPacket packet)
        {
            this.GetChunk(userId).Add(packet);
        }

        public byte[] BreakChunk(int userId)
        {
            return ParseChunk(this.GetChunk(userId));
        }
    }
}