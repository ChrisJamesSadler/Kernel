using Kernel.System.Core;
using Kernel.System.Drivers;
using System;

namespace Kernel.System.Drawing
{
    public unsafe class DefaultGraphics : GraphicsDriver
    {
        public DefaultGraphics(object h) : base(h)
        {
        }

        public static void Init()
        {
            new DefaultGraphics(null);
        }

        public override void Clear(uint Colour)
        {
            Utils.memset(Settings.DisplayBuffer, Colour, (uint)(Settings.DisplayWidth * Settings.DisplayHeight));
        }

        public override void DrawRectangle(int X, int Y, int Width, int Height, uint Colour)
        {
            uint* where = Settings.DisplayBuffer + ((Y * Settings.DisplayWidth));
            int yTarget = (Y + Height);
            int adder = Settings.DisplayWidth;
            for (int yTmp = Y; yTmp < yTarget; yTmp++)
            {
                Utils.memset(where + X, Colour, (uint)Width);
                where += adder;
            }
        }

        public override void DrawLine(int x0, int y0, int x1, int y1, uint c)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep) { Swap(ref x0, ref y0); Swap(ref x1, ref y1); }
            if (x0 > x1) { Swap(ref x0, ref x1); Swap(ref y0, ref y1); }
            int dX = (x1 - x0), dY = Math.Abs(y1 - y0), err = (dX / 2), ystep = (y0 < y1 ? 1 : -1), y = y0;

            for (int x = x0; x <= x1; ++x)
            {
                if (steep)
                {
                    SetPixel(y, x, c);
                }
                else
                {
                    SetPixel(x, y, c);
                }
                err = err - dY;
                if (err < 0) { y += ystep; err += dX; }
            }
        }

        public void Swap<T>(ref T lhs, ref T rhs) { T temp; temp = lhs; lhs = rhs; rhs = temp; }
        
        public void SetPixel(int X, int Y, uint Colour)
        {
            uint* where = Settings.DisplayBuffer + (Y * Settings.DisplayWidth) + X;
            where[0] = Colour;
        }

        public override void DrawRectangleEmpty(int X, int Y, int Width, int Height, uint Colour)
        {
            DrawLine(X, Y, X + Width, Y, Colour);
            DrawLine(X, Y, X, Y + Height, Colour);
            DrawLine(X, Y + Height, X + Width, Y + Height, Colour);
            DrawLine(X + Width, Y, X + Width, Y + Height, Colour);
        }

        public override void Update()
        {
            Utils.memcpy(Settings.DisplayVRam, Settings.DisplayBuffer, Settings.DisplayWidth * Settings.DisplayHeight);
        }
    }
}