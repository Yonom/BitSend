using System;

namespace BitSend
{

    internal enum ChunkPacket
    {
        Restore = 0,
        Data = 1 << 31,
    }
}