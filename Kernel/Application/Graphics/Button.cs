using Kernel.System.Drawing;

namespace Kernel.Application.Graphics
{
    public delegate void OnClickListener();
    public class Button : IControl
    {
        public OnClickListener OnClick;

        public Button()
        {
            Content = "Button";
            SetSize(100, 20);
            PrimaryColour = Theme.ButtonPrimary;
            SecondaryColour = Theme.ButtonSecondary;
        }

        public override void OnDraw(Bitmap target)
        {
            RenderArea.Width = Bounds.Width;
            RenderArea.Height = Bounds.Height;
            if (RenderArea.Contains(Input.MousePosition))
            {
                target.DrawRectangle(RenderArea.X, RenderArea.Y, Bounds.Width, Bounds.Height, PrimaryColour + 0x090909, true);
            }
            else
            {
                target.DrawRectangle(RenderArea.X, RenderArea.Y, Bounds.Width, Bounds.Height, PrimaryColour, true);
            }
            target.DrawRectangle(RenderArea.X, RenderArea.Y, Bounds.Width, Bounds.Height, SecondaryColour, false);
            int posX = RenderArea.X + (Bounds.Width / 2);
            int posY = RenderArea.Y + (Bounds.Height / 2);
            posX -= ((Content.Length * GL.fontSize) / 2) + 3;
            posY -= (GL.fontSize / 2);
            target.DrawString(posX, posY, Content, Theme.ButtonText);
        }
    }
}