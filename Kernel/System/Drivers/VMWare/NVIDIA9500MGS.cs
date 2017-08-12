using Kernel.System.HAL;
using Kernel.System.IO;
using Math = System.Math;
using Flags = System.FlagsAttribute;
using Kernel.System.Drawing;
using Kernel.System.HAL.PCI;

namespace Kernel.System.Drivers.VMWare
{
    public unsafe class NVIDIA9500MGS : IDriver
    {
        public enum Register : ushort
        {
            ID = 0,
            Enable = 1,
            Width = 2,
            Height = 3,
            MaxWidth = 4,
            MaxHeight = 5,
            Depth = 6,
            BitsPerPixel = 7,
            PseudoColour = 8,
            RedMask = 9,
            GreenMask = 10,
            BlueMask = 11,
            BytesPerLine = 12,
            FrameBufferStart = 13,
            FrameBufferOffset = 14,
            VRamSize = 15,
            FrameBufferSize = 16,

            Capabilities = 17,
            MemStart = 18,
            MemSize = 19,
            ConfigDone = 20,
            Sync = 21,
            Busy = 22,
            GuestID = 23,
            CursorID = 24,
            CursorX = 25,
            CursorY = 26,
            CursorOn = 27,
            HostBitsPerPixel = 28,
            ScratchSize = 29,
            MemRegs = 30,
            NumDisplays = 31,
            PitchLock = 32,

            /// <summary>
            /// Indicates maximum size of FIFO Registers.
            /// </summary>
            FifoNumRegisters = 293
        }

        private enum ID : uint
        {
            Magic = 0x900000,
            V0 = Magic << 8,
            V1 = (Magic << 8) | 1,
            V2 = (Magic << 8) | 2,
            Invalid = 0xFFFFFFFF
        }

        public enum FIFO : uint
        {	// values are multiplied by 4 to access the array by byte index
            Min = 0,
            Max = 4,
            NextCmd = 8,
            Stop = 12
        }

        private enum FIFOCommand
        {
            Update = 1,
            RECT_FILL = 2,
            RECT_COPY = 3,
            DEFINE_BITMAP = 4,
            DEFINE_BITMAP_SCANLINE = 5,
            DEFINE_PIXMAP = 6,
            DEFINE_PIXMAP_SCANLINE = 7,
            RECT_BITMAP_FILL = 8,
            RECT_PIXMAP_FILL = 9,
            RECT_BITMAP_COPY = 10,
            RECT_PIXMAP_COPY = 11,
            FREE_OBJECT = 12,
            RECT_ROP_FILL = 13,
            RECT_ROP_COPY = 14,
            RECT_ROP_BITMAP_FILL = 15,
            RECT_ROP_PIXMAP_FILL = 16,
            RECT_ROP_BITMAP_COPY = 17,
            RECT_ROP_PIXMAP_COPY = 18,
            DEFINE_CURSOR = 19,
            DISPLAY_CURSOR = 20,
            MOVE_CURSOR = 21,
            DEFINE_ALPHA_CURSOR = 22
        }

        private enum IOPortOffset : byte
        {
            Index = 0,
            Value = 1,
            Bios = 2,
            IRQ = 3
        }

        [Flags]
        private enum Capability
        {
            None = 0,
            RectFill = 1,
            RectCopy = 2,
            RectPatFill = 4,
            LecacyOffscreen = 8,
            RasterOp = 16,
            Cursor = 32,
            CursorByPass = 64,
            CursorByPass2 = 128,
            EigthBitEmulation = 256,
            AlphaCursor = 512,
            Glyph = 1024,
            GlyphClipping = 0x00000800,
            Offscreen1 = 0x00001000,
            AlphaBlend = 0x00002000,
            ThreeD = 0x00004000,
            ExtendedFifo = 0x00008000,
            MultiMon = 0x00010000,
            PitchLock = 0x00020000,
            IrqMask = 0x00040000,
            DisplayTopology = 0x00080000,
            Gmr = 0x00100000,
            Traces = 0x00200000,
            Gmr2 = 0x00400000,
            ScreenObject2 = 0x00800000
        }

        private static NVIDIA9500MGS instance;

        private ushort IndexPort;
        private ushort ValuePort;
        private ushort BiosPort;
        private ushort IRQPort;
        private uint capabilities;

        public Cosmos.Core.MemoryBlock VIDEO_MEMORY;
        private Cosmos.Core.MemoryBlock FIFO_Memory;

