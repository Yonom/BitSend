using System;

namespace BitSend
{
    [Flags]
    internal enum ChunkPacket
    {
        Restore = 0,
        Data = 1 << 31,
    }
}