namespace Kernel.System.Hardware.PCI
{
    public abstract class PCIDevice : Device
    {
        public PCIType type;
        public ushort vendorId;
        public ushort deviceId;
        public byte classCode;
        public byte subclass;
        public byte progIntf;
        public byte intLine;
        public byte secondBus;
        public PCIHeaderType header;
        public string label;

        protected Cosmos.Core.IOGroup.PCI IO = new Cosmos.Core.IOGroup.PCI();
        public uint bus = 0;
        public uint slot = 0;
        public uint function = 0;

        public override string Name()
        {
            return label;
        }

        public PCIDevice(uint bus, uint slot, uint function)
        {
            this.bus = bus;
            this.slot = slot;
            this.function = function;
        }

        public override void Init()
        {
            vendorId = ReadRegister16(PCI.PCI_CONFIG_VENDOR_ID);
            deviceId = ReadRegister16(PCI.PCI_CONFIG_DEVICE_ID);
            progIntf = ReadRegister8(PCI.PCI_CONFIG_PROG_INTF);
            subclass = ReadRegister8(PCI.PCI_CONFIG_SUBCLASS);
            classCode = ReadRegister8(PCI.PCI_CONFIG_CLASS_CODE);
            intLine = ReadRegister8(PCI.PCI_CONFIG_INTERRUPT_LINE);
            secondBus = ReadRegister8(0x19);
            header = (PCIHeaderType)ReadRegister8(0x0E);
            label = PCI.GetDetails(vendorId, deviceId, classCode, subclass, progIntf, out type);

            ushort command = ReadRegister16(0x04);
            ushort flags = 0x0007;
            command |= flags;
            WriteRegister16(0x04, command);

            if (intLine != 255 && intLine != 0 && classCode != 6 && subclass != 4)
            {
                Console.WriteLine(label);
            }
        }

        protected uint GetAddressBase(uint aBus, uint aSlot, uint aFunction)
        {
            return (uint)(0x80000000 | (aBus << 16) | ((aSlot & 0x1F) << 11) | ((aFunction & 0x07) << 8));
        }

        public byte ReadRegister8(ushort aRegister)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | ((uint)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            return (byte)(IO.ConfigDataPort.DWord >> ((aRegister % 4) * 8) & 0xFF);
        }

        public void WriteRegister8(ushort aRegister, byte value)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | ((uint)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            IO.ConfigDataPort.Byte = value;
        }

        public ushort ReadRegister16(ushort aRegister)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | ((uint)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            return (ushort)(IO.ConfigDataPort.DWord >> ((aRegister % 4) * 8) & 0xFFFF); ;
        }

        public void WriteRegister16(ushort aRegister, ushort value)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | ((uint)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            IO.ConfigDataPort.Word = value;
        }

        public uint ReadRegister32(byte aRegister)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | ((uint)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            return IO.ConfigDataPort.DWord;
        }

        public void WriteRegister32(byte aRegister, uint value)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | ((uint)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            IO.ConfigDataPort.DWord = value;
        }
    }

    public enum PCIType
    {
        Processor,
        Input,
        Network,
        Storage,
        Graphics,
        Audio,
        Other
    }

    public enum PCIHeaderType : byte
    {
        Normal = 0x00,
        Bridge = 0x01,
        Cardbus = 0x02
    }
}