        public NVIDIA9500MGS(PCIDevice host) : base(host)
        {
            Label = "NVIDIA 9500MGS";
            instance = this;
            Settings.EnterVideoMode = EnterGraphicsMode;
            host.EnableMemory(true);

            uint basePort = ((PCIDeviceNormal)host).BaseAddresses[0].BaseAddress();
            IndexPort = ((ushort)(basePort + (uint)IOPortOffset.Index));
            ValuePort = ((ushort)(basePort + (uint)IOPortOffset.Value));
            BiosPort = ((ushort)(basePort + (uint)IOPortOffset.Bios));
            IRQPort = ((ushort)(basePort + (uint)IOPortOffset.IRQ));

            WriteRegister(Register.ID, (uint)ID.V2);
            if (ReadRegister(Register.ID) != (uint)ID.V2)
                return;

            VIDEO_MEMORY = new Cosmos.Core.MemoryBlock(ReadRegister(Register.FrameBufferStart), ReadRegister(Register.VRamSize));
            capabilities = ReadRegister(Register.Capabilities);
            InitializeFIFO();
        }

        public void EnterGraphicsMode()
        {
            SetMode(Settings.DisplayWidth, Settings.DisplayHeight);
            GL.UpdatePointer = (uint)((aMethod)Update).GetHashCode();
        }

        protected void WriteRegister(Register register, uint value)
        {
            IOPort.outd(IndexPort, (uint)register);
            IOPort.outd(ValuePort, value);
        }

        protected uint ReadRegister(Register register)
        {
            IOPort.outd(IndexPort, (uint)register);
            return IOPort.ind(ValuePort);
        }

        protected void InitializeFIFO()
        {
            FIFO_Memory = new Cosmos.Core.MemoryBlock(ReadRegister(Register.MemStart), ReadRegister(Register.MemSize));
            FIFO_Memory[(uint)FIFO.Min] = (uint)Register.FifoNumRegisters * sizeof(uint);
            FIFO_Memory[(uint)FIFO.Max] = FIFO_Memory.Size;
            FIFO_Memory[(uint)FIFO.NextCmd] = FIFO_Memory[(uint)FIFO.Min];
            FIFO_Memory[(uint)FIFO.Stop] = FIFO_Memory[(uint)FIFO.Min];
            WriteRegister(Register.ConfigDone, 1);
        }

        public void SetMode(int width, int height, int depth = 32)
        {
            Settings.DisplayVRam = (uint*)VIDEO_MEMORY.Base;
            WriteRegister(Register.Width, (uint)width);
            WriteRegister(Register.Height, (uint)height);
            WriteRegister(Register.BitsPerPixel, (uint)depth);
            WriteRegister(Register.Enable, 1);
            InitializeFIFO();
        }

        protected int GetFIFO(FIFO cmd)
        {
            return (int)FIFO_Memory[(uint)cmd];
        }

        protected int SetFIFO(FIFO cmd, int value)
        {
            return (int)(FIFO_Memory[(uint)cmd] = (uint)value);
        }

        protected void WaitForFifo()
        {
            WriteRegister(Register.Sync, 1);
            while (ReadRegister(Register.Busy) != 0) { }
        }

        protected void WriteToFifo(int value)
        {
            if (((GetFIFO(FIFO.NextCmd) == GetFIFO(FIFO.Max) - 4) && GetFIFO(FIFO.Stop) == GetFIFO(FIFO.Min)) ||
                (GetFIFO(FIFO.NextCmd) + 4 == GetFIFO(FIFO.Stop)))
                WaitForFifo();

            SetFIFO((FIFO)GetFIFO(FIFO.NextCmd), value);
            SetFIFO(FIFO.NextCmd, GetFIFO(FIFO.NextCmd) + 4);

            if (GetFIFO(FIFO.NextCmd) == GetFIFO(FIFO.Max))
                SetFIFO(FIFO.NextCmd, GetFIFO(FIFO.Min));
        }

        public static void Update()
        {
            Utils.memcpy(Settings.DisplayVRam, Settings.DisplayBuffer, Settings.DisplayWidth * Settings.DisplayHeight);
            instance.Update(0, 0, Settings.DisplayWidth, Settings.DisplayHeight);
        }

        public void Update(int x, int y, int width, int height)
        {
            WriteToFifo((int)FIFOCommand.Update);
            WriteToFifo(x);
            WriteToFifo(y);
            WriteToFifo(width);
            WriteToFifo(height);
            WaitForFifo();
        }
    }
}