using Kernel.System.IO;
using System.Runtime.InteropServices;

namespace Kernel.System.HAL
{
    public static unsafe class ACPI
    {
        private static RSD Rsd;
        public static uint[] ACPITable;
        private static byte* Facp = null;
        private static int* SMI_CMD;
        private static int* PM1a_CNT;
        private static int* PM1b_CNT;
        private static int SLP_TYPa;
        private static int SLP_TYPb;
        private static int SLP_EN;
        private static int SCI_EN;
        private static byte ACPI_ENABLE;
        private static byte ACPI_DISABLE;
        private static byte PM1_CNT_LEN;
        private static Cosmos.Core.IOPort smiIO;
        private static Cosmos.Core.IOPort pm1aIO;
        private static Cosmos.Core.IOPort pm1bIO;

        public static void Init()
        {
            uint rsdp = 0;
            for (uint addr = 0xE0000; addr < 0x100000; addr += 4)
            {
                if (CheckForRSDP(addr))
                {
                    rsdp = addr;
                }
            }
            if (rsdp == 0)
            {
                uint ebda_address = *((uint*)0x040E);

                ebda_address = (ebda_address * 0x10) & 0x000fffff;

                for (uint addr = ebda_address; addr < ebda_address + 1024; addr += 4)
                {
                    if (CheckForRSDP(addr))
                    {
                        rsdp = addr;
                    }
                }
            }
            if (rsdp != 0)
            {
                Rsd = *((RSD*)rsdp);
                acpi_table_header* Rsdt = (acpi_table_header*)Rsd.RsdtAddress;
                uint e = Rsdt->length;
                e = (e - 36) / 4;

                ACPITable = new uint[e];

                uint rsdt = Rsd.RsdtAddress;
                for (int j = 0; j < e; j++)
                {
                    ACPITable[j] = *(uint*)(rsdt + 36 + j * 4);
                    acpi_table_header* header = (acpi_table_header*)ACPITable[j];
                }

                SetupPower((byte*)rsdp);
                Enable();
            }
        }

        private static int Compare(string c1, byte* c2)
        {
            for (int i = 0; i < c1.Length; i++)
            {
                if (c1[i] != (char)c2[i]) { return -1; }
            }
            return 0;
        }

        private static int* facpget(int number)
        {

            if (number == 0) { return (int*)*((int*)(Facp + 40)); }
            else if (number == 1) { return (int*)*((int*)(Facp + 48)); }
            else if (number == 2) { return (int*)*((int*)(Facp + 64)); }
            else if (number == 3) { return (int*)*((int*)(Facp + 68)); }
            else { return null; }
        }

        private static byte facpbget(int number)
        {
            if (number == 0) { return *(Facp + 52); }
            else if (number == 1) { return *(Facp + 53); }
            else if (number == 2) { return *(Facp + 89); }
            else return 0;
        }

        private static void SetupPower(byte* ptr)
        {
            int addr = 0;
            for (int i = 19; i >= 16; i--)
            {
                addr += (*((byte*)ptr + i));
                addr = (i == 16) ? addr : addr << 8;
            }
            ptr = (byte*)addr;
            ptr += 4; addr = 0;
            for (int i = 3; i >= 0; i--)
            {
                addr += (*((byte*)ptr + i));
                addr = (i == 0) ? addr : addr << 8;
            }
            int length = addr;
            ptr -= 4;
            if (ptr != null && Compare("RSDT", (byte*)ptr) == 0)
            {
                addr = 0;
                int entrys = length;
                entrys = (entrys - 36) / 4;
                ptr += 36;
                byte* yeuse;

                while (0 < entrys--)
                {
                    for (int i = 3; i >= 0; i--)
                    {
                        addr += (*((byte*)ptr + i));
                        addr = (i == 0) ? addr : addr << 8;
                    }
                    yeuse = (byte*)addr;
                    Facp = (byte*)yeuse;
                    if (Compare("FACP", Facp) == 0)
                    {
                        if (Compare("DSDT", (byte*)facpget(0)) == 0)
                        {
                            byte* S5Addr = (byte*)facpget(0) + 36;
                            int dsdtLength = *(facpget(0) + 1) - 36;
                            while (0 < dsdtLength--)
                            {
                                if (Compare("_S5_", (byte*)S5Addr) == 0)
                                    break;
                                S5Addr++;
                            }
                            if (dsdtLength > 0)
                            {
                                if ((*(S5Addr - 1) == 0x08 || (*(S5Addr - 2) == 0x08 && *(S5Addr - 1) == '\\')) && *(S5Addr + 4) == 0x12)
                                {
                                    S5Addr += 5;
                                    S5Addr += ((*S5Addr & 0xC0) >> 6) + 2;
                                    if (*S5Addr == 0x0A)
                                        S5Addr++;
                                    SLP_TYPa = (short)(*(S5Addr) << 10);
                                    S5Addr++;
                                    if (*S5Addr == 0x0A)
                                        S5Addr++;
                                    SLP_TYPb = (short)(*(S5Addr) << 10);
                                    SMI_CMD = facpget(1);
                                    ACPI_ENABLE = facpbget(0);
                                    ACPI_DISABLE = facpbget(1);
                                    PM1a_CNT = facpget(2);
                                    PM1b_CNT = facpget(3);
                                    PM1_CNT_LEN = facpbget(3);
                                    SLP_EN = 1 << 13;
                                    SCI_EN = 1;
                                    smiIO = new Cosmos.Core.IOPort((ushort)SMI_CMD);
                                    pm1aIO = new Cosmos.Core.IOPort((ushort)PM1a_CNT);
                                    pm1bIO = new Cosmos.Core.IOPort((ushort)PM1b_CNT);
                                }
                            }
                        }
                    }
                    ptr += 4;
                }
            }
        }

