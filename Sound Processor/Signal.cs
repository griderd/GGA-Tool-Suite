using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sound_Processor
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    /// <summary>
    /// Represents a 16-bit floating-point signal.
    /// </summary>
    public class Signal : ISerializable
    {
        float[] values;

        public int Length { get { return values.Length; } }

        public float[] Values { get { return values; } }

        public float this[int index]
        {
            get
            {
                if ((index >= 0) && (index < Length))
                {
                    return values[index];
                }
                
                throw new IndexOutOfRangeException();
            }
            set
            {
                if ((index >= 0) && (index < Length))
                {
                    values[index] = value;
                }
            }
        }

        public Signal(SerializationInfo info, StreamingContext context)
        {
            values = (float[])info.GetValue("values", typeof(float[]));
        }

        public Signal()
        {
            values = new float[0];
        }

        public Signal(int length)
        {
            if (length > 0)
                values = new float[length];
            else
                throw new ArgumentOutOfRangeException("Length must be greater than zero.");
        }

        public Signal(float[] values)
        {
            if (values == null)
                throw new ArgumentNullException();
            else if (values.Length == 0)
                throw new ArgumentOutOfRangeException("Length must be greater than zero.");
            else
                this.values = values;
        }

        public Samples ToSamples(short scale = short.MaxValue)
        {
            short[] data = new short[Length];

            for (int i = 0; i < Length; i++)
            {
                float value = values[i] * scale;

                if (value > short.MaxValue)
                    value = short.MaxValue;
                else if (value < short.MinValue)
                    value = short.MinValue;

                data[i] = (short)value;
            }

            return new Samples(data);
        }

        public static Signal FromSamples(Samples samples, short scale = short.MaxValue)
        {
            Signal signal = new Signal(samples.Data.Length);

            for (int i = 0; i < signal.Length; i++)
            {
                signal[i] = (float)samples.Data[i] / (float)scale;
            }

            return signal;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("values", values);
        }

        public Signal PadWithZeros()
        {
            int len = IsPowOf2(Length) ? Length : NextPowOf2(Length);

            Signal baseSignal = new Signal(len);

            Buffer.BlockCopy(values, 0, baseSignal.values, 0, Length);

            return baseSignal;
        }

        /// <summary>
        /// Determines if a value is a power of 2.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        bool IsPowOf2(int num)
        {
            if (num == 0) return false;

            return (int)Math.Ceiling(Math.Log(num, 2f)) == (int)Math.Floor(Math.Log(num, 2f));
        }

        /// <summary>
        /// Gets the smallest power of 2 greater or equal to a number.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        int NextPowOf2(int num)
        {
            int pos = (int)Math.Ceiling(Math.Log(num, 2));
            return (int)Math.Pow(2, pos);
        }

        public double[] ToDoubleArray()
        {
            double[] data = new double[Length];

            for (int i = 0; i < Length; i++)
            {
                data[i] = values[i];
            }

            return data;
        }
    }
}
