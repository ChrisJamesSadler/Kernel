using Kernel.System.IO;
using Kernel.System.Threading;

namespace Kernel.System.Components
{
    public static unsafe class Speaker
    {
        public static void sound(uint nFrequence)
        {
            if(nFrequence == 0)
            {
                nFrequence = 1;
            }
            uint Div;
            byte tmp;
            Div = 1193180 / nFrequence;
            IOPort.outb(0x43, 0xb6);
            IOPort.outb(0x42, (byte)(Div));
            IOPort.outb(0x42, (byte)(Div >> 8));
            tmp = IOPort.inb(0x61);
            if (tmp != (tmp | 3))
            {
                IOPort.outb(0x61, (byte)(tmp | 3));
            }
        }

        public static void nosound()
        {
            byte tmp = (byte)(IOPort.inb(0x61) & 0xFC);
            IOPort.outb(0x61, tmp);
        }

        public static void beep()
        {
            sound(1000);
            Thread.Sleep(10);
            nosound();
        }
    }
}