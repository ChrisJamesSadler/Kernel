using Kernel.System.Core;
using Kernel.System.IO;

namespace Kernel.System.HAL
{
    public static class RTC
    {
        public const ushort AddressPort = 0x70;
        public const ushort DataPort = 0x71;
        private static bool isBCDMode;
        private static bool is24HourMode;
        private static byte StatusByteB;

        public static void Init()
        {
            WaitForReady();
            IOPort.outb(AddressPort, 0x0B);
            StatusByteB = IOPort.inb(DataPort);

            if ((StatusByteB & 0x02) == 0x02)
            {
                is24HourMode = true;
            }
            else
            {
                is24HourMode = false;
            }

            if ((StatusByteB & 0x04) == 0x04)
            {
                isBCDMode = false;
            }
            else
            {
                isBCDMode = true;
            }
            IOPort.outb(0x70, 0x8B);
            byte prev = IOPort.inb(0x71);
            IOPort.outb(0x70, 0x8B);
            IOPort.outb(0x71, (byte)(prev | 0x40));
        }

        public static byte Second
        {
            get
            {
                WaitForReady();
                IOPort.outb(AddressPort, 0);
                if (isBCDMode)
                {
                    return FromBCD(IOPort.inb(DataPort));
                }
                else
                {
                    return IOPort.inb(DataPort);
                }
            }
        }

        public static byte Minute
        {
            get
            {
                WaitForReady();
                IOPort.outb(AddressPort, 2);
                if (isBCDMode)
                {
                    return FromBCD(IOPort.inb(DataPort));
                }
                else
                {
                    return IOPort.inb(DataPort);
                }
            }
        }

        public static byte Hour
        {
            get
            {
                WaitForReady();
                IOPort.outb(AddressPort, 4);
                if (isBCDMode)
                {
                    if (is24HourMode)
                    {
                        return FromBCD(IOPort.inb(DataPort));
                    }
                    else
                    {
                        byte b = IOPort.inb(DataPort);
                        if ((b & 0x80) == 0x80)
                        {
                            return (byte)(FromBCD(b) + 12);
                        }
                        else
                        {
                            if (FromBCD(b) == 12)
                            {
                                return 0;
                            }
                            else
                            {
                                return FromBCD(b);
                            }
                        }
                    }
                }
                else
                {
                    if (is24HourMode)
                    {
                        return IOPort.inb(DataPort);
                    }
                    else
                    {
                        byte b = IOPort.inb(DataPort);
                        if ((b & 0x80) == 0x80)
                        {
                            return (byte)(b + 12);
                        }
                        else
                        {
                            if (b == 12)
                            {
                                return 0;
                            }
                            else
                            {
                                return b;
                            }
                        }
                    }
                }
            }
        }

        private static void WaitForReady()
        {
            do
            {
                IOPort.outb(AddressPort, 10);
            }
            while ((IOPort.inb(DataPort) & 0x80) != 0);
        }

        private static byte FromBCD(byte value)
        {
            return (byte)(((value >> 4) & 0x0F) * 10 + (value & 0x0F));
        }
    }
}