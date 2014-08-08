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
            for (var i = chunk.Count - 1; i >= 0; i--)
            {
                var packet = chunk[i];
                var type = packet.GetPacketType();
                if (type == ChunkPacket.Restore)
                {
                    var pointerPacket = chunk[i - 1];
                    var pointer = (int)(pointerPacket & ~(ChunkPacket.Data));
                    if (pointer < 0 || pointer > i) // i - 1 because every restore message is two packets long
                        throw new InvalidDataException("Received invalid pointer.");

                    // Change the packet type to data
                    if (pointerPacket.GetPacketType() == ChunkPacket.Data)
                        packet |= ChunkPacket.Data;

                    chunk.RemoveAt(i);
                    chunk.RemoveAt(i - 1);
                    chunk.Insert(pointer, packet); // This insert makes up for the skiping of the pointer
                }
            }
        }

        private static byte[] ParseChunk(Chunk chunk)
        {
            RepairChunk(chunk);

            var writePointer = 0;
            var a = new BitArray(chunk.Count * 30); // Max possible amount (30 bits per packet)

            foreach (var packet in chunk)
            {
                var readCount = 30;
                var readPointer = 0;
                var vector = new BitVector32((int)packet);

                // Find the starting point of the data
                while (vector[1 << readCount])
                {
                    readCount--;
                }
                readCount--; // decrease one last time to skip the first 0

                // Copy the data
                while (readPointer <= readCount)
                {
                    a[writePointer] = vector[1 << readPointer];

                    readPointer++;
                    writePointer++;
                }
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