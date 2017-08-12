using Kernel.System.Drivers.Other;

namespace Kernel.System.Drawing
{
    public delegate void OnClickListener();
    public class Button : IComponent
    {
        public OnClickListener onClick = null;

        public Button()
        {
            primaryColour = Theme.ButtonPrimary;
            secondaryColour = Theme.ButtonSecondary;
        }

        public override void OnDraw()
        {
            if (area.Width == 0 && area.Height == 0)
            {
                area.Height = 20;
                area.Width = 10 * content.Length;
            }
            Graphics.DrawRectangle(GlobalX, GlobalY, area.Width, area.Height, primaryColour);
            Graphics.DrawRectangleEmpty(GlobalX, GlobalY, area.Width, area.Height, secondaryColour);
            int XCentre = GlobalX;
            Graphics.DrawText(GlobalX + 1, GlobalY + 1, content, secondaryColour);
            if (Settings.CursorState == PS2Mouse.MouseState.Left && area.Containts(Settings.CursorX, Settings.CursorY))
            {
                onClick?.Invoke();
            }
        }
    }
}