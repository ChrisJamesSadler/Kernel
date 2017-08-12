using Kernel.System.HAL;
using Kernel.System.IO;
using Kernel.System.Core;
using Kernel.System.HAL.PCI;

namespace Kernel.System.Drivers.Other
{
    public class PS2Mouse : IDriver
    {
        public const ushort p60 = 0x60;
        public const ushort p64 = 0x64;
        private static byte[] mouse_byte = new byte[4];
        private static byte mouse_cycle = 0;

        public enum MouseState
        {
            None = 0,
            Left = 1,
            Right = 2,
            Middle = 4
        }

        public PS2Mouse(PCIDevice host) : base(host)
        {
            Label = "PS2 Mouse";
            WaitSignal();
            IOPort.outb(p64, 0xA8);
            WaitSignal();
            IOPort.outb(p64, 0x20);
            WaitData();
            byte status = (byte)(IOPort.inb(p60) | 2);
            WaitSignal();
            IOPort.outb(p64, 0x60);
            WaitSignal();
            IOPort.outb(p60, status);
            Write(0xF6);
            Read();
            Write(0xF4);
            Read();
            ISR.SetIntHandler(12, HandleMouse);
        }

        private static byte Read()
        {
            WaitData();
            return IOPort.inb(p60);
        }

        private static void Write(byte b)
        {
            WaitSignal();
            IOPort.outb(p64, 0xD4);
            WaitSignal();
            IOPort.outb(p60, b);
        }

        private static void WaitData()
        {
            for (int i = 0; i < 100 & ((IOPort.inb(p64) & 1) == 1); i++) ;
        }

        private static void WaitSignal()
        {
            for (int i = 0; i < 100 & ((IOPort.inb(p64) & 2) != 0); i++) ;
        }

        public static void HandleMouse()
        {
            switch (mouse_cycle)
            {
                case 0:
                    mouse_byte[0] = Read();
                    if ((mouse_byte[0] & 0x8) == 0x8)
                        mouse_cycle++;

                    break;
                case 1:
                    mouse_byte[1] = Read();
                    mouse_cycle++;
                    break;
                case 2:
                    mouse_byte[2] = Read();
                    mouse_cycle = 0;

                    if (mouse_byte[1] > 128)
                    {
                        Settings.CursorX -= (byte)(256 - mouse_byte[1]);
                    }
                    else
                    {
                        Settings.CursorX += mouse_byte[1];
                    }
                    if (mouse_byte[2] > 128)
                    {
                        Settings.CursorY += (byte)(256 - mouse_byte[2]);
                    }
                    else
                    {
                        Settings.CursorY -= mouse_byte[2];
                    }

                    if (Settings.CursorX < 0)
                        Settings.CursorX = 0;
                    else if (Settings.CursorX > Settings.DisplayWidth - 1)
                        Settings.CursorX = Settings.DisplayWidth - 1;

                    if (Settings.CursorY < 0)
                        Settings.CursorY = 0;
                    else if (Settings.CursorY > Settings.DisplayHeight - 1)
                        Settings.CursorY = Settings.DisplayHeight - 1;

                    Settings.CursorState = (MouseState)(mouse_byte[0] & 0x7);
                    break;
            }

        }
    }
}