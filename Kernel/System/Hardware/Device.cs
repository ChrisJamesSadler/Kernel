namespace Kernel.System.Hardware
{
    public abstract class Device
    {
        public abstract string Name();
        public virtual void Init() { }
    }
}