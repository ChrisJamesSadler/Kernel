using System.Collections.Generic;

namespace Kernel.System.Drawing
{
    public abstract class IComponent
    {
        public string content;
        public Rect area = new Rect();
        public uint primaryColour;
        public uint secondaryColour;
        public IComponent parent;
        public int GlobalX;
        public int GlobalY;
        public List<IComponent> Children = new List<IComponent>(32);

        public void Draw()
        {
            GlobalX = 0;
            GlobalY = 0;
            TracePosition(this, ref GlobalX, ref GlobalY);
            OnDraw();
            foreach (IComponent comp in Children)
            {
                comp.Draw();
            }
        }

        public abstract void OnDraw();

        public void Add(IComponent child)
        {
            child.parent = this;
            Children.Add(child);
        }

        public void SetSize(int width, int height)
        {
            area.Width = width;
            area.Height = height;
        }

        private static void TracePosition(IComponent draw, ref int posX, ref int posY)
        {
            if (draw.parent != null)
            {
                TracePosition(draw.parent, ref posX, ref posY);
            }
            posX += draw.area.X;
            posY += draw.area.Y;
        }
    }
}