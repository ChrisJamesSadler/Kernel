using Kernel.System.Drivers.Other;
using System.Collections.Generic;

namespace Kernel.System.Drawing
{
    public abstract class IWindow : IComponent
    {
        public string title = string.Empty;
        public Rect bar;
        private bool dragging = false;
        private Point draggingOffset;

        public IWindow()
        {
            primaryColour = Theme.WindowPrimaryColour;
            secondaryColour = Theme.WindowSecondaryColour;
            bar = new Rect(100, 100, 500, 20);
            SetSize(0, 350);
            draggingOffset = new Point(0, 0);
        }

        public override void OnDraw()
        {
            if(!dragging)
            {
                if(Settings.CursorState == PS2Mouse.MouseState.Left)
                {
                    if(bar.Containts(Settings.CursorX, Settings.CursorY))
                    {
                        dragging = true;
                        draggingOffset.X = Settings.CursorX - bar.X;
                        draggingOffset.Y = Settings.CursorY - bar.Y;
                    }
                }
            }
            else
            {
                bar.X = Settings.CursorX - draggingOffset.X;
                bar.Y = Settings.CursorY - draggingOffset.Y;
                if(Settings.CursorState != PS2Mouse.MouseState.Left)
                {
                    dragging = false;
                    draggingOffset.X = 0;
                    draggingOffset.Y = 0;
                }
            }
            area.X = bar.X;
            area.Y = bar.Y + bar.Height;
            area.Width = bar.Width;
            Graphics.DrawRectangle(area.X, area.Y, area.Width, area.Height, primaryColour);
            Graphics.DrawRectangleEmpty(area.X, area.Y, area.Width, area.Height, secondaryColour);
            Graphics.DrawRectangle(bar.X, bar.Y, bar.Width, bar.Height, Theme.WindowBarPrimary);
            Graphics.DrawRectangleEmpty(bar.X, bar.Y, bar.Width, bar.Height, secondaryColour);
            Graphics.DrawText(bar.X + 10, bar.Y + 5, title, Theme.WindowBarText);
        }
    }
}