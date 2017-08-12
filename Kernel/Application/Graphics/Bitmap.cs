using Kernel.System.Core;
using Math = System.Math;
using Kernel.System.Drawing;
using Kernel.System;

namespace Kernel.Application.Graphics
{
    public unsafe class Bitmap
    {
        private bool usesExisting;
        public uint* Buffer;
        public int Width;
        public int Height;

        public Bitmap(int w, int h)
        {
            usesExisting = false;
            Width = w;
            Height = h;
            Buffer = (uint*)Heap.alloc((uint)(Width * Height) * 4);
        }

        public Bitmap(int w, int h, uint* b)
        {
            usesExisting = true;
            Buffer = b;
            Width = w;
            Height = h;
        }

        public void Clear(uint colour)
        {
            GL.Clear(Buffer, colour, Width * Height);
        }

        public void DrawRectangle(int x, int y, int width, int height, uint colour, bool filled = true)
        {
            if (filled)
            {
                GL.DrawRectangleFilled(Buffer, Width, colour, x, y, width, height);
            }
            else
            {
                GL.DrawRectangleEmpty(Buffer, Width, colour, x, y, width, height);
            }
        }

        public void DrawString(int x, int y, string content, uint colour)
        {
            GL.DrawText(Buffer, Width, x, y, content, colour);
        }

        public void DrawCharacter(int x, int y, char aChar, uint colour)
        {
            GL.DrawCharacter(Buffer, Width, x, y, aChar, colour);
        }

        public void DrawNumber(int x, int y, int content, uint colour)
        {
            GL.DrawNumber(Buffer, Width, x, y, content, colour);
        }

        public void DrawLine(int fromX, int fromY, int toX, int toY, uint colour)
        {
            GL.DrawLine(Buffer, Width, fromX, fromY, toX, toY, colour);
        }

        public void DrawImage(int X, int Y, Bitmap image)
        {
            uint* from = image.Buffer + (Y * image.Width) + X;
            uint* to = Buffer + (Y * Width) + X;
            for (int i = 0; i < image.Height; i++)
            {
                Utils.memcpy(to, from, image.Width);
                from += image.Width;
                to += Width;
            }
        }

        public void ResizeImage(int newXSize, int newYSize)
        {
            uint* newBMP = (uint*)Heap.alloc((uint)(newXSize * newYSize) * 4);
            int w1 = Width;
            int h1 = Height;
            int x_ratio = (int)((w1 << 16) / newXSize) + 1;
            int y_ratio = (int)((h1 << 16) / newYSize) + 1;

            int x2, y2;
            for (int i = 0; i < newYSize; i++)
            {
                for (int j = 0; j < newXSize; j++)
                {
                    x2 = ((j * x_ratio) >> 16);
                    y2 = ((i * y_ratio) >> 16);
                    newBMP[(i * newXSize) + j] = Buffer[(y2 * Width) + x2];
                }
            }
            if (usesExisting)
            {
                Utils.memcpy(Buffer, newBMP, newXSize * newYSize);
                Heap.free(newBMP);
            }
            else
            {
                Heap.free(Buffer);
                Buffer = newBMP;
            }
            Width = newXSize;
            Height = newYSize;
        }

        public static void ResizeImage(uint* newBMP, int newXSize, int newYSize, uint* Buffer, int Width, int Height)
        {
            int x_ratio = (int)((Width << 16) / newXSize) + 1;
            int y_ratio = (int)((Height << 16) / newYSize) + 1;

            int x2, y2;
            for (int i = 0; i < newYSize; i++)
            {
                for (int j = 0; j < newXSize; j++)
                {
                    x2 = ((j * x_ratio) >> 16);
                    y2 = ((i * y_ratio) >> 16);
                    newBMP[(i * newXSize) + j] = Buffer[(y2 * Width) + x2];
                }
            }
        }
    }
}