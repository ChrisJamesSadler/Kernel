using Kernel.System;
using Kernel.System.Drawing;
using Kernel.System.Threading;
using Kernel.Window;
using System.Collections.Generic;

namespace Kernel.Application
{
    public unsafe class DWM
    {
        public static List<Form> OpenForms = new List<Form>(32);

        public static void Run()
        {
            Settings.EnterVideoMode();
            
            Settings.CursorX = Settings.DisplayWidth / 2;
            Settings.CursorY = Settings.DisplayHeight / 2;

            while (true)
            {
                Graphics.Clear(Theme.DesktopBackground);

                Graphics.DrawRectangle(0, Settings.DisplayHeight - 40, Settings.DisplayWidth, 40, Theme.DesktopBar);
                
                for (int i = 0; i < 6; i++)
                {
                    Graphics.DrawLine(Settings.CursorX, Settings.CursorY, Settings.CursorX + i, Settings.CursorY + 10, Theme.Cursor);
                }
                Graphics.DrawLine(Settings.CursorX, Settings.CursorY, Settings.CursorX + 3, Settings.CursorY + 15, Theme.Cursor);

                Graphics.Update();
                //Thread.Sleep(1000 / 10);
            }
        }
    }
}