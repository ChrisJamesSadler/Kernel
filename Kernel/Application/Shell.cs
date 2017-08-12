using Kernel.System;
using Kernel.System.Events;

namespace Kernel.Application
{
    public abstract class Shell : EventListener
    {
        public Shell()
        {
            Settings.ActiveShell = this;
        }

        public void Main() { }
    }
}