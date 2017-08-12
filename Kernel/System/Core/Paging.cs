using Kernel.System.Collections;
using Kernel.System.HAL;
using Flags = System.FlagsAttribute;

namespace Kernel.System.Core
{
    public unsafe static class Paging
    {
        [Flags]
        private enum PTEFlags : uint
        {
            None = 0x0,
            Present = 0x1,
            Writeable = 0x2,
            UserAllowed = 0x4,
            PageLevelWriteThrough = 0x8,
            PageLevelCacheDisable = 0x10,
            Size_FourMiBPages = 0x80,
            Global = 0x100
        }

        public static uint PAGE_S = 0x400000;
        public static uint EMPTY_TAB = 0x00000002;
        public static uint* current_vpage_dir = null;
        public static uint* root_vpage_dir = null;

        public static void Init()
        {
            return;
            root_vpage_dir = mk_vpage_dir();
            for (int i = 0; i < 1024; i++)
            {
                idpaging(root_vpage_dir, 0, (int)Heap.heapPointer);
                root_vpage_dir++;
            }

            ISR.SetRealHandler(14, vpage_fault);
            switch_vpage_dir(root_vpage_dir);
        }

        private static void idpaging(uint* first_pte, uint from, int size)
        {
            from = from & 0xfffff000; // discard bits we don't want
            for (; size > 0; from += 4096, size -= 4096, first_pte++)
            {
                *first_pte = from | 1;     // mark page present.
            }
        }

        public static uint* mk_vpage_dir()
        {
            uint* dir = (uint*)PageManager.GetFreePage();

            int i;
            for (i = 0; i < 1024; i++)
                dir[i] = EMPTY_TAB;

            return dir;
        }

        public static uint* mk_vpage_table()
        {
            uint* tab = (uint*)PageManager.GetFreePage();
            int i;
            for (i = 0; i < 1024; i++)
            {
                tab[i] = (int)PTEFlags.Writeable;
            }
            return tab;
        }

        public static void vpage_map(uint* dir, uint phys, uint virt)
        {
            uint id = virt >> 22;
            uint* tab = mk_vpage_table();
            dir[id] = (uint)tab | 3;
            int i;
            for (i = 0; i < 1024; i++)
            {
                tab[i] += (uint)((phys >> 12) & 0x03FF);
                tab[i] &= (uint)PTEFlags.Present;
                phys += 4096;
            }
        }

        public static void switch_vpage_dir(void* dir)
        {
            CPU.WriteCR3((uint)dir);
            CPU.WriteCR0(CPU.ReadCR0() | 0x80000000);
        }

        public static void vpage_fault()
        {
            int err_pos = (int)CPU.ReadCR2();
            Console.Write("Page fault occurred at ");
            Console.Write(err_pos, 'h');

            Console.Write("\nReasons:");

            uint no_page = ISR.Registers.Param & 1;
            uint rw = ISR.Registers.Param & 2;
            uint um = ISR.Registers.Param & 4;
            uint re = ISR.Registers.Param & 8;
            uint dc = ISR.Registers.Param & 16;

            if (dc != 0) Console.Write(" (Instruction decode error) ");
            if (no_page == 0) Console.Write(" (No page present) ");
            if (um != 0) Console.Write(" (in user mode) ");
            if (rw != 0) Console.Write(" (Write permissions) ");
            if (re != 0) Console.Write(" (RE) ");
            Console.NewLine();
            while (true) { }
            //Console.Write("\n");
            //ProgramManager.CurrentThread.state = Thread.THREAD_STATES.DEAD;
            //Scheduler.Handler();
        }
    }
}