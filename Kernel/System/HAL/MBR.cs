using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.System.HAL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PartitionTableEntry
    {
        public byte bootable;
        public byte start_head;
        public byte start_sector;
        public byte start_cylinder;

        public byte partition_id;

        public byte end_head;
        public byte end_sector;
        public byte end_cylinder;

        public uint start_lba;
        public uint length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MBR
    {
        public List<Partition> partitions;
        public uint EBRLocation;

        public void Setup(DiskDrive hd, byte* aMBR)
        {
            partitions = new List<Partition>();
            ParsePartition(0, hd, aMBR, 446);
            ParsePartition(1, hd, aMBR, 462);
            ParsePartition(2, hd, aMBR, 478);
            ParsePartition(3, hd, aMBR, 494);
        }

        public void ParsePartition(uint no, DiskDrive hd, byte* aMBR, uint aLoc)
        {
            var info = (PartitionTableEntry*)(uint)aMBR;
            byte xSystemID = aMBR[aLoc + 4];
            EBRLocation = 0;
            if (xSystemID == 0x5 || xSystemID == 0xF || xSystemID == 0x85)
            {
                EBRLocation = Utils.ToUInt32(aMBR, aLoc + 8);
            }
            else if (xSystemID != 0)
            {
                uint xStartSector = Utils.ToUInt32(aMBR, aLoc + 8);
                uint xSectorCount = Utils.ToUInt32(aMBR, aLoc + 12);

                Partition xPartInfo = new Partition(hd, xStartSector, xSectorCount, no);
                partitions.Add(xPartInfo);
            }
        }
    }
}