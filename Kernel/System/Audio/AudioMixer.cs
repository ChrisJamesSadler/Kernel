using Kernel.System.Collections;
using Kernel.System.Components;
using Kernel.System.Core;
using Kernel.System.HAL;
using System.Collections.Generic;

namespace Kernel.System.Audio
{
    public static unsafe class AudioMixer
    {
        public static List<AudioStream> audioStreams;
        private static int lastFreq;

        public static void Init()
        {
            audioStreams = new List<AudioStream>();
            Scheduler.TaskSwitchEvents.Add((uint)((aMethod)SelectNextTone).GetHashCode());
        }

        private static void SelectNextTone()
        {
            uint freq = 0;
            foreach (AudioStream stream in audioStreams)
            {
                freq += stream.samples.Pop();
            }
            if (freq != 0)
            {
                Speaker.sound(freq);
                lastFreq = (int)freq;
            }
            else
            {
                if (lastFreq <= 0)
                {
                    Speaker.nosound();
                }
                else
                {
                    Speaker.sound((uint)lastFreq);
                    lastFreq -= 50;
                }
            }
        }
    }
}