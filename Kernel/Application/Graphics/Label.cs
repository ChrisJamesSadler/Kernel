using Kernel.System.Drawing;

namespace Kernel.Application.Graphics
{
    public class Label : IControl
    {
        public Label()
        {
            SetSize(100, 20);
            PrimaryColour = Theme.LabelPrimary;
            SecondaryColour = Theme.LabelSecondary;
        }

        public override void OnDraw(Bitmap target)
        {
            RenderArea.Width = Bounds.Width;
            RenderArea.Height = Bounds.Height;
            //target.DrawRectangle(RenderArea.X, RenderArea.Y, Bounds.Width, Bounds.Height, PrimaryColour, true);
            //target.DrawRectangle(RenderArea.X, RenderArea.Y, Bounds.Width, Bounds.Height, SecondaryColour, false);
            int posX = RenderArea.X + (Bounds.Width / 2);
            int posY = RenderArea.Y + (Bounds.Height / 2);
            posX -= ((Content.Length * GL.fontSize) / 2) + 3;
            posY -= (GL.fontSize / 2);
            target.DrawString(posX, posY, Content, Theme.ButtonText);
        }
    }
}