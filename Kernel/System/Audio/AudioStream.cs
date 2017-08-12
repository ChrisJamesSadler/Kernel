using Kernel.System.Collections;
using Kernel.System.HAL;

namespace Kernel.System.Audio
{
    public class AudioStream
    {
        public CircularBuffer samples;
        
        public AudioStream(int maxSamples)
        {
            samples = new CircularBuffer(maxSamples);
            AudioMixer.audioStreams.Add(this);
        }

        public void AddSample(int sample)
        {
            while(samples.Space == 0) { }
            samples.Push(sample);
        }

        public void AddSample(int sample, int delay)
        {
            int timeBetween = (int)(1000 / PIT.IPS);
            while (delay >= 0)
            {
                delay -= timeBetween;
                AddSample(sample);
            }
        }
    }
}