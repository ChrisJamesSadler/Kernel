using Kernel.Application;
using Kernel.System;
using Kernel.System.Drawing;
using Kernel.System.Drivers.Other;

namespace Kernel.Window
{
    public class Form : Drawable
    {
        public Rect WindowBar = new Rect(100, 100, 320, 20);
        private bool Dragging = false;
        private Point DraggingOffset = new Point();

        public Form()
        {
            Title = "New Window";
            PrimaryColour = Theme.WindowPrimaryColour;
            SecondaryColour = Theme.WindowSecondaryColour;
            SetPosition(100, 100);
            SetSize(320, 240);
            DWM.OpenForms.Add(this);
        }

        public new void SetPosition(int X, int Y)
        {
            WindowBar.X = X;
            WindowBar.Y = Y;
        }

        public new void SetSize(int W, int H)
        {
            WindowBar.Width = W;
            Bounds.Height = H;
        }

        public override void OnDraw()
        {
            if (!Dragging)
            {
                if (CurrentActiveComponent == this)
                {
                    if (Settings.CursorState == PS2Mouse.MouseState.Left)
                    {
                        if (WindowBar.Containts(Settings.CursorX, Settings.CursorY))
                        {
                            Dragging = true;
                            DraggingOffset.X = Settings.CursorX - WindowBar.X;
                            DraggingOffset.Y = Settings.CursorY - WindowBar.Y;
                        }
                    }
                }
                else
                {
                    Dragging = false;
                }
            }
            else
            {
                WindowBar.X = Settings.CursorX - DraggingOffset.X;
                WindowBar.Y = Settings.CursorY - DraggingOffset.Y;
                if (Settings.CursorState != PS2Mouse.MouseState.Left)
                {
                    Dragging = false;
                    DraggingOffset.X = 0;
                    DraggingOffset.Y = 0;
                }
            }
            Bounds.X = WindowBar.X;
            Bounds.Y = WindowBar.Y + WindowBar.Height;
            Bounds.Width = WindowBar.Width;
            Graphics.DrawRectangle(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height, PrimaryColour);
            Graphics.DrawRectangleEmpty(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height, SecondaryColour);
            Graphics.DrawRectangle(WindowBar.X, WindowBar.Y, WindowBar.Width, WindowBar.Height, Theme.WindowBarPrimary);
            Graphics.DrawRectangleEmpty(WindowBar.X, WindowBar.Y, WindowBar.Width, WindowBar.Height, SecondaryColour);
            Graphics.DrawText(WindowBar.X + 10, WindowBar.Y + 5, Title, Theme.WindowBarText);
        }

        public override void OnMouseDown(int mouse)
        {
            if (mouse == 0)
            {
                if (WindowBar.Containts(Settings.CursorX, Settings.CursorY) )
                {
                    CurrentActiveComponent = this;
                }
            }
        }
    }
}