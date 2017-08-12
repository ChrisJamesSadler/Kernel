using Kernel.System.Core;
using Kernel.System.IO;
using static Kernel.System.Hardware.ATA.ATA;

namespace Kernel.System.Hardware.ATA
{
    public unsafe class BlockDevice : Device
    {
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
        public ATA.ControllerIdEnum mControllerID;
        public ATA.BusPositionEnum mBusPosition;
        public ATA.SpecLevel DriveType;
        public bool LBA48Bit;
        public byte* SerialNo;
        public byte* FirmwareRev;
        public byte* ModelNo;

        public override string Name()
        {
            return "Block Device";
        }

        public byte* NewBlockArray(ulong aBlockCount)
        {
            return (byte*)Heap.alloc((uint)(aBlockCount * BlockSize));
        }

        public void Setup(Cosmos.Core.IOGroup.ATA aIO, ATA.ControllerIdEnum aControllerId, ATA.BusPositionEnum aBusPosition)
        {
            IOControl = aIO.Control.Port;
            IOCommand = aIO.Command.Port;
            IOData = aIO.Data.Port;
            IOStatus = aIO.Status.Port;
            IODeviceSelect = aIO.DeviceSelect.Port;
            IOLBA0 = aIO.LBA0.Port;
            IOLBA1 = aIO.LBA1.Port;
            IOLBA2 = aIO.LBA2.Port;
            IOSectorCount = aIO.SectorCount.Port;
            mControllerID = aControllerId;
            mBusPosition = aBusPosition;
            IOPort.outb(IOControl, 0x02);

            DriveType = DiscoverDrive();
        }

        private void SelectDrive(byte aLbaHigh4)
        {
            IOPort.outb(IODeviceSelect, (byte)((byte)(ATA.DvcSelVal.Default | ATA.DvcSelVal.LBA | (mBusPosition == ATA.BusPositionEnum.Slave ? ATA.DvcSelVal.Slave : 0)) | aLbaHigh4));
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

        private ATA.SpecLevel DiscoverDrive()
        {
            SelectDrive(0);
            var xIdentifyStatus = SendCmd(ATA.Cmd.Identify);
            if (xIdentifyStatus == ATA.Status.None)
            {
                return ATA.SpecLevel.Null;
            }
            else if ((xIdentifyStatus & ATA.Status.Error) != 0)
            {
                int xTypeId = IOPort.inb(IOLBA2) << 8 | IOPort.inb(IOLBA1);
                if (xTypeId == 0xEB14 || xTypeId == 0x9669)
                {
                    return ATA.SpecLevel.ATAPI;
                }
                else
                {
                    return ATA.SpecLevel.Null;
                }
            }
            else if ((xIdentifyStatus & ATA.Status.DRQ) == 0)
            {
                return ATA.SpecLevel.Null;
            }
            return ATA.SpecLevel.ATA;
        }

        private ATA.Status SendCmd(ATA.Cmd aCmd)
        {
            IOPort.outb(IOCommand, (byte)aCmd);
            ATA.Status xStatus;
            do
            {
                Wait();
                xStatus = (ATA.Status)IOPort.inb(IOStatus);
            } while ((xStatus & ATA.Status.Busy) != 0);
            return xStatus;
        }

        public override void Init()
        {
            if (DriveType == ATA.SpecLevel.Null)
            {
                return;
            }
            if (DriveType == ATA.SpecLevel.ATA)
            {
                SendCmd(ATA.Cmd.Identify);
            }
            else
            {
                SendCmd(ATA.Cmd.IdentifyPacket);
            }
            var xBuff = (ushort*)Heap.alloc(512);
            for (int i = 0; i < 256; i++)
            {
                xBuff[i] = IOPort.inw(IOData);
            }
            SerialNo = GetString(xBuff, 10, 20);
            FirmwareRev = GetString(xBuff, 23, 8);
            ModelNo = GetString(xBuff, 27, 40);
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

            int parts = 0;
            for (int i = 0; i < xMBR.partitions.Length; i++)
            {
                if (xMBR.partitions[i] != null)
                {
                    DeviceManager.AllDevices.Add(xMBR.partitions[i]);
                    parts++;
                }
            }
            Console.Write(mControllerID == ControllerIdEnum.Primary ? "Primary" : "Secondary");
            Console.Write(" drive on ");
            Console.Write(mBusPosition == BusPositionEnum.Master ? "Master" : "Slave");
            Console.Write(" bus with ");
            Console.Write(parts);
            Console.WriteLine(" partitions");
        }

        private byte* GetString(ushort* aBuffer, int aIndexStart, int aStringLength)
        {
            var xChars = (byte*)Heap.alloc((uint)aStringLength);
            for (int i = 0; i < aStringLength / 2; i++)
            {
                ushort xChar = aBuffer[aIndexStart + i];
                xChars[i * 2] = (byte)(xChar >> 8);
                xChars[i * 2 + 1] = (byte)xChar;
            }
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
            SendCmd(LBA48Bit ? ATA.Cmd.ReadPioExt : ATA.Cmd.ReadPio);
            for (ulong i = 0; i < 256 * aBlockCount; i++)
            {
                ptr[i] = IOPort.inw(IOData);
            }
        }

        public void WriteBlock(ulong aBlockNo, ulong aBlockCount, byte* aData)
        {
            SelectSector(aBlockNo, aBlockCount);
            SendCmd(LBA48Bit ? ATA.Cmd.WritePioExt : ATA.Cmd.WritePio);

            ushort* ptr = (ushort*)aData;
            for (ulong i = 0; i < 512 * aBlockCount; i++)
            {
                IOPort.outw(IOData, ptr[i]);
            }
            Wait();

            SendCmd(ATA.Cmd.CacheFlush);
        }
    }
}