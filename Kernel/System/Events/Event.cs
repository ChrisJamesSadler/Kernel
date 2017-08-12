using Kernel.System.Drivers.Other;
using System.Collections.Generic;

namespace Kernel.System.Events
{
    public static class Event
    {
        private static int lastX;
        private static int lastY;
        private static byte aChar;
        private static PS2Mouse.MouseState lastState;

        public static List<EventListener> AllListeners = new List<EventListener>();

        public static void Init()
        {
            while(true)
            {
                if(lastX != Settings.CursorX || lastY != Settings.CursorY)
                {
                    lastX = Settings.CursorX;
                    lastY = Settings.CursorY;
                    foreach (EventListener e in AllListeners)
                    {
                        e.OnMouseMove(lastX, lastY);
                    }
                }
                aChar = (byte)PS2Keyboard.buffer.Pop();
                if(aChar != 0)
                {
                    if ((aChar & 128) == 0)
                    {
                        aChar = (byte)PS2Keyboard.Decode(aChar);
                        if (aChar != 0)
                        {
                            Console.RecieveKey((char)aChar);
                            foreach (EventListener e in AllListeners)
                            {
                                e.OnKeyDown((char)aChar);
                            }
                        }
                    }
                }
                if(lastState != Settings.CursorState)
                {
                    lastState = Settings.CursorState;
                    if (lastState == PS2Mouse.MouseState.Left)
                    {
                        foreach (EventListener e in AllListeners)
                        {
                            e.OnMouseDown(0);
                        }
                    }
                    else if (lastState == PS2Mouse.MouseState.Right)
                    {
                        foreach (EventListener e in AllListeners)
                        {
                            e.OnMouseDown(1);
                        }
                    }
                }
            }
        }
    }
}