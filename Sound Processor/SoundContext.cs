using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.IO;
using System.Windows.Forms;
using System.Numerics;
using NodeEditor;
using MathNet.Numerics.IntegralTransforms;

namespace Sound_Processor
{
    public class SoundContext : INodesContext
    {
        const int SAMPLE_RATE = 44100;
        const int BITS_PER_SAMPLE = 16;

        public SoundContext()
        {
            
        }

        public NodeVisual CurrentProcessingNode { get; set; }
        public event Action<string, NodeVisual, FeedbackType, object, bool> FeedbackInfo;

        float AngularFrequency(float frequency)
        {
            return (float)((Math.PI * 2f * frequency) / SAMPLE_RATE);
        }

        float Remap(float value, float inMin, float inMax, float outMin, float outMax)
        {
            return ((value - inMin) / (inMax - inMin)) * (outMax - outMin) + outMin;
        }

        #region Math

        [Node("Constant", "Math", "General", "Provides a constant number.", false)]
        public void Constant(float value, Signal lenSignal, out Signal signal, float length = 1)
        {
            Signal result = new Signal((int)((lenSignal.Length == 0 ? length : lenSignal[0]) * SAMPLE_RATE));

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = value;
            }

            signal = result;
        }

        [Node("Int16.Max", "Math", "General", "Provides a constant number.", false)]
        public void Int16Max(out Signal signal, Signal lenSignal, float length = 1)
        {
            Signal result = new Signal((int)((lenSignal.Length == 0 ? length : lenSignal[0]) * SAMPLE_RATE));

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = short.MaxValue;
            }

