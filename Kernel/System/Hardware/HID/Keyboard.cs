using Kernel.System.IO;
using Kernel.System.Core;
using Kernel.System.Collections;
using System;

namespace Kernel.System.Hardware.HID
{
    public class Keyboard : Device
    {
        public static CircularBuffer buffer;
        private static byte[] states;
        private static string qwertyuiop = "qwertyuiop";
        private static string asdfghjkl = "asdfghjkl";
        private static string zxcvbnm = "zxcvbnm,./";
        private static string num = "1234567890";

        private static byte ONE_PRESSED = 0x02;
        private static byte ZERO_PRESSED = 0x0B;

        public override string Name()
        {
            return "Keyboard";
        }

        public override void Init()
        {
            buffer = new CircularBuffer(32);
            states = new byte[255];
            for (int i = 0; i < 255; i++)
            {
                states[i] = 0;
            }
            ISR.SetIntHandler(1, Handler);
        }

        private static void Handler()
        {
            byte scan = IOPort.inb(0x60);
            bool released = (scan & 128) == 128;
            if (!released)
            {
                if (states[scan] == 0)
                {
                    states[scan] = 1;
                    buffer.Push(scan);
                }
            }
            else
            {
                buffer.Push(scan);
                scan -= 128;
                states[scan] = 0;
            }
        }

        public char Decode(byte key)
        {
            if (key == 0x1C) return '\n';
            if (key == 0x39) return ' ';
            if (key == 0xE) return '\b';
            if (key >= ONE_PRESSED && key <= ZERO_PRESSED)
                return num[key - ONE_PRESSED];
            if (key >= 0x10 && key <= 0x1C)
            {
                return qwertyuiop[key - 0x10];
            }
            else if (key >= 0x1E && key <= 0x26)
            {
                return asdfghjkl[key - 0x1E];
            }
            else if (key >= 0x2C && key <= 0x35)
            {
                return zxcvbnm[key - 0x2C];
            }
            return (char)0;
        }
    }
}