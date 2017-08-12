using System.Collections.Generic;

namespace Kernel.System.HAL.PCI
{
    public static class PCI
    {
        public static List<PCIDevice> PCIDevices = new List<PCIDevice>(32);

        public static void Init()
        {
            EnumerateBus(0, 0);
            foreach(PCIDevice dev in PCIDevices)
            {
                Console.WriteLine(dev.Label);
            }
        }

        public static void EnumerateBus(uint xBus, uint step)
        {
            for (uint xDevice = 0; xDevice < 32; xDevice++)
            {
                PCIDevice xPCIDevice = new PCIDevice(xBus, xDevice, 0x00);
                if (xPCIDevice.DeviceExists)
                {
                    if (xPCIDevice.HeaderType == PCIDevice.PCIHeaderType.Bridge)
                    {
                        for (uint xFunction = 0; xFunction < 8; xFunction++)
                        {
                            xPCIDevice = new PCIDevice(xBus, xDevice, xFunction);
                            if (xPCIDevice.DeviceExists)
                            {
                                var dev = new PCIDeviceBridge(xBus, xDevice, xFunction);
                                PCIDevices.Add(dev);
                                EnumerateBus(dev.SecondaryBusNumber, step + 1);
                            }
                        }
                    }
                    else if(xPCIDevice.HeaderType == PCIDevice.PCIHeaderType.Normal)
                    {
                        PCIDevices.Add(new PCIDeviceNormal(xBus, xDevice, 0x00));
                    }
                }
            }
        }

        public static PCIDevice GetDevice(ushort VendorID, ushort DeviceID)
        {
            for (int i = 0; i < PCIDevices.Count; i++)
            {
                if (PCIDevices[i].VendorID == VendorID && PCIDevices[i].DeviceID == DeviceID)
                    return PCIDevices[i];
            }
            return null;
        }
    }
}