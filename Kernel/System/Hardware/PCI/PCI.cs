using Kernel.System.Hardware.PCI.Devices;
using Kernel.System.IO;

namespace Kernel.System.Hardware.PCI
{
    public static unsafe class PCI
    {
        public static ushort PCI_CONFIG_ADDR = 0xcf8;
        public static ushort PCI_CONFIG_DATA = 0xcfC;
        public static ushort PCI_TYPE_MULTIFUNC = 0x80;
        public static ushort PCI_CONFIG_VENDOR_ID = 0x00;
        public static ushort PCI_CONFIG_DEVICE_ID = 0x02;
        public static ushort PCI_CONFIG_COMMAND = 0x04;
        public static ushort PCI_CONFIG_STATUS = 0x06;
        public static ushort PCI_CONFIG_REVISION_ID = 0x08;
        public static ushort PCI_CONFIG_PROG_INTF = 0x09;
        public static ushort PCI_CONFIG_SUBCLASS = 0x0a;
        public static ushort PCI_CONFIG_CLASS_CODE = 0x0b;
        public static ushort PCI_CONFIG_CACHELINE_SIZE = 0x0c;
        public static ushort PCI_CONFIG_LATENCY = 0x0d;
        public static ushort PCI_CONFIG_HEADER_TYPE = 0x0e;
        public static ushort PCI_CONFIG_BIST = 0x0f;
        public static ushort PCI_CONFIG_INTERRUPT_LINE = 0x3c;
        public static ushort PCI_CONFIG_INTERRUPT_PIN = 0x3d;

        public static void Init()
        {
            Scan(0);
        }

        public static void Scan(uint bus)
        {
            for (uint dev = 0; dev < 32; dev++)
            {
                uint baseId = PCI_MAKE_ID(bus, dev, 0);
                ushort headerType = PciRead8(baseId, 0x0E);
                uint funcCount = (uint)(Utils.toBool(headerType & PCI_TYPE_MULTIFUNC) ? 8 : 1);

                for (uint func = 0; func < funcCount; ++func)
                {
                    Identify(bus, dev, func);
                }
            }
        }

        public static void Identify(uint bus, uint dev, uint func)
        {
            uint id = PCI_MAKE_ID(bus, dev, func);

            ushort vendor = PciRead16(id, PCI_CONFIG_VENDOR_ID);
            if (vendor == 0xffff)
            {
                return;
            }
            PCIDevice device = new PCIDevice(bus, dev, func);
            if (device.header == PCIHeaderType.Bridge)
            {
                Scan(device.secondBus);
            }
            if (device.deviceId != 0x7A0)
            {
                DeviceManager.AllDevices.Add(device);
            }
        }

        private static uint PCI_MAKE_ID(uint bus, uint dev, uint func)
        {
            return ((bus) << 16) | ((dev) << 11) | ((func) << 8);
        }

        private static byte PciRead8(uint id, uint reg)
        {
            uint addr = 0x80000000 | id | (reg & 0xfc);
            IOPort.outd(PCI_CONFIG_ADDR, addr);
            return IOPort.inb((ushort)(PCI_CONFIG_DATA + (reg & 0x03)));
        }

        private static ushort PciRead16(uint id, uint reg)
        {
            uint addr = 0x80000000 | id | (reg & 0xfc);
            IOPort.outd(PCI_CONFIG_ADDR, addr);
            return IOPort.inw((ushort)(PCI_CONFIG_DATA + (reg & 0x02)));
        }

        public static string GetDetails(uint VendorID, uint DeviceID, uint ClassCode, uint Subclass, uint ProgIF, out PCIType type)
        {
            switch (VendorID)
            {
                case 0x1022: //AMD
                    switch (DeviceID)
                    {
                        case 0x2000:
                            type = PCIType.Network;
                            return "AMD PCnet LANCE PCI Ethernet Controller";
                    }
                    break;
                case 0x104B: //Sony
                    switch (DeviceID)
                    {
                        case 0x1040:
                            type = PCIType.Storage;
                            return "Mylex BT958 SCSI Host Adaptor";
                    }
                    break;
                case 0x1274: //Ensoniq
                    switch (DeviceID)
                    {
                        case 0x1371:
                            type = PCIType.Audio;
                            return "Ensoniq AudioPCI";
                    }
                    break;
                case 0x15AD: //VMware
                    switch (DeviceID)
                    {
                        case 0x0405:
                            type = PCIType.Graphics;
                            return "VMware NVIDIA 9500MGS";
                        case 0x0770:
                            type = PCIType.Storage;
                            return "VMware Standard Enhanced PCI to USB Host Controller";
                        case 0x0790:
                            type = PCIType.Storage;
                            return "VMware 6.0 Virtual USB 2.0 Host Controller";
                        case 0x07A0:
                            type = PCIType.Storage;
                            return "VMware PCI Express Root Port";
                    }
                    break;
                case 0x8086: //Intel
                    switch (DeviceID)
                    {
                        case 0x7190:
                            type = PCIType.Other;
                            return "Intel 440BX/ZX AGPset Host Bridge";
                        case 0x7191:
                            type = PCIType.Other;
                            return "Intel 440BX/ZX AGPset PCI-to-PCI bridge";
                        case 0x7110:
                            type = PCIType.Other;
                            return "Intel PIIX4/4E/4M ISA Bridge";
                        case 0x7112:
                            type = PCIType.Other;
                            return "Intel PIIX4/4E/4M USB Interface";
                    }
                    break;
            }

            switch (ClassCode)
            {
                //case 0x00:
                //    return "Any device";
                case 0x01:
                    type = PCIType.Storage;
                    return "Mass Storage Controller";
                case 0x02:
                    type = PCIType.Network;
                    return "Network Controller";
                case 0x03:
                    type = PCIType.Graphics;
                    return "Display Controller";
                case 0x04:
                    type = PCIType.Other;
                    return "Multimedia Controller";
                case 0x05:
                    type = PCIType.Other;
                    return "Memory Controller";
                case 0x06:
                    type = PCIType.Other;
                    return "Bridge Device";
                case 0x07:
                    type = PCIType.Other;
                    return "Simple Communication Controller";
                case 0x08:
                    type = PCIType.Other;
                    return "Base System Peripheral";
                case 0x09:
                    type = PCIType.Input;
                    return "Input Device";
                case 0x0A:
                    type = PCIType.Other;
                    return "Docking Station";
                case 0x0B:
                    type = PCIType.Processor;
                    return "Processor";
                case 0x0C:
                    type = PCIType.Other;
                    return "Serial Bus Controller";
                case 0x0D:
                    type = PCIType.Network;
                    return "Wireless Controller";
                case 0x0E:
                    type = PCIType.Other;
                    return "Intelligent I/O Controller";
                case 0x0F:
                    type = PCIType.Other;
                    return "Satellite Communication Controller";
                case 0x10:
                    type = PCIType.Other;
                    return "Encryption/Decryption Controller";
                case 0x11:
                    type = PCIType.Other;
                    return "Data Acquisition and Signal Processing Controller";
                case 0xFF:
                    type = PCIType.Other;
                    return "Unkown device";
            }
            type = PCIType.Other;
            return "ClassCode: " + ClassCode + "     Subclass: " + Subclass + "     ProgIF: " + ProgIF;
        }
    }
}
