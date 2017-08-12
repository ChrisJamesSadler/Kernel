using Kernel.Application.Graphics;
using Kernel.System;
using Kernel.System.Drivers.Other;

namespace Kernel.Application
{
    public static class Input
    {
        public static Point MousePosition;

        private static bool MouseLeft;
        private static bool wasLeftDown;
        private static bool MouseRight;
        private static bool wasRightDown;

        public static void Update()
        {
            if (MousePosition == null)
            {
                MousePosition = new Point();
            }
            MousePosition.X = Settings.CursorX;
            MousePosition.Y = Settings.CursorY;
            if(MousePosition.X < 0)
            {
                MousePosition.X = 0;
            }
            if(MousePosition.Y < 0)
            {
                MousePosition.Y = 0;
            }
            if(MousePosition.X > Settings.DisplayWidth)
            {
                MousePosition.X = Settings.DisplayWidth;
            }
            if (MousePosition.Y > Settings.DisplayHeight)
            {
                MousePosition.Y = Settings.DisplayHeight;
            }

            if (Settings.CursorState == PS2Mouse.MouseState.Left)
            {
                if (!wasLeftDown)
                {
                    wasLeftDown = true;
                }
                else
                {
                    MouseLeft = true;
                }
            }
            else
            {
                MouseLeft = false;
            }
            if (Settings.CursorState == PS2Mouse.MouseState.Right)
            {
                if (!wasRightDown)
                {
                    wasRightDown = true;
                }
                else
                {
                    MouseRight = true;
                }
            }
            else
            {
                MouseRight = false;
            }
        }

        public static bool GetMouseButton(int num)
        {
            switch (num)
            {
                case 0:
                    return MouseLeft;
                    break;

                case 1:
                    return MouseRight;
                    break;
            }
            return false;
        }
    }
}