using Kernel.System.HAL;
using Kernel.System.HAL.PCI;
using Kernel.System.IO;
using Flags = System.FlagsAttribute;
using Math = System.Math;

namespace Kernel.System.Drivers.Other
{
    public unsafe class BochsGraphicsAdaptor : IDriver
    {
        public const ushort IndexPort = 0x01CE;
        public const ushort DataPort = 0x01CF;

        public enum VBERegisterIndex
        {
            VBEDisplayID = 0x00,
            VBEDisplayXResolution,
            VBEDisplayYResolution,
            VBEDisplayBPP,
            VBEDisplayEnable,
            VBEDisplayBankMode,
            VBEDisplayVirtualWidth,
            VBEDisplayVirtualHeight,
            VBEDisplayXOffset,
            VBEDisplayYOffset
        };

        [Flags]
        public enum VBEEnableValues
        {
            VBEDisabled = 0x00,
            VBEEnabled = 0x01,
            VBEUseLinearFrameBuffer = 0x40,
            VBENoClearMemory = 0x80,
        };

        public BochsGraphicsAdaptor(PCIDevice host) : base(host)
        {
            Label = "Bochs Graphics Adapter";
            Settings.EnterVideoMode = EnterGraphicsMode;
        }

        public void EnterGraphicsMode()
        {
            Settings.DisplayVRam = (uint*)0xE0000000;
            VBEWrite(VBERegisterIndex.VBEDisplayEnable, (ushort)VBEEnableValues.VBEDisabled);
            VBEWrite(VBERegisterIndex.VBEDisplayBPP, 32);
            VBEWrite(VBERegisterIndex.VBEDisplayXResolution, Settings.DisplayWidth);
            VBEWrite(VBERegisterIndex.VBEDisplayYResolution, Settings.DisplayHeight);
            VBEWrite(VBERegisterIndex.VBEDisplayEnable, (ushort)(VBEEnableValues.VBEEnabled | VBEEnableValues.VBEUseLinearFrameBuffer | VBEEnableValues.VBENoClearMemory));
        }

        public void VBEWrite(VBERegisterIndex index, int value)
        {
            IOPort.outw(IndexPort, (ushort)index);
            IOPort.outd(DataPort, (uint)value);
        }
    }
}