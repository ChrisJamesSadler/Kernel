using Kernel.System.Core;

namespace Kernel.System.Collections
{
    public unsafe class CircularBuffer
    {
        public uint* buffer;
        public int Capacity;
        private int next;
        private int current;
        public int Space;

        public CircularBuffer(int size)
        {
            Capacity = size;
            Space = size;
            buffer = (uint*)Heap.alloc((uint)size * 4);
        }

        public void Push(object obj)
        {
            Push(Utils.getPointer(obj));
        }

        public void Push(void* content)
        {
            Push((uint)content);
        }

        public void Push(bool content)
        {
            Push(content ? 1 : 0);
        }

        public void Push(int content)
        {
            Push((uint)content);
        }

        public void Push(uint content)
        {
            while (Space <= 0) { }
            Space--;
            buffer[next] = content;
            next = (next + 1) % Capacity;
        }

        public uint Pop()
        {
            if (Space != Capacity)
            {
                Space++;
                uint val = buffer[current];
                current = (current + 1) % Capacity;
                return val;
            }
            return 0;
        }

        public object PopObject()
        {
            uint got = Pop();
            if (got == 0)
            {
                return null;
            }
            return Utils.getHandler(got);
        }

        public bool Contains(void* content)
        {
            return Contains((uint)content);
        }

        public bool Contains(bool content)
        {
            return Contains(content ? 1 : 0);
        }

        public bool Contains(int content)
        {
            return Contains((uint)content);
        }

        public bool Contains(uint content)
        {
            int c = current;
            while (c != next)
            {
                if (buffer[c] == content)
                {
                    return true;
                }
                c = (c + 1) % Capacity;
            }
            return false;
        }
    }
}