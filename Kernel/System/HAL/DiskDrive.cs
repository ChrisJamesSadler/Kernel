using System.Collections.Generic;
using System;
using Kernel.System.IO;
using Kernel.System.HAL.ATA;

namespace Kernel.System.HAL
{
    public unsafe abstract class DiskDrive
    {
        [Flags]
        public enum ATAControllerIdEnum
        {
            Primary,
            Secondary
        }

        [Flags]
        public enum ATABusPositionEnum
        {
            Master,
            Slave
        }

        [Flags]
        public enum ATASpecLevel
        {
            Null,
            ATA,
            ATAPI
        }

        [Flags]
        public enum ATACmd : byte
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
        public enum ATAStatus : byte
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
        public enum ATADvcSelVal : byte
        {
            Slave = 0x10,
            LBA = 0x40,
            Default = 0xA0
        };

        public static List<DiskDrive> DetectedDiskDrives;
        public List<Partition> DetectedPartitions = new List<Partition>();
        public string Label;

        public static void Init()
        {
            DetectedDiskDrives = new List<DiskDrive>();
            DetectATADrive(ATAControllerIdEnum.Primary, ATABusPositionEnum.Master);
            DetectATADrive(ATAControllerIdEnum.Primary, ATABusPositionEnum.Slave);
            DetectATADrive(ATAControllerIdEnum.Secondary, ATABusPositionEnum.Master);
            DetectATADrive(ATAControllerIdEnum.Secondary, ATABusPositionEnum.Slave);
            foreach(var drive in DetectedDiskDrives)
            {
                Console.WriteLine(drive.Label);
            }
        }

        public static void DetectATADrive(ATAControllerIdEnum aControllerID, ATABusPositionEnum aBusPosition)
        {
            bool primary = aControllerID == ATAControllerIdEnum.Primary;
            var xBAR0 = (ushort)(!primary ? 0x0170 : 0x01F0);
            var xBAR1 = (ushort)(!primary ? 0x0374 : 0x03F4);
            ushort IOControl = (ushort)(xBAR1 + 2);
            ushort IOCommand = (ushort)(xBAR0 + 7);
            ushort IOData = (ushort)xBAR0;
            ushort IOStatus = (ushort)(xBAR0 + 7);
            ushort IODeviceSelect = (ushort)(xBAR0 + 6);
            ushort IOLBA0 = (ushort)(xBAR0 + 3);
            ushort IOLBA1 = (ushort)(xBAR0 + 4);
            ushort IOLBA2 = (ushort)(xBAR0 + 5);
            ushort IOSectorCount = (ushort)(xBAR0 + 2);
            IOPort.outb(IOControl, 0x02);

            ATASpecLevel DriveType = DiscoverATADrive(IOLBA1, IOLBA2, IOCommand, IOStatus, IODeviceSelect, aBusPosition);

            if (DriveType == ATASpecLevel.Null)
            {
                return;
            }
            if (DriveType != ATASpecLevel.ATA)
            {
                return;
            }

            DetectedDiskDrives.Add(new ATADiskDrive(primary, aControllerID, aBusPosition, DriveType));
        }

        public static ATASpecLevel DiscoverATADrive(ushort IOLBA1, ushort IOLBA2, ushort IOCommand, ushort IOStatus, ushort IODeviceSelect, ATABusPositionEnum mBusPosition)
        {
            SelectATADrive(0, IODeviceSelect, mBusPosition, IOStatus);
            var xIdentifyStatus = SendATACmd(ATACmd.Identify, IOCommand, IOStatus);
            if (xIdentifyStatus == ATAStatus.None)
            {
                return ATASpecLevel.Null;
            }
            else if ((xIdentifyStatus & ATAStatus.Error) != 0)
            {
                int xTypeId = IOPort.inb(IOLBA2) << 8 | IOPort.inb(IOLBA1);
                if (xTypeId == 0xEB14 || xTypeId == 0x9669)
                {
                    return ATASpecLevel.ATAPI;
                }
                else
                {
                    return ATASpecLevel.Null;
                }
            }
            else if ((xIdentifyStatus & ATAStatus.DRQ) == 0)
            {
                return ATASpecLevel.Null;
            }
            return ATASpecLevel.ATA;
        }

        public static ATAStatus SendATACmd(ATACmd aCmd, ushort IOCommand, ushort IOStatus)
        {
            IOPort.outb(IOCommand, (byte)aCmd);
            ATAStatus xStatus;
            do
            {
                ATAWait(IOStatus);
                xStatus = (ATAStatus)IOPort.inb(IOStatus);
            } while ((xStatus & ATAStatus.Busy) != 0);
            return xStatus;
        }

        public static void ATAWait(ushort IOStatus)
        {
            byte xVoid;
            xVoid = IOPort.inb(IOStatus);
            xVoid = IOPort.inb(IOStatus);
            xVoid = IOPort.inb(IOStatus);
            xVoid = IOPort.inb(IOStatus);
        }

        public static void SelectATADrive(byte aLbaHigh4, ushort IODeviceSelect, ATABusPositionEnum mBusPosition, ushort IOStatus)
        {
            IOPort.outb(IODeviceSelect, (byte)((byte)(ATADvcSelVal.Default | ATADvcSelVal.LBA | (mBusPosition == ATABusPositionEnum.Slave ? ATADvcSelVal.Slave : 0)) | aLbaHigh4));
            ATAWait(IOStatus);
        }

        public abstract void ReadBlock(ulong aBlockNo, ulong aBlockCount, byte* aData);
    }
}