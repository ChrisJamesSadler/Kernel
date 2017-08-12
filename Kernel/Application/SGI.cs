using Kernel.Application.Graphics;
using Kernel.System;
using Kernel.System.Core;
using Kernel.System.Drawing;
using Kernel.System.HAL;
using Kernel.System.Threading;
using System.Collections.Generic;

namespace Kernel.Application
{
    public unsafe class SGI : Shell
    {
        public static List<Form> Forms = new List<Form>();
        public static Form ActiveForm;
        public static Form CurrentForm;
        public static IControl ActiveControl;
        public static IControl CurrentControl;

        public Panel MenuBar;
        public Panel MenuArea;

        public new void Main()
        {
            if (Settings.UseDebugVideoMode)
            {
                Settings.DisplayVRam = (uint*)Heap.alloc((uint)(Settings.DisplayWidth * Settings.DisplayHeight) * 4);
            }
            else
            {
                Settings.EnterVideoMode();
            }

            Bitmap display = new Bitmap(Settings.DisplayWidth, Settings.DisplayHeight, Settings.DisplayBuffer);

            bool Dragging = false;
            int xOffset = 0;
            int yOffset = 0;

            byte lastSecond = 0;
            int fps = 0;
            int lFPS = 0;

            MenuBar = new Panel();
            MenuBar.SetSize(display.Width, 30);
            MenuBar.SetPosition(0, display.Height - MenuBar.Bounds.Height);
            MenuBar.PrimaryColour = Theme.DesktopBar;
            MenuBar.SecondaryColour = Theme.DesktopBar;
            Button btnMenu = new Button();
            btnMenu.SetPosition(25, 0);
            btnMenu.SetSize(100, MenuBar.Bounds.Height);
            btnMenu.Content = "App Menu";
            btnMenu.OnClick = ToggleMenuButton;
            MenuBar.Add(btnMenu);
            MenuArea = new Panel();
            MenuArea.Visable = false;
            MenuArea.SetSize(300, (int)(display.Height / 1.25f));
            MenuArea.SetPosition(0, MenuBar.Bounds.Y - MenuArea.Bounds.Height);
            MenuBar.Add(MenuArea);

            Form aWindow = new Login();

            bool toggle = false;

            while (true)
            {
                #region Calculate FPS
                fps++;
                if (lastSecond != RTC.Second)
                {
                    lastSecond = RTC.Second;
                    Console.Write("FPS is ");
                    Console.WriteLine(fps);
                    lFPS = fps;
                    fps = 0;
                    Console.Write("Allocations ");
                    Console.WriteLine(Heap.totalAllocations);
                }
                #endregion

                Input.Update();

                #region Draw Background
                if (toggle)
                {
                    int r = 0;
                    int g = 255;
                    int b = 50;
                    int steps = 1;
                    bool skip = false;
                    for (int i = 1; i <= 768 / steps; i++)
                    {
                        uint colour = (uint)((r << 16) | (g << 8) | b);
                        display.DrawRectangle(0, display.Height - (i * steps), display.Width, steps, colour);
                        if (!skip)
                        {
                            g--;
                        }
                        skip = !skip;
                        if (g < 10)
                        {
                            g = 10;
                        }
                    }
                }
                else
                {
                    display.Clear(Theme.DesktopBackground);
                }
                #endregion

                #region Draw Forms
                CurrentForm = null;
                CurrentControl = null;
                for (int i = Forms.Count - 1; i >= 0; i--)
                {
                    Forms[i].Draw(display);
                }
                #endregion

                #region Drag Forms
                if (!Dragging)
                {
                    if (CurrentForm != null)
                    {
                        ActiveForm = CurrentForm;
                    }
                    if (CurrentControl != null)
                    {
                        ActiveControl = CurrentControl;
                    }
                }
                if (ActiveForm != null)
                {
                    if (!Dragging)
                    {
                        if (Input.GetMouseButton(0) && ActiveForm.Banner.Contains(Input.MousePosition))
                        {
                            Dragging = true;
                            xOffset = Input.MousePosition.X - ActiveForm.Bounds.X;
                            yOffset = Input.MousePosition.Y - ActiveForm.Bounds.Y;
                        }
                    }
                    else
                    {
                        if (!Input.GetMouseButton(0))
                        {
                            Dragging = false;
                        }
                        else
                        {
                            ActiveForm.Bounds.X = Input.MousePosition.X - xOffset;
                            ActiveForm.Bounds.Y = Input.MousePosition.Y - yOffset;
                        }
                    }
                    if (ActiveForm.Bounds.X < 0)
                    {
                        ActiveForm.Bounds.X = 0;
                    }
                    if (ActiveForm.Bounds.Y <= ActiveForm.Banner.Height)
                    {
                        ActiveForm.Bounds.Y = ActiveForm.Banner.Height;
                    }
                    if (ActiveForm.Bounds.X > Settings.DisplayWidth - ActiveForm.Bounds.Width)
                    {
                        ActiveForm.Bounds.X = Settings.DisplayWidth - ActiveForm.Bounds.Width;
                    }
                    if (ActiveForm.Bounds.Y > Settings.DisplayHeight - ActiveForm.Bounds.Height)
                    {
                        ActiveForm.Bounds.Y = Settings.DisplayHeight - ActiveForm.Bounds.Height;
                    }
                }
                #endregion

                #region Draw Taskbar
                bool draw = true;
                if (ActiveForm != null)
                {
                    if (ActiveForm.WindowState == Form.FormWindowState.FullScreen)
                    {
                        draw = false;
                    }
                }
                if (draw)
                {
                    MenuBar.Draw(display);

                    int pos = 150;
                    for (int i = 0; i < Forms.Count; i++)
                    {
                        int btnSize = (Forms[i].Content.Length * GL.fontSize) + 6;
                        display.DrawRectangle(pos, MenuBar.Bounds.Y, btnSize, MenuBar.Bounds.Height, Theme.DesktopBar + 0x202020);
                        display.DrawString(pos + 3, MenuBar.Bounds.Y + (MenuBar.Bounds.Height / 2) - (GL.fontSize / 2), Forms[i].Content, 0);
                        pos += btnSize;
                    }
                }
                #endregion

                #region Handle Clicks
                if (Input.GetMouseButton(0))
                {
                    if (ActiveControl != null)
                    {
                        if (ActiveControl is Button)
                        {
                            ((Button)ActiveControl).OnClick?.Invoke();
                        }
                    }
                }
                #endregion

                #region Draw Time
                if (draw)
                {
                    byte time = RTC.Hour;
                    if (time < 10)
                    {
                        display.DrawNumber(display.Width - 90, display.Height - 25, 0, 0);
                        display.DrawNumber(display.Width - 80, display.Height - 25, time, 0);
                    }
                    else
                    {
                        display.DrawNumber(display.Width - 90, display.Height - 25, time, 0);
                    }
                    display.DrawCharacter(display.Width - 70, display.Height - 25, ':', 0);
                    time = RTC.Minute;
                    if (time < 10)
                    {
                        display.DrawNumber(display.Width - 60, display.Height - 25, 0, 0);
                        display.DrawNumber(display.Width - 50, display.Height - 25, time, 0);
                    }
                    else
                    {
                        display.DrawNumber(display.Width - 60, display.Height - 25, time, 0);
                    }
                    display.DrawCharacter(display.Width - 40, display.Height - 25, ':', 0);
                    time = RTC.Second;
                    if (time < 10)
                    {
                        display.DrawNumber(display.Width - 30, display.Height - 25, 0, 0);
                        display.DrawNumber(display.Width - 20, display.Height - 25, time, 0);
                    }
                    else
                    {
                        display.DrawNumber(display.Width - 30, display.Height - 25, time, 0);
                    }
                }
                #endregion

                // draw fps
                display.DrawNumber(0, 0, lFPS, 0xFF0000);

                #region Draw Cursor
                display.DrawLine(Input.MousePosition.X, Input.MousePosition.Y, Input.MousePosition.X, Input.MousePosition.Y + 10, 0);
                display.DrawLine(Input.MousePosition.X, Input.MousePosition.Y, Input.MousePosition.X + 4, Input.MousePosition.Y + 8, 0);
                display.DrawLine(Input.MousePosition.X, Input.MousePosition.Y + 10, Input.MousePosition.X + 4, Input.MousePosition.Y + 8, 0);
                #endregion

                #region Reorganise Window Layer
                if (ActiveForm != null)
                {
                    if (Forms[0] != ActiveForm)
                    {
                        for (int i = 0; i < Forms.Count; i++)
                        {
                            if (Forms[i] == ActiveForm)
                            {
                                var f = Forms[i];
                                Forms.RemoveAt(i);
                                Forms.Insert(0, f);
                                break;
                            }
                        }
                    }
                }
                #endregion

                GL.Update();

                #region Remove Old Forms
                for (int i = 0; i < Forms.Count; i++)
                {
                    if (Forms[i].WindowState == Form.FormWindowState.Closed)
                    {
                        //if (ActiveControl != null)
                        //{
                        //    if (ActiveForm != null)
                        //    {
                        //        IControl root = ActiveControl.Parent;
                        //        while (root.Parent != null)
                        //        {
                        //            root = root.Parent;
                        //        }
                        //        if (root == Forms[i])
                        //        {
                        //            ActiveControl = null;
                        //        }
                        //    }
                        //}
                        //Forms[i].Dispose();
                        //Forms.RemoveAt(i);
                        ActiveForm = null;
                        ActiveControl = null;
                        CurrentForm = null;
                        CurrentControl = null;
                        break;
                    }
                }
                #endregion

                Thread.Sleep(1000 / 30);
            }
        }

        public override void OnKeyDown(char aChar)
        {
            if (ActiveControl != null)
            {
                if (ActiveControl is TextBox)
                {
                    int max = 32;
                    TextBox tb = (TextBox)ActiveControl;

                    if (aChar == '\b')
                    {
                        if (tb.EnteredIndex > 0)
                        {
                            tb.EnteredIndex--;
                            tb.EnteredContent[tb.EnteredIndex] = 0;
                        }
                    }
                    else if (aChar == '\t')
                    {
                        OnKeyDown(' ');
                        OnKeyDown(' ');
                        OnKeyDown(' ');
                        OnKeyDown(' ');
                    }
                    else
                    {
                        if (tb.EnteredIndex <= max)
                        {
                            tb.EnteredContent[tb.EnteredIndex] = (byte)aChar;
                            tb.EnteredIndex++;
                        }
                    }
                }
            }
        }

        public void ToggleMenuButton()
        {
            MenuArea.Visable = !MenuArea.Visable;
        }
    }
}