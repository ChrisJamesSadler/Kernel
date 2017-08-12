namespace Kernel.System.Core
{
    public static unsafe class Ensure
    {
        public static void Init()
        {
            bool skip = false;
            if (skip)
            {
                ISR.old_esp = 0;
                ISR.Registers = new ISR.IRQContext();
                ISR.CommonISRHandler(ref ISR.Registers);
            }
        }
    }
}