        public static bool Enable()
        {
            if (pm1aIO.Word == 0)
            {
                if (SMI_CMD != null && ACPI_ENABLE != 0)
                {
                    smiIO.Byte = ACPI_ENABLE;
                    int i;
                    for (i = 0; i < 300; i++)
                    {
                        if ((pm1aIO.Word & 1) == 1)
                            break;
                    }
                    if (PM1b_CNT != null)
                        for (; i < 300; i++)
                        {
                            if ((pm1bIO.Word & 1) == 1)
                                break;
                        }
                    if (i < 300) return true;
                    else return false;
                }
                else return false;
            }
            else return true;
        }

        public static void Disable()
        {
            smiIO.Byte = ACPI_DISABLE;
        }

        public static void Shutdown()
        {
            pm1aIO.Word = (ushort)(SLP_TYPa | SLP_EN);
            if (PM1b_CNT != null)
                pm1bIO.Word = (ushort)(SLP_TYPb | SLP_EN);
        }

        public static void Reboot()
        {
            byte good = 0x02;
            while ((good & 0x02) != 0)
                good = IOPort.inb(0x64);
            IOPort.outb(0x64, 0xFE);
            Cosmos.Core.Global.CPU.Halt();
        }

        private static unsafe bool CheckForRSDP(uint addr)
        {
            byte* ch = (byte*)addr;

            if (*(ch++) != (byte)'R') return false;
            if (*(ch++) != (byte)'S') return false;
            if (*(ch++) != (byte)'D') return false;
            if (*(ch++) != (byte)' ') return false;
            if (*(ch++) != (byte)'P') return false;
            if (*(ch++) != (byte)'T') return false;
            if (*(ch++) != (byte)'R') return false;
            if (*(ch++) != (byte)' ') return false;

            byte sum = 0;
            byte* check = (byte*)addr;

            for (int i = 0; i < 20; i++)
                sum += *(check++);

            return (sum == 0);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct RSD
        {
            [FieldOffset(0)]
            public int Signature0;
            [FieldOffset(4)]
            public int Signature1;

            [FieldOffset(8)]
            public byte Checksum;

            [FieldOffset(9)]
            private byte OemID0;
            [FieldOffset(10)]
            private byte OemID1;
            [FieldOffset(11)]
            private byte OemID2;
            [FieldOffset(12)]
            private byte OemID3;
            [FieldOffset(13)]
            private byte OemID4;
            [FieldOffset(14)]
            private byte OemID5;

            [FieldOffset(15)]
            public byte Revision;
            [FieldOffset(16)]
            public uint RsdtAddress;

            public string Signature
            {
                get
                {
                    return "" + (char)Signature0 + (char)Signature1;
                }
            }
            public string OemID
            {
                get
                {
                    return "" + (char)OemID0 + (char)OemID1 + (char)OemID2 + (char)OemID3 + (char)OemID4 + (char)OemID5;
                }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct acpi_table_header
        {
            [FieldOffset(0)]
            public byte signature0;
            [FieldOffset(1)]
            public byte signature1;
            [FieldOffset(2)]
            public byte signature2;
            [FieldOffset(3)]
            public byte signature3;

            [FieldOffset(4)]
            public uint length;
            [FieldOffset(8)]
            public byte revision;
            [FieldOffset(9)]
            public byte checksum;

            [FieldOffset(10)]
            byte oem_id0;
            [FieldOffset(11)]
            byte oem_id1;
            [FieldOffset(12)]
            byte oem_id2;
            [FieldOffset(13)]
            byte oem_id3;
            [FieldOffset(14)]
            byte oem_id4;
            [FieldOffset(15)]
            byte oem_id5;

            [FieldOffset(16)]
            byte oem_table_id0;
            [FieldOffset(17)]
            byte oem_table_id1;
            [FieldOffset(18)]
            byte oem_table_id2;
            [FieldOffset(19)]
            byte oem_table_id3;
            [FieldOffset(20)]
            byte oem_table_id4;
            [FieldOffset(21)]
            byte oem_table_id5;
            [FieldOffset(22)]
            byte oem_table_id6;
            [FieldOffset(23)]
            byte oem_table_id7;

            [FieldOffset(24)]
            public uint oem_revision;
            [FieldOffset(28)]
            public uint asl_compiler_id;
            [FieldOffset(32)]
            public uint asl_compiler_revision;


            public string Signature
            {
                get
                {
                    return "" + (char)signature0 + (char)signature1 + (char)signature2 + (char)signature3;
                }
            }
            public string OemID
            {
                get
                {
                    return "" + (char)oem_id0 + (char)oem_id1 + (char)oem_id2 + (char)oem_id3 + (char)oem_id4 + (char)oem_id5;
                }
            }
            public string OemTable
            {
                get
                {
                    return "" + (char)oem_table_id0 + (char)oem_table_id1 + (char)oem_table_id2 + (char)oem_table_id3 + (char)oem_table_id4 + (char)oem_table_id5 + (char)oem_table_id6 + (char)oem_table_id7;
                }
            }
        }
    }
}