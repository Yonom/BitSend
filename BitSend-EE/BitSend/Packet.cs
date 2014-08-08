namespace BitSend
{
    internal enum Packet
    {
        Chunk = 0,
        BreakChunk = ~0,
        Bai = ~1,
        Hai = ~ChunkPacket.Data,
        Hey = ~ChunkPacket.Data - 1
    }
}