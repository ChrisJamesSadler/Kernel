using Kernel.System;
using Kernel.System.HAL;

namespace Kernel.Application.Graphics
{
    public abstract class Form : IControl
    {
        public static Form formRendering;
        public Rect Banner = new Rect();
        private bool initialized = false;
        public FormWindowState WindowState = FormWindowState.Normal;
        public Bitmap BackgroundImage = null;

        public Form()
        {
            Content = "New Form";
            PrimaryColour = Theme.WindowPrimaryColour;
            SecondaryColour = Theme.WindowShadow;
            SetSize(250, 300);
            CentreToScreen();
            SGI.Forms.Add(this);
        }

        public void CentreToScreen()
        {
            Bounds.X = Settings.DisplayWidth / 2;
            Bounds.X -= Bounds.Width / 2;
            Bounds.Y = Settings.DisplayHeight / 2;
            Bounds.Y -= Bounds.Height / 2;
        }

        public override void OnDraw(Bitmap target)
        {
            formRendering = this;
            if (!initialized)
            {
                initialized = true;
                InitializeComponents();
            }

            BeforeRedraw();

            if(WindowState == FormWindowState.Minimized)
            {
                skipChildren = true;
            }
            else if(WindowState == FormWindowState.Normal)
            {
                Banner.X = RenderArea.X;
                Banner.Y = RenderArea.Y - 20;
                Banner.Width = Bounds.Width;
                Banner.Height = 20;
                target.DrawRectangle(Banner.X, Banner.Y, Banner.Width, Banner.Height, Theme.WindowBarPrimary, true);
                target.DrawRectangle(Banner.X, Banner.Y, Banner.Width, Banner.Height, Theme.WindowShadow, false);
                target.DrawString(Banner.X + 5, Banner.Y + 6, Content, Theme.WindowBarText);
                target.DrawRectangle(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height, PrimaryColour, true);
                target.DrawRectangle(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height, SecondaryColour, false);
            }
            else if(WindowState == FormWindowState.Maximized)
            {
                RenderArea.X = 0;
                RenderArea.Y = 20;
                Bounds.X = 0;
                Bounds.Y = 20;
                Bounds.Width = target.Width;
                Bounds.Height = target.Height;
                Banner.X = 0;
                Banner.Y = 0;
                Banner.Width = Bounds.Width;
                Banner.Height = 20;
                target.DrawRectangle(Banner.X, Banner.Y, Banner.Width, Banner.Height, Theme.WindowBarPrimary, true);
                target.DrawRectangle(Banner.X, Banner.Y, Banner.Width, Banner.Height, Theme.WindowShadow, false);
                target.DrawString(Banner.X + 5, Banner.Y + 6, Content, Theme.WindowBarText);
                target.DrawRectangle(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height, PrimaryColour, true);
                target.DrawRectangle(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height, SecondaryColour, false);
            }
            else if(WindowState == FormWindowState.FullScreen)
            {
                RenderArea.X = 0;
                RenderArea.Y = 0;
                Bounds.X = 0;
                Bounds.Y = 0;
                Bounds.Width = target.Width;
                Bounds.Height = target.Height;
                Banner.X = 0;
                Banner.Y = 0;
                Banner.Width = Bounds.Width;
                Banner.Height = 20;
                target.DrawRectangle(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height, PrimaryColour, true);
            }

            if (BackgroundImage != null)
            {
                if (BackgroundImage.Width != Bounds.Width || BackgroundImage.Height != Bounds.Height)
                {
                    BackgroundImage.ResizeImage(Bounds.Width, Bounds.Height);
                }
                target.DrawImage(Bounds.X, Bounds.Y, BackgroundImage);
            }

            AfterRedraw();

            if (Input.GetMouseButton(0) && (RenderArea.Contains(Input.MousePosition) || Banner.Contains(Input.MousePosition)))
            {
                SGI.CurrentForm = this;
            }
        }

        public abstract void InitializeComponents();

        public virtual void BeforeRedraw() { }

        public virtual void AfterRedraw() { }

        public void Close()
        {
            WindowState = FormWindowState.Closed;
        }

        public enum FormWindowState
        {
            Normal,
            Minimized,
            Maximized,
            FullScreen,
            Closed
        }
    }
}