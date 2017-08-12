using Kernel.System.Core;
using Kernel.System.Drawing;

namespace Kernel.Application.Graphics
{
    public unsafe class TextBox : IControl
    {
        public int EnteredIndex = 0;
        public byte* EnteredContent;

        public TextBox()
        {
            EnteredContent = (byte*)Heap.alloc(128);
            Content = "";
            SetSize(100, 20);
            PrimaryColour = Theme.TextBoxPrimary;
            SecondaryColour = Theme.TextBoxSecondary;
        }

        public override void OnDraw(Bitmap target)
        {
            RenderArea.Width = Bounds.Width;
            RenderArea.Height = Bounds.Height;
            target.DrawRectangle(RenderArea.X, RenderArea.Y, Bounds.Width, Bounds.Height, PrimaryColour, true);
            target.DrawRectangle(RenderArea.X, RenderArea.Y, Bounds.Width, Bounds.Height, SecondaryColour, false);
            int posY = RenderArea.Y + (Bounds.Height / 2);
            posY -= 4;
            int posX = RenderArea.X + 2;
            for (int i = 0; i < EnteredIndex; i++)
            {
                target.DrawCharacter(posX, posY, (char)EnteredContent[i], Theme.TextBoxText);
                posX += GL.fontSize;
            }
        }
    }
}