using Kernel.System.Drivers.Other;
using Kernel.System.Drivers.VMWare;
using Kernel.System.HAL;
using Kernel.System.HAL.PCI;
using Kernel.System.IO;
using System.Collections.Generic;

namespace Kernel.System.Drivers
{
    public static class Driver
    {
        public static List<IDriver> Drivers = new List<IDriver>(32);

        public static void Init()
        {
            Drivers.Add(new PS2Keyboard(null));
            Drivers.Add(new PS2Mouse(null));
            Drivers.Add(new VideoGraphicsArray(null));
            bool foundGPU = false;
            foreach(PCIDevice device in PCI.PCIDevices)
            {
                IDriver driver = null;
                if (device.type == PCIDevice.PCIType.Graphics)
                {
                    foundGPU = true;
                }
                switch (device.VendorID)
                {
                    case 0x1022: //AMD
                        switch (device.DeviceID)
                        {
                            case 0x2000:
                                break;//return "AMD PCnet LANCE PCI Ethernet Controller";
                        }
                        break;
                    case 0x104B: //Sony
                        switch (device.DeviceID)
                        {
                            case 0x1040:
                                break;//return "Mylex BT958 SCSI Host Adaptor";
                        }
                        break;
                    case 0x1274: //Ensoniq
                        switch (device.DeviceID)
                        {
                            case 0x1371:
                                break;//return "Ensoniq AudioPCI";
                        }
                        break;
                    case 0x15AD: //VMware
                        switch (device.DeviceID)
                        {
                            case 0x0405:
                                Drivers.Add(new NVIDIA9500MGS(device));
                                break;//return "VMware NVIDIA 9500MGS";
                            case 0x0770:
                                break;//return "VMware Standard Enhanced PCI to USB Host Controller";
                            case 0x0790:
                                break;//return "VMware 6.0 Virtual USB 2.0 Host Controller";
                            case 0x07A0:
                                break;//return "VMware PCI Express Root Port";
                        }
                        break;
                    case 0x8086: //Intel
                        switch (device.DeviceID)
                        {
                            case 0x7190:
                                break;//return "Intel 440BX/ZX AGPset Host Bridge";
                            case 0x7191:
                                break;//return "Intel 440BX/ZX AGPset PCI-to-PCI bridge";
                            case 0x7110:
                                break;//return "Intel PIIX4/4E/4M ISA Bridge";
                            case 0x7112:
                                break;//return "Intel PIIX4/4E/4M USB Interface";
                        }
                        break;
                }
                switch (device.ClassCode)
                {
                    case 0x01:
                        break;//return "Mass Storage Controller";
                    case 0x02:
                        break;//return "Network Controller";
                    case 0x03:
                        break;//return "Display Controller";
                    case 0x04:
                        break;//return "Multimedia Controller";
                    case 0x05:
                        break;//return "Memory Controller";
                    case 0x06:
                        break;//return "Bridge Device";
                    case 0x07:
                        break;//return "Simple Communication Controller";
                    case 0x08:
                        break;//return "Base System Peripheral";
                    case 0x09:
                        break;//return "Input Device";
                    case 0x0A:
                        break;//return "Docking Station";
                    case 0x0B:
                        break;//return "Processor";
                    case 0x0C:
                        break;//return "Serial Bus Controller";
                    case 0x0D:
                        break;//return "Wireless Controller";
                    case 0x0E:
                        break;//return "Intelligent I/O Controller";
                    case 0x0F:
                        break;//return "Satellite Communication Controller";
                    case 0x10:
                        break;//return "Encryption/Decryption Controller";
                    case 0x11:
                        break;//return "Data Acquisition and Signal Processing Controller";
                    case 0xFF:
                        break;//return "Unkown device";
                }
                if(driver != null)
                {
                    Drivers.Add(driver);
                }
            }
            if (!foundGPU)
            {
                IOPort.outw(0x01CE, 0x00);
                ushort read = IOPort.inw(0x01CF);
                if (read >= 0xB0C4 && read != 0xFFFF)
                {
                    Drivers.Add(new BochsGraphicsAdaptor(null));
                }
            }
        }
    }
}