namespace Kernel.System.Collections
{
    public class BinaryMap
    {
        private readonly byte[] bitmap;
        private int setCount;

        public int Count
        {
            get
            {
                return setCount;
            }
        }

        public BinaryMap(int size)
        {
            bitmap = new byte[size / 8];
        }

        public void Set(int entry)
        {
            bitmap[entry / 8] = (byte)(bitmap[entry / 8] | (1 << (entry % 8)));
            setCount++;
        }
        
        public void Clear(int entry)
        {
            bitmap[entry / 8] = (byte)(bitmap[entry / 8] & ~(1 << (entry % 8)));
            setCount--;
        }
        
        public bool IsSet(int entry)
        {
            return (bitmap[entry / 8] & (1 << (entry % 8))) != 0;
        }
    }
}