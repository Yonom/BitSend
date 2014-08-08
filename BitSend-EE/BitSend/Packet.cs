namespace BitSend
{
    internal enum Packet
    {
        Chunk = 0,
        BreakChunk = -1,
        Bai = -2,
        Hai = ~ChunkPacket.Data,
        Hey = ~ChunkPacket.Data - 1
    }
}