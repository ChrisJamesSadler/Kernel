﻿using Kernel.System.Core;
using Kernel.System.IO;

namespace Kernel.System.HAL.ATA
{
    public unsafe class ATADiskDrive : DiskDrive
    {
        private ushort IOControl;
        private ushort IOCommand;
        private ushort IOData;
        private ushort IOStatus;
        private ushort IODeviceSelect;
        private ushort IOLBA0;
        private ushort IOLBA1;
        private ushort IOLBA2;
        private ushort IOSectorCount;
        public ATAControllerIdEnum mControllerID;
        public ATABusPositionEnum mBusPosition;
        public ATASpecLevel DriveType;
        public ulong BlockCount;
        public ulong BlockSize;
        public bool LBA48Bit;
        public byte* DeviceLa;
        public byte* SerialNo;
        public byte* FirmwareRev;
        public byte* ModelNo;

        public ATADiskDrive(bool primary, ATAControllerIdEnum aControllerId, ATABusPositionEnum aBusPosition, ATASpecLevel mDriveType)
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
            DriveType = mDriveType;

            if (DriveType == ATASpecLevel.ATA)
            {
                SendATACmd(ATACmd.Identify, IOCommand, IOStatus);
            }
            else
            {
                SendATACmd(ATACmd.IdentifyPacket, IOCommand, IOStatus);
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
            while (*ptr != 0)
            {
                ptr++;
                l++;
            }
            char[] tmp = new char[l];
            for (int i = 0; i < l; i++)
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
            
            for (int i = 0; i < xMBR.partitions.Count; i++)
            {
                DetectedPartitions.Add(xMBR.partitions[i]);
            }
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

        private ATAStatus SendCmd(ATACmd aCmd)
        {
            IOPort.outb(IOCommand, (byte)aCmd);
            ATAStatus xStatus;
            do
            {
                Wait();
                xStatus = (ATAStatus)IOPort.inb(IOStatus);
            } while ((xStatus & ATAStatus.Busy) != 0);
            return xStatus;
        }

        private void Wait()
        {
            byte xVoid;
            xVoid = IOPort.inb(IOStatus);
            xVoid = IOPort.inb(IOStatus);
            xVoid = IOPort.inb(IOStatus);
            xVoid = IOPort.inb(IOStatus);
        }

        private void SelectDrive(byte aLbaHigh4)
        {
            IOPort.outb(IODeviceSelect, (byte)((byte)(ATADvcSelVal.Default | ATADvcSelVal.LBA | (mBusPosition == ATABusPositionEnum.Slave ? ATADvcSelVal.Slave : 0)) | aLbaHigh4));
            Wait();
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

        public override void ReadBlock(ulong aBlockNo, ulong aBlockCount, byte* aData)
        {
            ushort* ptr = (ushort*)aData;
            Utils.memset(aData, 0, (uint)(512 * aBlockCount));
            SelectSector(aBlockNo, aBlockCount);
            SendCmd(LBA48Bit ? ATACmd.ReadPioExt : ATACmd.ReadPio);
            for (ulong i = 0; i < 256 * aBlockCount; i++)
            {
                ushort read = IOPort.inw(IOData);
                byte upper = (byte)(read >> 8);
                byte lower = (byte)read;
                ptr[i] = (ushort)((lower << 8) | upper);
            }
        }
    }
}