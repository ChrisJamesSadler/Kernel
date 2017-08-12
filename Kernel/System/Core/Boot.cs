using Kernel.System.Core;
using Sys = Cosmos.System;
using Kernel.System.Threading;
using Kernel.System.HAL;
using Kernel.System.Drivers;
using Kernel.Application;
using Kernel.System.Events;
using Kernel.System.Audio;
using Kernel.System.HAL.PCI;

namespace Kernel.System.Boot
{
    public unsafe class Boot : Sys.Kernel
    {
        public override void MainMethod()
        {
            Ensure.Init();
            CPU.DisableInts();
            GDT.Init();
            IDT.Init();
            ISR.Init();
            Console.Clear();
            ACPI.Init();
            ACPI.Enable();
            PageManager.Init();
            Heap.Init();
            GC.Init();
            Paging.Init();
            PIT.Init();
            RTC.Init();
            Scheduler.Init();
            CPU.EnableInts();
            DiskDrive.Init();
            //FatFileSystem.Init();
            PCI.Init();
            Driver.Init();
            AudioMixer.Init();

            new Thread(Event.Init, "Events").Start();
            new Thread(new SCI().Main, "Shell").Start();

            Scheduler.Idle();
        }
    }
}