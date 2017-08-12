namespace Kernel.System.Hardware.ATA
{
    public unsafe class Partition : Device
    {
        public byte mountPoint;
        public ulong fatStart;
        public ulong fatSize;
        public ulong dataStart;
        public ulong rootStart;
        public ulong sectorsPerCluster;
        public byte* rootName;
        public BlockDevice host;
        public uint id;
        public ulong offset;
        public ulong size;

        public override string Name()
        {
            return "Mass Storage Partition";
        }

        public void Setup(BlockDevice host, ulong offset, ulong size, uint id)
        {
            this.host = host;
            this.offset = offset;
            this.size = size;
            this.id = id;
        }

        public void Read(ulong aBlockNo, ulong aBlockCount, byte* aData)
        {
            host.ReadBlock(aBlockNo, aBlockCount, aData);
        }

        public void Write(ulong aBlockNo, ulong aBlockCount, byte* aData)
        {
            host.WriteBlock(aBlockNo, aBlockCount, aData);
        }
    }
}