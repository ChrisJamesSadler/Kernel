using Kernel.System;
using Kernel.System.Drawing;
using Kernel.System.Events;
using System.Collections.Generic;

namespace Kernel.Window
{
    public abstract class Drawable : EventListener
    {
        public static Drawable CurrentActiveComponent;

        public Rect Bounds = new Rect();
        public int GlobalX;
        public int GlobalY;
        public uint PrimaryColour;
        public uint SecondaryColour;
        public Drawable Parent;
        public List<Drawable> Children = new List<Drawable>(32);
        public string Title = string.Empty;

        public void Add(Drawable child)
        {
            child.Parent = this;
            Children.Add(this);
        }

        public void SetPosition(int X, int Y)
        {
            Bounds.X = X;
            Bounds.Y = Y;
        }

        public void SetSize(int W, int H)
        {
            Bounds.Width = W;
            Bounds.Height = H;
        }

        public void Draw()
        {
            GlobalX = 0;
            GlobalY = 0;
            TracePosition(this, ref GlobalX, ref GlobalY);
            OnDraw();
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] != null)
                {
                    Children[i].Draw();
                }
            }
        }

        public abstract void OnDraw();

        public override void OnMouseDown(int mouse)
        {
            if(mouse == 0)
            {
                if(Bounds.Containts(Settings.CursorX, Settings.CursorY))
                {
                    CurrentActiveComponent = this;
                }
            }
        }

        private static void TracePosition(Drawable draw, ref int posX, ref int posY)
        {
            if (draw.Parent != null)
            {
                TracePosition(draw.Parent, ref posX, ref posY);
            }
            posX += draw.Bounds.X;
            posY += draw.Bounds.Y;
        }
    }
}