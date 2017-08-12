namespace Kernel.Application.Graphics
{
    public class Panel : IControl
    {
        public Panel()
        {
            PrimaryColour = Theme.PanelPrimary;
            SecondaryColour = Theme.PanelSecondary;
        }

        public override void OnDraw(Bitmap target)
        {
            RenderArea.Width = Bounds.Width;
            RenderArea.Height = Bounds.Height;
            target.DrawRectangle(RenderArea.X, RenderArea.Y, Bounds.Width, Bounds.Height, PrimaryColour, true);
            target.DrawRectangle(RenderArea.X, RenderArea.Y, Bounds.Width, Bounds.Height, SecondaryColour, false);
        }
    }
}