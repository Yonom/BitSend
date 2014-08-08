using System.Collections;

namespace BitSend
{
    internal static class ExtensionMethods
    {
        public static byte[] ToByteArray(this BitArray bits)
        {
            int numBytes = bits.Count / 8;
            if (bits.Count % 8 != 0) numBytes++;

            var bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                    bytes[byteIndex] |= (byte)(1 << bitIndex);

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }

        public static ChunkPacket GetPacketType(this ChunkPacket packet)
        {
            return (packet & ChunkPacket.Data) == ChunkPacket.Data
                ? ChunkPacket.Data
                : ChunkPacket.Restore;
        }
    }
}