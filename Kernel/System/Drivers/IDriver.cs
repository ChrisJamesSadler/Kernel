using Kernel.System.HAL;
using Kernel.System.HAL.PCI;

namespace Kernel.System.Drivers
{
    public abstract class IDriver
    {
        public string Label;
        public PCIDevice device;

        public IDriver(PCIDevice host)
        {
            device = host;
        }
    }
}