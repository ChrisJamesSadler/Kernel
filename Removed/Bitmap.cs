using Kernel.System.Core;

namespace Kernel.System.Drawing
{
    public unsafe class Bitmap
    {
        public uint* buffer;
        public int width;
        public int height;

        public static Bitmap Create(uint width, uint height)
        {
            return Create((int)width, (int)height);
        }

        public static Bitmap Create(int width, int height)
        {
            Bitmap bmp = new Bitmap();
            bmp.width = width;
            bmp.height = height;
            bmp.buffer = (uint*)Heap.alloc((uint)(width * height) * 4);
            return bmp;
        }

        public static Bitmap Create(uint* ptr, uint width, uint height)
        {
            return Create(ptr, (int)width, (int)height);
        }

        public static Bitmap Create(uint* ptr, int width, int height)
        {
            Bitmap bmp = new Bitmap();
            bmp.width = width;
            bmp.height = height;
            bmp.buffer = ptr;
            return bmp;
        }
    }
}