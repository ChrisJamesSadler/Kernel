namespace Kernel.System.HAL
{
    public class Partition
    {
        public DiskDrive host;
        public uint id;
        public ulong offset;
        public ulong size;

        public Partition(DiskDrive host, ulong offset, ulong size, uint id)
        {
            this.host = host;
            this.offset = offset;
            this.size = size;
            this.id = id;
        }
    }
}