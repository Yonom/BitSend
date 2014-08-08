namespace BitSend
{
    internal enum Packet
    {
        StartChunk = -1,
        EndChunk = -2,
        Hai = 1 << 31 - 1,
        Hey = 1 << 31 - 2,
        Bai = 1 << 31 - 3,
    }
}