using Kernel.System.Core;
using Kernel.System.IO;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kernel.System.HAL
{
    public unsafe class ATABlockDevice
    {
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

        public ulong BlockCount;
        public ulong BlockSize;
        private ushort IOControl;
        private ushort IOCommand;
        private ushort IOData;
        private ushort IOStatus;
        private ushort IODeviceSelect;
        private ushort IOLBA0;
        private ushort IOLBA1;
        private ushort IOLBA2;
        private ushort IOSectorCount;
        public ControllerIdEnum mControllerID;
        public BusPositionEnum mBusPosition;
        public SpecLevel DriveType;
        public bool LBA48Bit;
        public byte* DeviceLa;
        public byte* SerialNo;
        public byte* FirmwareRev;
        public byte* ModelNo;
        public byte PartitionCount;
        public string Label;
        public bool Exists = false;

        public ATABlockDevice(bool primary, ControllerIdEnum aControllerId, BusPositionEnum aBusPosition)
        {
            var xBAR0 = (ushort)(!primary ? 0x0170 : 0x01F0);
            var xBAR1 = (ushort)(!primary ? 0x0374 : 0x03F4);
            IOControl = (ushort)(xBAR1 + 2);
            IOCommand = (ushort)(xBAR0 + 7);
            IOData = (ushort)xBAR0;
            IOStatus = (ushort)(xBAR0 + 7);
            IODeviceSelect = (ushort)(xBAR0 + 6);
            IOLBA0 = (ushort)(xBAR0 + 3);
            IOLBA1 = (ushort)(xBAR0 + 4);
            IOLBA2 = (ushort)(xBAR0 + 5);
            IOSectorCount = (ushort)(xBAR0 + 2);
            mControllerID = aControllerId;
            mBusPosition = aBusPosition;
            IOPort.outb(IOControl, 0x02);

            DriveType = DiscoverDrive();

            if (DriveType == SpecLevel.Null)
            {
                Exists = false;
                return;
            }
            if(DriveType != SpecLevel.ATA)
            {
                Exists = false;
                return;
            }
            if (DriveType == SpecLevel.ATA)
            {
                SendCmd(Cmd.Identify);
            }
            else
            {
                SendCmd(Cmd.IdentifyPacket);
            }
            var xBuff = (ushort*)Heap.alloc(512);
            for (int i = 0; i < 256; i++)
            {
                ushort read = IOPort.inw(IOData);
                byte upper = (byte)(read >> 8);
                byte lower = (byte)read;
                xBuff[i] = (ushort)((lower << 8) | upper);
            }

            SerialNo = GetString(xBuff, 10, 20);
            FirmwareRev = GetString(xBuff, 23, 8);
            ModelNo = GetString(xBuff, 27, 40);
            DeviceLa = GetString(xBuff, 54, 40);
            uint l = 0;
            byte* ptr = DeviceLa;
            while(*ptr != 0)
            {
                ptr++;
                l++;
            }
            char[] tmp = new char[l];
            for(int i = 0; i < l; i++)
            {
                tmp[i] = (char)DeviceLa[i];
            }
            Label = new string(tmp);

            BlockCount = ((uint)xBuff[61] << 16 | xBuff[60]) - 1;
            LBA48Bit = (xBuff[83] & 0x40) != 0;
            if (LBA48Bit)
            {
                BlockCount = ((ulong)xBuff[102] << 32 | (ulong)xBuff[101] << 16 | (ulong)xBuff[100]) - 1;
            }
            byte* xMbrData = (byte*)Heap.alloc(512);
            ReadBlock(0, 1, xMbrData);
            MBR xMBR = new MBR();
            xMBR.Setup(this, xMbrData);

            PartitionCount = 0;
            for (int i = 0; i < xMBR.partitions.Count; i++)
            {
                ATA.ATAPartitions.Add(xMBR.partitions[i]);
                PartitionCount++;
            }
        }

        private void SelectDrive(byte aLbaHigh4)
        {
            IOPort.outb(IODeviceSelect, (byte)((byte)(DvcSelVal.Default | DvcSelVal.LBA | (mBusPosition == BusPositionEnum.Slave ? DvcSelVal.Slave : 0)) | aLbaHigh4));
            Wait();
        }

        private void Wait()
        {
            byte xVoid;
            xVoid = IOPort.inb(IOStatus);
            xVoid = IOPort.inb(IOStatus);
            xVoid = IOPort.inb(IOStatus);
            xVoid = IOPort.inb(IOStatus);
        }

        private SpecLevel DiscoverDrive()
        {
            SelectDrive(0);
            var xIdentifyStatus = SendCmd(Cmd.Identify);
            if (xIdentifyStatus == Status.None)
            {
                return SpecLevel.Null;
            }
            else if ((xIdentifyStatus & Status.Error) != 0)
            {
                int xTypeId = IOPort.inb(IOLBA2) << 8 | IOPort.inb(IOLBA1);
                if (xTypeId == 0xEB14 || xTypeId == 0x9669)
                {
                    return SpecLevel.ATAPI;
                }
                else
                {
                    return SpecLevel.Null;
                }
            }
            else if ((xIdentifyStatus & Status.DRQ) == 0)
            {
                return SpecLevel.Null;
            }
            return SpecLevel.ATA;
        }

        private Status SendCmd(Cmd aCmd)
        {
            IOPort.outb(IOCommand, (byte)aCmd);
            Status xStatus;
            do
            {
                Wait();
                xStatus = (Status)IOPort.inb(IOStatus);
            } while ((xStatus & Status.Busy) != 0);
            return xStatus;
        }

        private byte* GetString(ushort* aBuffer, int aIndexStart, int aStringLength)
        {
            byte* bfr = (byte*)aBuffer;
            var xChars = (byte*)Heap.alloc((uint)aStringLength);
            for (int i = 0; i < aStringLength; i++)
            {
                *xChars = bfr[aIndexStart + i];
                xChars++;
            }
            xChars -= aStringLength;
            Utils.strDepad(xChars, 40);
            return xChars;
        }

        private void SelectSector(ulong aSectorNo, ulong aSectorCount)
        {
            SelectDrive((byte)(aSectorNo >> 24));
            if (LBA48Bit)
            {
                IOPort.outw(IOSectorCount, (ushort)aSectorCount);
                IOPort.outb(IOLBA0, (byte)(aSectorNo >> 24));
                IOPort.outb(IOLBA0, (byte)(aSectorNo));
                IOPort.outb(IOLBA1, (byte)(aSectorNo >> 32));
                IOPort.outb(IOLBA1, (byte)(aSectorNo >> 8));
                IOPort.outb(IOLBA2, (byte)(aSectorNo >> 40));
                IOPort.outb(IOLBA2, (byte)(aSectorNo >> 16));
            }
            else
            {
                IOPort.outb(IOSectorCount, (byte)aSectorCount);
                IOPort.outb(IOLBA0, (byte)(aSectorNo));
                IOPort.outb(IOLBA1, (byte)(aSectorNo >> 8));
                IOPort.outb(IOLBA2, (byte)(aSectorNo >> 16));
            }
        }

        public void ReadBlock(ulong aBlockNo, ulong aBlockCount, byte* aData)
        {
            ushort* ptr = (ushort*)aData;
            Utils.memset(aData, 0, (uint)(512 * aBlockCount));
            SelectSector(aBlockNo, aBlockCount);
            SendCmd(LBA48Bit ? Cmd.ReadPioExt : Cmd.ReadPio);
            for (ulong i = 0; i < 256 * aBlockCount; i++)
            {
                ushort read = IOPort.inw(IOData);
                byte upper = (byte)(read >> 8);
                byte lower = (byte)read;
                ptr[i] = (ushort)((lower << 8) | upper);
                //ptr[i] = IOPort.inw(IOData);
            }
        }

        public void WriteBlock(ulong aBlockNo, ulong aBlockCount, byte* aData)
        {
            SelectSector(aBlockNo, aBlockCount);
            SendCmd(LBA48Bit ? Cmd.WritePioExt : Cmd.WritePio);

            ushort* ptr = (ushort*)aData;
            for (ulong i = 0; i < 512 * aBlockCount; i++)
            {
                IOPort.outw(IOData, ptr[i]);
            }
            Wait();

            SendCmd(Cmd.CacheFlush);
        }

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
            public List<ATAPartition> partitions;
            public uint EBRLocation;

            public void Setup(ATABlockDevice hd, byte* aMBR)
            {
                partitions = new List<ATAPartition>(4);
                ParsePartition(0, hd, aMBR, 446);
                ParsePartition(1, hd, aMBR, 462);
                ParsePartition(2, hd, aMBR, 478);
                ParsePartition(3, hd, aMBR, 494);
            }

            public void ParsePartition(uint no, ATABlockDevice hd, byte* aMBR, uint aLoc)
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

                    ATAPartition xPartInfo = new ATAPartition(hd, xStartSector, xSectorCount, no);
                    partitions.Add(xPartInfo);
                }
            }
        }
    }
}