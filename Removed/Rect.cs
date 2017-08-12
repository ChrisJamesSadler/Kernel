namespace Kernel.System.Drawing
{
    public class Rect : Point
    {
        public int Width;
        public int Height;

        public Rect()
        {

        }

        public Rect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Containts(int x, int y)
        {
            if(x > X && y > Y)
            {
                if(x < X + Width && y < Y + Height)
                {
                    return true;
                }
            }
            return false;
        }
    }
}