            signal = result;
        }

        [Node("Int16.Min", "Math", "General", "Provides a constant number.", false)]
        public void Int16Min(out Signal signal, Signal lenSignal, float length = 1)
        {
            Signal result = new Signal((int)((lenSignal.Length == 0 ? length : lenSignal[0]) * SAMPLE_RATE));

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = short.MinValue;
            }

            signal = result;
        }

        [Node("Remap", "Math", "General", "Remaps a signal to another range.", false)]
        public void Remap(Signal signalIn, float inLow, float inHigh, float outLow, float outHigh, out Signal signal)
        {
            Signal result = new Signal(signalIn.Length);

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Remap(signalIn[i], inLow, inHigh, outLow, outHigh);
            }

            signal = result;
        }

        [Node("Linear", "Math", "General", "Creates a linear range.", false)]
        public void Linear(float slope, float yIntercept, Signal lenSignal, out Signal signal, float length = 1 )
        {
            Signal result = new Signal((int)((lenSignal.Length == 0 ? length : lenSignal[0]) * SAMPLE_RATE));

            float sampleSlope = slope / SAMPLE_RATE;

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (sampleSlope * i) + yIntercept;
            }

            signal = result;
        }

        [Node("Exponential", "Math", "General", "Creates a exponential range.", false)]
        public void Exponential(float a, float b, Signal lenSignal, out Signal signal, float length = 1)
        {
            Signal result = new Signal((int)((lenSignal.Length == 0 ? length : lenSignal[0]) * SAMPLE_RATE));

            // TODO: Check if b is positive

            for (int i = 0; i < result.Length; i++)
            {
                float x = (float)i / SAMPLE_RATE;
                result[i] = (float)(a * Math.Pow(b, x));
            }

            signal = result;
        }

        [Node("Quadratic", "Math", "General", "Creates a quadratic range.", false)]
        public void Quadratic(float a, float b, Signal lenSignal, out Signal signal, float length = 1)
        {
            Signal result = new Signal((int)((lenSignal.Length == 0 ? length : lenSignal[0]) * SAMPLE_RATE));

            for (int i = 0; i < result.Length; i++)
            {
                float x = (float)i / SAMPLE_RATE;
                result[i] = (float)((a * x * x) + (b * x));
            }

            signal = result;
        }

        #endregion

        #region Output

        [Node("Process", "Output", "General", "Processes the sound.", true, true)]
        public void Process(Signal signal, out Samples samplesOut, short scale = short.MaxValue, bool play = true, bool export = true, string path = "MySound.wav")
        {
            if (signal == null)
            {
                samplesOut = new Samples();
                return;
            }

            samplesOut = signal.ToSamples(scale);
            WaveFile wave = new WaveFile(1, SAMPLE_RATE, 16, samplesOut.Binary);

            if (play)
            {
                using (MemoryStream stream = new MemoryStream())
                using (SoundPlayer player = new SoundPlayer(stream))
                {
                    byte[] data = wave.ToBinary();
                    stream.Write(data, 0, data.Length);
                    player.Stream.Position = 0;
                    player.Play();
                }
            }

            if (export)
            {
                try
                {
                    wave.SaveFile(path, FileMode.Create);
                }
                catch
                {
                    MessageBox.Show("Error writing file.");
                }
            }
        }

        #endregion

        #region Tones
        [Node("Sine Wave", "Tone", "General", "Generates a sine wave tone.", false)]
        public void SinWave(out Signal signal, Signal freqSignal, Signal ampSignal, Signal lenSignal, float frequency = 261.6f, float amplitude = 0.5f, float length = 1f, float delay = 0f)
        {
            Signal result = new Signal((int)((lenSignal.Length == 0 ? length + delay : lenSignal[0] + delay) * SAMPLE_RATE));

            for (int i = (int)(delay * SAMPLE_RATE); i < result.Length; i++)
            {
                float freq = freqSignal.Length == 0 ? frequency : (freqSignal.Length <= i ? freqSignal[freqSignal.Length - 1] : freqSignal[i]);
                float amp = ampSignal.Length == 0 ? amplitude : (ampSignal.Length <= i ? ampSignal[ampSignal.Length - 1] : ampSignal[i]);

                float value = (float)(amp * Math.Sin(AngularFrequency(freq) * i));
                result[i] = value;
            }

            signal = result;
        }

        [Node("Square Wave", "Tone", "General", "Generates a square wave tone.", false)]
        public void SquareWave(out Signal signal, Signal freqSignal, Signal ampSignal, Signal lenSignal, float frequency = 261.6f, float amplitude = 0.5f, float length = 1f, float delay = 0f)
        {
            Signal result = new Signal((int)((lenSignal.Length == 0 ? length + delay : lenSignal[0] + delay) * SAMPLE_RATE));

            for (int i = (int)(delay * SAMPLE_RATE); i < result.Length; i++)
            {
                float freq = freqSignal.Length == 0 ? frequency : (freqSignal.Length <= i ? freqSignal[freqSignal.Length - 1] : freqSignal[i]);
                float amp = ampSignal.Length == 0 ? amplitude : (ampSignal.Length <= i ? ampSignal[ampSignal.Length - 1] : ampSignal[i]);

                float value = amp * Math.Sign(Math.Sin(AngularFrequency(freq) * i));
                result[i] = value;
            }

            signal = result;
        }

        [Node("Sawtooth Wave", "Tone", "General", "Generates a sawtooth wave tone.", false)]
        public void SawtoothWave(out Signal signal, Signal freqSignal, Signal ampSignal, Signal lenSignal, float frequency = 261.6f, float amplitude = 0.5f, float length = 1f, float delay = 0f)
        {
            Signal result = new Signal((int)((lenSignal.Length == 0 ? length + delay : lenSignal[0] + delay) * SAMPLE_RATE));

            for (int i = (int)(delay * SAMPLE_RATE); i < result.Length; i++)
            {
                float freq = freqSignal.Length == 0 ? frequency : (freqSignal.Length <= i ? freqSignal[freqSignal.Length - 1] : freqSignal[i]);
                float amp = ampSignal.Length == 0 ? amplitude : (ampSignal.Length <= i ? ampSignal[ampSignal.Length - 1] : ampSignal[i]);
                float x = (float)i / (float)SAMPLE_RATE;

                float period = 1f / freq;
                float value = (float)(((-2f * amp) / Math.PI) * Math.Atan(1f / Math.Tan((Math.PI * x) / period)));

                result[i] = value;
            }

            signal = result;
        }

        [Node("Triangle Wave", "Tone", "General", "Generates a triangle wave tone.", false)]
        public void TriangleWave(out Signal signal, Signal freqSignal, Signal ampSignal, Signal lenSignal, float frequency = 261.6f, float amplitude = 0.5f, float length = 1f, float delay = 0f)
        {
            Signal result = new Signal((int)((lenSignal.Length == 0 ? length + delay : lenSignal[0] + delay) * SAMPLE_RATE));

            for (int i = (int)(delay * SAMPLE_RATE); i < result.Length; i++)
            {
                float freq = freqSignal.Length == 0 ? frequency : (freqSignal.Length <= i ? freqSignal[freqSignal.Length - 1] : freqSignal[i]);
                float amp = ampSignal.Length == 0 ? amplitude : (ampSignal.Length <= i ? ampSignal[ampSignal.Length - 1] : ampSignal[i]);
                float x = (float)i / (float)SAMPLE_RATE;

                float period = 1f / freq;
                float value = (float)(((2 * amp) / Math.PI) * Math.Asin(Math.Sin(((2 * Math.PI) / period) * x)));

                result[i] = value;
            }

            signal = result;
        }

        #endregion

        #region Noises

        [Node("White Noise", "Noise", "General", "Generates white noise.", false)]
        public void WhiteNoise(out Signal signal, Signal ampSignal, Signal lenSignal, float amplitude = 0.5f, float length = 1f, float delay = 0f)
        {
            Random rnd = new Random();

            Signal result = new Signal((int)((lenSignal.Length == 0 ? length + delay : lenSignal[0] + delay) * SAMPLE_RATE));

            for (int i = (int)(delay * SAMPLE_RATE); i < result.Length; i++)
            {
                float amp = ampSignal.Length == 0 ? amplitude : (ampSignal.Length <= i ? ampSignal[ampSignal.Length - 1] : ampSignal[i]);
                float value = Remap((float)rnd.NextDouble(), 0f, 1f, -amp, amp);

                result[i] = value;
            }

            signal = result;
        }

        #endregion

        #region Mixers

        [Node("Additive Mixer", "Mixers", "General", "Performs additive mixing of signals.", false)]
        public void AdditiveMixer(Signal a, Signal b, out Signal signal)
        {
            if ((a.Length == 0) | (b.Length == 0))
            {
                signal = new Signal(0);
                return;
            }

            int length = a.Length > b.Length ? a.Length : b.Length;

            Signal result = new Signal(length);

            for (int i = 0; i < length; i++)
            {
                float _a, _b;
                _a = _b = 0;

                if (i < a.Length) _a = a[i];
                if (i < b.Length) _b = b[i];

                result[i] = (_a + _b);
            }

            signal = result;
        }

        [Node("Multiply Mixer", "Mixers", "General", "Performs multiply mixing of signals.", false)]
        public void MultiplyMixer(Signal a, Signal b, out Signal signal)
        {
            if ((a.Length == 0) | (b.Length == 0))
            {
                signal = new Signal(0);
                return;
            }

            int length = a.Length > b.Length ? a.Length : b.Length;

            Signal result = new Signal(length);

            for (int i = 0; i < length; i++)
            {
                float _a, _b;
                _a = _b = 0;

                if (i < a.Length) _a = a[i];
                if (i < b.Length) _b = b[i];

                result[i] = (_a * _b);
            }

            signal = result;
        }

        [Node("Subtractive Mixer", "Mixers", "General", "Performs subtractive mixing of signals.", false)]
        public void SubtractiveMixer(Signal a, Signal b, out Signal signal)
        {
            if ((a.Length == 0) | (b.Length == 0))
            {
                signal = new Signal(0);
                return;
            }

            int length = a.Length > b.Length ? a.Length : b.Length;

            Signal result = new Signal(length);

            for (int i = 0; i < length; i++)
            {
                float _a, _b;
                _a = _b = 0;

                if (i < a.Length) _a = a[i];
                if (i < b.Length) _b = b[i];

                result[i] = (_a - _b);
            }

            signal = result;
        }

        [Node("Division Mixer", "Mixers", "General", "Performs division mixing of signals.", false)]
        public void DivisionMixer(Signal a, Signal b, out Signal signal)
        {
            if ((a.Length == 0) | (b.Length == 0))
            {
                signal = new Signal(0);
                return;
            }

            int length = a.Length > b.Length ? a.Length : b.Length;

            Signal result = new Signal(length);

            for (int i = 0; i < length; i++)
            {
                float _a, _b;
                _a = _b = 0;

                if (i < a.Length) _a = a[i];
                if (i < b.Length) _b = b[i];

                result[i] = (_a / _b);
            }

            signal = result;
        }

        [Node("Modulus Mixer", "Mixers", "General", "Performs division mixing of signals.", false)]
        public void ModulusMixer(Signal a, Signal b, out Signal signal)
        {
            if ((a.Length == 0) | (b.Length == 0))
            {
                signal = new Signal(0);
                return;
            }

            int length = a.Length > b.Length ? a.Length : b.Length;

            Signal result = new Signal(length);

            for (int i = 0; i < length; i++)
            {
                float _a, _b;
                _a = _b = 0;

                if (i < a.Length) _a = a[i];
                if (i < b.Length) _b = b[i];

                result[i] = (_a % _b);
            }

            signal = result;
        }

        [Node("Screen Mixer", "Mixers", "General", "Performs additive mixing of signals.", false)]
        public void ScreenMixer(Signal a, Signal b, out Signal signal)
        {
            if ((a.Length == 0) | (b.Length == 0))
            {
                signal = new Signal(0);
                return;
            }

            int length = a.Length > b.Length ? a.Length : b.Length;

            Signal result = new Signal(length);

            for (int i = 0; i < length; i++)
            {
                float _a, _b;
                _a = _b = 0;

                if (i < a.Length) _a = a[i];
                if (i < b.Length) _b = b[i];

                result[i] = -(-_a * -_b);

            }

            signal = result;
        }

        [Node("Overlay Mixer", "Mixers", "General", "Performs additive mixing of signals.", false)]
        public void OverlayMixer(Signal a, Signal b, out Signal signal)
        {
            if ((a.Length == 0) | (b.Length == 0))
            {
                signal = new Signal(0);
                return;
            }

            int length = a.Length > b.Length ? a.Length : b.Length;

            Signal result = new Signal(length);

            for (int i = 0; i < length; i++)
            {
                float _a, _b;
                _a = _b = 0;

                if (i < a.Length) _a = a[i];
                if (i < b.Length) _b = b[i];

                if (_a < 0.5)
                    result[i] = 2f * _a * _b;
                else
                    result[i] = 1f - 2f * (1f - _a) * (1f - _b);

            }

            signal = result;
        }

        #endregion

        #region Inputs

        [Node("File In", "Inputs", "General", "An existing wave file.", false)]
        public void FileIn(string path, out Signal signal)
        {
            if (path == "")
            {
                signal = new Signal(0);
                return;
            }

            try
            {
                WaveFile file = new WaveFile(path);

                short[] data = new short[file.Data.Length / 2];
                Buffer.BlockCopy(file.Data, 0, data, 0, file.Data.Length);
                Samples samples = new Samples(data);
                signal = Signal.FromSamples(samples);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                signal = new Signal();
            }
        }

        #endregion 

        #region Filters

        [Node("Invert", "Filters", "General", "Inverts a signal.", false)]
        public void Invert(Signal signalIn, out Signal signal)
        {
            if (signalIn.Length == 0)
            {
                signal = new Signal(0);
                return;
            }

            signal = new Signal(signalIn.Length);

            for (int i = 0; i < signalIn.Length; i++)
            {
                signal[i] = -signalIn[i];
            }
        }

        [Node("Volume Multiplier", "Filters", "General", "Changes the volume of the signal.", false)]
        public void Volume(Signal signalIn, out Signal signal, float volume = 0.5f)
        {
            if (signalIn.Length == 0)
            {
                signal = new Signal(0);
                return;
            }

            signal = new Signal(signalIn.Length);

            for (int i = 0; i < signalIn.Length; i++)
            {
                signal[i] = signalIn[i] * volume;
            }
        }

        [Node("Time Shift", "Filters", "General", "Changes the start position of the signal.", false)]
        public void TimeShift(Signal signalIn, float shift, out Signal signal)
        {
            if (signalIn.Length == 0)
            {
                signal = new Signal(0);
                return;
            }

            int length = signalIn.Length;
            int offset = (int)(shift * SAMPLE_RATE);
            length = length + offset;

            signal = new Signal(length);

            for (int i = offset; i < length; i++)
            {
                if (i + offset < 0)
                    continue;

                signal[i] = signalIn[i - offset];
            }
        }

        [Node("Amplitude Highpass", "Filters", "General", "Allows signal values above a certain amount to pass, sets the rest to floor.", false)]
        public void AmplitudeHighpass(Signal signalIn, float cutoff, float floor, out Signal signal)
        {
            if (signalIn.Length == 0)
            {
                signal = new Signal(0);
                return;
            }

            signal = new Signal(signalIn.Length);

            for (int i = 0; i < signalIn.Length; i++)
            {
                if (signalIn[i] >= cutoff)
                    signal[i] = signalIn[i];
                else
                    signal[i] = floor;
            }
        }

        [Node("Amplitude Lowpass", "Filters", "General", "Allows signal values below a certain amount to pass, sets the rest to floor.", false)]
        public void AmplitudeLowpass(Signal signalIn, float cutoff, float floor, out Signal signal)
        {
            if (signalIn.Length == 0)
            {
                signal = new Signal(0);
                return;
            }

            signal = new Signal(signalIn.Length);

            for (int i = 0; i < signalIn.Length; i++)
            {
                if (signalIn[i] <= cutoff)
                    signal[i] = signalIn[i];
                else
                    signal[i] = floor;
            }
        }

        #endregion 

        
        [Node("FFT", "FFT", "General", "FFT", false)]
        public void FFT(Signal signalIn, out Signal signal)
        {
            if (signalIn.Length == 0)
            {
                signal = new Signal(0);
                return;
            }

            float[] values = new float[signalIn.Length + 2];
            Buffer.BlockCopy(signalIn.Values, 0, values, 0, signalIn.Length);
            Fourier.ForwardReal(values, signalIn.Length);

            signal = new Signal(values);
        }

        [Node("Reverse FFT", "FFT", "General", "FFT", false)]
        public void ReverseFFT(Signal signalIn, out Signal signal)
        {
            if (signalIn.Length == 0)
            {
                signal = new Signal(0);
                return;
            }

            float[] values = new float[signalIn.Length + 2];
            Buffer.BlockCopy(signalIn.Values, 0, values, 0, signalIn.Length);
            Fourier.InverseReal(values, signalIn.Length);

            signal = new Signal(values);
        }
    }
}
