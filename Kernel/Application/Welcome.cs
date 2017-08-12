using Kernel.Application.Graphics;

namespace Kernel.Application
{
    public class Welcome : Form
    {
        public override void InitializeComponents()
        {
            WindowState = FormWindowState.Normal;
            SetSize(500, 350);
            CentreToScreen();
            Content = "Welcome Screen";
        }
    }
}