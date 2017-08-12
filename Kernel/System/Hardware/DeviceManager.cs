using Kernel.System.Hardware.HID;
using Kernel.System.Hardware.PCI.Devices;
using Kernel.System.IO;
using System.Collections.Generic;

namespace Kernel.System.Hardware
{
    public static class DeviceManager
    {
        public static List<Device> AllDevices = new List<Device>(64);

        public static void Init()
        {
            #region Devices
            IOPort.outw(0x01CE, 0x00);
            DeviceManager.AllDevices.Add(new VGA());
            if (IOPort.inw(0x00) == 0xB0C5)
            {
                DeviceManager.AllDevices.Add(new VBE());
            }
            else
            {
                // add and detect SVGAII driver
            }
            AllDevices.Add(new Keyboard());
            AllDevices.Add(new Mouse());
            #endregion

            for (int i = 0; i < AllDevices.Count; i++)
            {
                AllDevices[i].Init();
            }
        }
    }
}