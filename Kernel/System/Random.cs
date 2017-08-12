using Kernel.System.Core;
using Kernel.System.HAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.System
{
    public static class Random
    {
        static int GrowingSeed = 541395498;

        static uint m_w = 362436069;
        static uint m_z = 521288629;
        static int last = 10;
        static bool initialized = false;

        public static int Next(int min, int max)
        {
            if (!initialized)
            {
                initialized = true;
                Scheduler.TaskSwitchEvents.Add((uint)((aMethod)Callback).GetHashCode());
            }
            m_z = 36969 * (m_z & 65535) + (m_z >> 16);// + Cosmos.HAL.RTC.Second;
            m_w = 18000 * (m_w & 65535) + (m_w >> 16);// + Cosmos.HAL.RTC.Minute;
            last += GrowingSeed + (int)((m_z << 16) + m_w);// + Cosmos.HAL.RTC.Hour;
            if (min == 0)
                min = max / 2;
            while (last < min)
                last += min;
            while (last > max)
                last -= max;
            return last;
        }

        private static void Callback()
        {
            GrowingSeed += Cosmos.HAL.RTC.Second;
            if (GrowingSeed >= 999999999)
            {
                GrowingSeed = 0;
            }
        }
    }
}
