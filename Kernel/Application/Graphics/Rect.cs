namespace Kernel.Application.Graphics
{
    public class Rect : Point
    {
        public int Width;
        public int Height;

        public bool Contains(Point point)
        {
            if (point.X > X && point.X < X + Width)
                if (point.Y > Y && point.Y < Y + Height)
                    return true;
            return false;
        }
    }
}