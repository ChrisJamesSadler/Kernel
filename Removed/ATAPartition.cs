namespace Kernel.System.HAL
{
    public unsafe class ATAPartition
    {
        public byte mountPoint;
        public ulong fatStart;
        public ulong fatSize;
        public ulong dataStart;
        public ulong rootStart;
        public ulong sectorsPerCluster;
        public ATABlockDevice host;
        public uint id;
        public ulong offset;
        public ulong size;
        public byte* rootName;

        public ATAPartition(ATABlockDevice host, ulong offset, ulong size, uint id)
        {
            this.host = host;
            this.offset = offset;
            this.size = size;
            this.id = id;
        }
    }
}
