using Cosmos.Core;
using Flags = System.FlagsAttribute;
using System.Runtime.InteropServices;
using Kernel.System.Core;

namespace Kernel.System.Hardware.ATA
{
    public static unsafe class ATA
    {
        private static BaseIOGroups baseIOGroup = new BaseIOGroups();

        [Flags]
        public enum ControllerIdEnum
        {
            Primary,
            Secondary
        }

        [Flags]
        public enum BusPositionEnum
        {
            Master,
            Slave
        }

        [Flags]
        public enum SpecLevel
        {
            Null,
            ATA,
            ATAPI
        }

        [Flags]
        public enum Cmd : byte
        {
            ReadPio = 0x20,
            ReadPioExt = 0x24,
            ReadDma = 0xC8,
            ReadDmaExt = 0x25,
            WritePio = 0x30,
            WritePioExt = 0x34,
            WriteDma = 0xCA,
            WriteDmaExt = 0x35,
            CacheFlush = 0xE7,
            CacheFlushExt = 0xEA,
            Packet = 0xA0,
            IdentifyPacket = 0xA1,
            Identify = 0xEC,
            Read = 0xA8,
            Eject = 0x1B
        }

        [Flags]
        public enum Status : byte
        {
            None = 0x00,
            Busy = 0x80,
            ATA_SR_DRD = 0x40,
            ATA_SR_DF = 0x20,
            ATA_SR_DSC = 0x10,
            DRQ = 0x08,
            ATA_SR_COR = 0x04,
            ATA_SR_IDX = 0x02,
            Error = 0x01
        };

        [Flags]
        public enum DvcSelVal : byte
        {
            Slave = 0x10,
            LBA = 0x40,
            Default = 0xA0
        };

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
            public Partition[] partitions;
            public uint EBRLocation;

            public void Setup(BlockDevice hd, byte* aMBR)
            {
                partitions = new Partition[4];
                ParsePartition(0, hd, aMBR, 446);
                ParsePartition(1, hd, aMBR, 462);
                ParsePartition(2, hd, aMBR, 478);
                ParsePartition(3, hd, aMBR, 494);
            }

            public void ParsePartition(int no, BlockDevice hd, byte* aMBR, uint aLoc)
            {
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

                    Partition xPartInfo = new Partition();
                    xPartInfo.Setup(hd, xStartSector, xSectorCount, xSystemID);
                    partitions[no] = xPartInfo;
                }
            }
        }

        public static void Init()
        {
            InitAta(ControllerIdEnum.Primary, BusPositionEnum.Master);
            InitAta(ControllerIdEnum.Primary, BusPositionEnum.Slave);
            InitAta(ControllerIdEnum.Secondary, BusPositionEnum.Master);
            InitAta(ControllerIdEnum.Secondary, BusPositionEnum.Slave);
        }

        private static void InitAta(ControllerIdEnum aControllerID, BusPositionEnum aBusPosition)
        {
            var xIO = aControllerID == ControllerIdEnum.Primary ? baseIOGroup.ATA1 : baseIOGroup.ATA2;
            var xATA = new BlockDevice();
            xATA.Setup(xIO, aControllerID, aBusPosition);

            if (xATA.DriveType == SpecLevel.Null)
            {
                return;
            }
            if (xATA.DriveType != SpecLevel.ATA)
            {
                return;
            }
            DeviceManager.AllDevices.Add(xATA);
        }
    }
}