using Kernel.System.IO;
using System;

namespace Kernel.System.Hardware.PCI.Devices
{
    public class VBE : Device
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

        public override string Name()
        {
            return "VBE";
        }

        public override void Init()
        {
            Config.EnterVideoMode = Init;
            Config.DisplayWidth = 1024;
            Config.DisplayHeight = 768;
            VBEWrite(VBERegisterIndex.VBEDisplayEnable, (ushort)VBEEnableValues.VBEDisabled);
            VBEWrite(VBERegisterIndex.VBEDisplayBPP, 32);
            VBEWrite(VBERegisterIndex.VBEDisplayXResolution, Config.DisplayWidth);
            VBEWrite(VBERegisterIndex.VBEDisplayYResolution, Config.DisplayHeight);
            VBEWrite(VBERegisterIndex.VBEDisplayEnable, (ushort)(VBEEnableValues.VBEEnabled | VBEEnableValues.VBEUseLinearFrameBuffer | VBEEnableValues.VBENoClearMemory));
        }

        public void VBEWrite(VBERegisterIndex index, uint value)
        {
            IOPort.outw(IndexPort, (ushort)index);
            IOPort.outd(DataPort, value);
        }
    }
}