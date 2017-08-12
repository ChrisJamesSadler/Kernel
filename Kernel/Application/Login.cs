using Kernel.Application.Graphics;
using Kernel.System;
using Kernel.System.HAL;
using Kernel.System.Threading;

namespace Kernel.Application
{
    public unsafe class Login : Form
    {
        public Label lblTitle;
        public TextBox tbPassword;
        public Button btnLogin;
        public Button btnShutdown;

        public override void InitializeComponents()
        {
            WindowState = FormWindowState.FullScreen;
            SGI.ActiveForm = this;

            lblTitle = new Label();
            lblTitle.SetSize(100, 20);
            lblTitle.SetPosition((Settings.DisplayWidth / 2) - 50, (int)(Settings.DisplayHeight / 3));
            lblTitle.Content = "Unnamed Operating System";
            Add(lblTitle);

            tbPassword = new TextBox();
            tbPassword.SetSize((Settings.DisplayWidth / 2) - 50, 30);
            tbPassword.SetPosition((Settings.DisplayWidth / 4) + 25, Settings.DisplayHeight / 2);
            Add(tbPassword);

            btnLogin = new Button();
            btnLogin.SetSize(100, 20);
            btnLogin.SetPosition((Settings.DisplayWidth / 2) - 50, (int)(Settings.DisplayHeight / 1.7));
            btnLogin.OnClick = loginButtonPressed;
            btnLogin.Content = "Login";
            Add(btnLogin);

            btnShutdown = new Button();
            btnShutdown.SetSize(100, 20);
            btnShutdown.SetPosition((Settings.DisplayWidth / 2) - 50, (int)(Settings.DisplayHeight / 1.6));
            btnShutdown.OnClick = shutdownButtonPressed;
            btnShutdown.Content = "Shutdown";
            Add(btnShutdown);

            BackgroundImage = new Bitmap(255, 255);
        }

        public override void BeforeRedraw()
        {
            int r = 0;
            int g = 255;
            int b = 50 + RTC.Second;
            int steps = 1;
            bool skip = false;
            for (int i = 1; i <= BackgroundImage.Height / steps; i++)
            {
                uint colour = (uint)((r << 16) | (g << 8) | b);
                BackgroundImage.DrawRectangle(0, BackgroundImage.Height - (i * steps), BackgroundImage.Width, steps, colour);
                if (!skip)
                {
                    g--;
                }
                skip = !skip;
                if (g < 10)
                {
                    g = 10;
                }
            }
        }

        public void loginButtonPressed()
        {
            if (Utils.Compare(tbPassword.EnteredContent, "demo"))
            {
                formRendering.Close();
            }
        }

        public void shutdownButtonPressed()
        {
            ACPI.Shutdown();
        }
    }
}