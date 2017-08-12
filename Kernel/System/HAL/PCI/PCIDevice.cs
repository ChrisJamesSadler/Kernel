using System;

namespace Kernel.System.HAL.PCI
{
    public class PCIDevice
    {
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

        [Flags]
        public enum PCIBist : byte
        {
            CocdMask = 0x0f,   /* Return result */
            Start = 0x40,   /* 1 to start BIST, 2 secs or less */
            Capable = 0x80    /* 1 if BIST capable */
        }

        [Flags]
        public enum PCICommand : short
        {
            IO = 0x1,     /* Enable response in I/O space */
            Memory = 0x2,     /* Enable response in Memory space */
            Master = 0x4,     /* Enable bus mastering */
            Special = 0x8,     /* Enable response to special cycles */
            Invalidate = 0x10,    /* Use memory write and invalidate */
            VGA_Pallete = 0x20,   /* Enable palette snooping */
            Parity = 0x40,    /* Enable parity checking */
            Wait = 0x80,    /* Enable address/data stepping */
            SERR = 0x100,   /* Enable SERR */
            Fast_Back = 0x200,   /* Enable back-to-back writes */
        }

        [Flags]
        public enum PCIStatus : int
        {
            CAP_LIST = 0x10,   /* Support Capability List */
            SUPPORT_66MHZ = 0x20,    /* Support 66 Mhz PCI 2.1 bus */
            UDF = 0x40,    /* Support User Definable Features [obsolete] */
            FAST_BACK = 0x80,    /* Accept fast-back to back */
            PARITY = 0x100,   /* Detected parity error */
            DEVSEL_MASK = 0x600,   /* DEVSEL timing */
            DEVSEL_FAST = 0x000,
            DEVSEL_MEDIUM = 0x200,
            DEVSEL_SLOW = 0x400,
            SIG_TARGET_ABORT = 0x800, /* Set on target abort */
            REC_TARGET_ABORT = 0x1000, /* Master ack of " */
            REC_MASTER_ABORT = 0x2000, /* Set on master abort */
            SIG_SYSTEM_ERROR = 0x4000, /* Set when we drive SERR */
            DETECTED_PARITY = 0x8000 /* Set on parity error */
        }

        public enum PCIInterruptPIN : byte
        {
            None = 0x00,
            INTA = 0x01,
            INTB = 0x02,
            INTC = 0x03,
            INTD = 0x04
        }

        public PCIType type;

        public ushort VendorID { get; private set; }
        public ushort DeviceID { get; private set; }

        public PCICommand Command { get { return (PCICommand)ReadRegister16(0x04); } set { WriteRegister16(0x04, (ushort)value); } }
        public PCIStatus Status { get { return (PCIStatus)ReadRegister16(0x06); } set { WriteRegister16(0x06, (ushort)value); } }

        public byte RevisionID { get; private set; }
        public byte ProgIF { get; private set; }
        public byte Subclass { get; private set; }
        public byte ClassCode { get; private set; }

        public byte CacheLineSize { get; private set; }
        public byte LatencyTimer { get; private set; }
        public PCIHeaderType HeaderType { get; private set; }
        public PCIBist BIST { get; private set; }

        public byte InterruptLine { get; private set; }
        public PCIInterruptPIN InterruptPIN { get; private set; }

        public bool DeviceExists { get; private set; }

        public uint bus = 0;
        public uint slot = 0;
        public uint function = 0;

        protected Cosmos.Core.IOGroup.PCI IO = new Cosmos.Core.IOGroup.PCI();

        public string Label;

        public PCIDevice(uint bus, uint slot, uint function)
        {
            this.bus = bus;
            this.slot = slot;
            this.function = function;

            VendorID = ReadRegister16(0x00);
            DeviceID = ReadRegister16(0x02);

            RevisionID = ReadRegister8(0x08);
            ProgIF = ReadRegister8(0x09);
            Subclass = ReadRegister8(0x0A);
            ClassCode = ReadRegister8(0x0B);

            CacheLineSize = ReadRegister8(0x0C);
            LatencyTimer = ReadRegister8(0x0D);
            HeaderType = (PCIHeaderType)ReadRegister8(0x0E);
            BIST = (PCIBist)ReadRegister8(0x0F);

            InterruptLine = ReadRegister8(0x3C);
            InterruptPIN = (PCIInterruptPIN)ReadRegister8(0x3D);

            DeviceExists = (uint)VendorID != 0xFFFF && (uint)DeviceID != 0xFFFF;

            Label = GetDetails(VendorID, DeviceID, ClassCode, Subclass, ProgIF, out type);
        }

        protected uint GetAddressBase(uint aBus, uint aSlot, uint aFunction)
        {
            return (uint)(0x80000000 | (aBus << 16) | ((aSlot & 0x1F) << 11) | ((aFunction & 0x07) << 8));
        }

        public void EnableMemory(bool enable)
        {
            UInt16 command = ReadRegister16(0x04);

            UInt16 flags = 0x0007;

            if (enable)
                command |= flags;
            else
                command &= (ushort)~flags;

            WriteRegister16(0x04, command);
        }

        #region IOReadWrite
        protected byte ReadRegister8(byte aRegister)
        {
            UInt32 xAddr = GetAddressBase(bus, slot, function) | ((UInt32)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            return (byte)(IO.ConfigDataPort.DWord >> ((aRegister % 4) * 8) & 0xFF);
        }

        protected void WriteRegister8(byte aRegister, byte value)
        {
            UInt32 xAddr = GetAddressBase(bus, slot, function) | ((UInt32)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            IO.ConfigDataPort.Byte = value;
        }

        protected UInt16 ReadRegister16(byte aRegister)
        {
            UInt32 xAddr = GetAddressBase(bus, slot, function) | ((UInt32)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            return (UInt16)(IO.ConfigDataPort.DWord >> ((aRegister % 4) * 8) & 0xFFFF); ;
        }

        protected void WriteRegister16(byte aRegister, ushort value)
        {
            UInt32 xAddr = GetAddressBase(bus, slot, function) | ((UInt32)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            IO.ConfigDataPort.Word = value;
        }

        protected UInt32 ReadRegister32(byte aRegister)
        {
            UInt32 xAddr = GetAddressBase(bus, slot, function) | ((UInt32)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            return IO.ConfigDataPort.DWord;
        }

        protected void WriteRegister32(byte aRegister, uint value)
        {
            UInt32 xAddr = GetAddressBase(bus, slot, function) | ((UInt32)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            IO.ConfigDataPort.DWord = value;
        }
        #endregion

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