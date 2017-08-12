namespace Kernel.System.Events
{
    public abstract class EventListener
    {
        public EventListener()
        {
            Event.AllListeners.Add(this);
        }

        public virtual void OnKeyDown(char aChar)
        {

        }

        public virtual void OnMouseMove(int X, int Y)
        {

        }

        public virtual void OnMouseDown(int mouse)
        {

        }
    }
}