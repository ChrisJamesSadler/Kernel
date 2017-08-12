using Kernel.System.Core;
using Kernel.System.Events;
using Kernel.System.HAL;
using System.Collections.Generic;

namespace Kernel.Application.Graphics
{
    public abstract class IControl : EventListener
    {
        public IControl Parent;
        public Rect RenderArea
        {
            get
            {
                if(renArea == null)
                {
                    renArea = new Rect();
                }
                return renArea;
            }
            set
            {
                renArea = value;
            }
        }
        private Rect renArea;
        public uint PrimaryColour;
        public uint SecondaryColour;
        public Rect Bounds = new Rect();
        public string Content = string.Empty;
        public bool Visable;
        public bool skipChildren;
        public List<IControl> Children = new List<IControl>();

        public IControl()
        {
            Visable = true;
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

        public void Add(IControl child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public void Remove(IControl child)
        {
            child.Parent = null;
            for(int i = 0; i < Children.Count; i++)
            {
                if(Children[i] == child)
                {
                    Children.RemoveAt(i);
                }
            }
        }

        public void Draw(Bitmap target)
        {
            RenderArea.X = 0;
            RenderArea.Y = 0;
            RenderArea.Width = Bounds.Width;
            RenderArea.Height = Bounds.Height;
            TracePosition(this, ref RenderArea.X, ref RenderArea.Y);
            if (Input.GetMouseButton(0) && RenderArea.Contains(Input.MousePosition))
            {
                if (!(this is Form))
                {
                    SGI.CurrentControl = this;
                }
            }
            skipChildren = false;
            OnDraw(target);
            if (!skipChildren || Visable)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    if (Children[i] != null)
                    {
                        Children[i].Draw(target);
                    }
                }
            }
        }

        public abstract void OnDraw(Bitmap target);

        private static void TracePosition(IControl draw, ref int posX, ref int posY)
        {
            if (draw.Parent != null)
            {
                TracePosition(draw.Parent, ref posX, ref posY);
            }
            posX += draw.Bounds.X;
            posY += draw.Bounds.Y;
        }

        //public virtual void OnDispose() { }
        //public void Dispose()
        //{
        //    OnDispose();
        //    foreach(var child in Children)
        //    {
        //        child.Dispose();
        //    }
        //    Heap.free(renArea);
        //    Heap.free(Bounds);
        //    Heap.free(this);
        //}
    }
}