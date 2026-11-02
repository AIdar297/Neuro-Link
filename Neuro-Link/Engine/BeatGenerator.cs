using Microsoft.VisualBasic;
using NAudio.Wave;
using System;
using System.Threading;

namespace Neuro_Link.Engine
{
    public class BeatGenerator
    {
        private WaveOutEvent _waveOut;
        private BufferedWaveProvider _bufferedWaveProvider;
        public bool IsPlaying { get; private set; } = false;

        public void StartCyberpunkBeat()
        {
            if (IsPlaying) return;
            int sampleRate = 44100;
            _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(sampleRate, 1));
            _bufferedWaveProvider.BufferLength = sampleRate * 2;
            _waveOut = new WaveOutEvent();
            _waveOut.Init(_bufferedWaveProvider);
            Thread t = new Thread(() => Generate(sampleRate)) { IsBackground = true };
            t.Start();
            _waveOut.Play();
            IsPlaying = true;
        }

        private void Generate(int sampleRate)
        {
            double bpm = 120;
            int samplesPerBeat = (int)(sampleRate * 60.0 / bpm);
            int sampleIndex = 0;
            Random rnd = new Random();
            while (IsPlaying)
            {
                byte[] buffer = new byte[sampleRate * 2];
                int idx = 0;
                for (int i = 0; i < sampleRate; i++)
                {
                    int beat = sampleIndex / samplesPerBeat;
                    int inBeat = sampleIndex % samplesPerBeat;
                    double s = 0;
                    double t = (double)i / sampleRate;
                    if (inBeat < sampleRate * 0.1)
                        s += Math.Sin(2 * Math.PI * (150 - t * 1000) * t) * (1 - t * 10) * 0.5;
                    s += Math.Sin(2 * Math.PI * 55 * ((double)sampleIndex / sampleRate)) * 0.3;
                    if (beat % 2 == 1 && inBeat < sampleRate * 0.02)
                        s += (rnd.NextDouble() * 2 - 1) * 0.2;
                    short sh = (short)(s * 32767 * 0.7);
                    buffer[idx++] = (byte)(sh & 0xFF);
                    buffer[idx++] = (byte)((sh >> 8) & 0xFF);
                    sampleIndex++;
                }
                _bufferedWaveProvider.AddSamples(buffer, 0, idx);
                Thread.Sleep(800);
            }
        }

        public void Stop()
        {
            IsPlaying = false;
            _waveOut?.Stop();
        }
    }
}