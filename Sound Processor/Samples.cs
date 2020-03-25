using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sound_Processor
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Samples : ISerializable
    {
        short[] samples;

        public Samples()
        {
            samples = new short[0];
        }

        public Samples(short[] samples)
        {
            this.samples = samples;
        }

        public Samples(SerializationInfo info, StreamingContext context)
        {
            samples = (short[])info.GetValue("samples", typeof(short[]));
        }

        public short[] Data { get { return samples; } set { samples = value; } }

        /// <summary>
        /// Returns a value between -1 and 1.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public float GetFloat(int index)
        {
            if (Data[index] >= 0)
                return (float)Data[index] / short.MaxValue;
            else
                return (float)Data[index] / -short.MinValue;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("samples", samples);
        }

        public byte[] Binary
        {
            get
            {
                byte[] data = new byte[samples.Length * 2];
                Buffer.BlockCopy(samples, 0, data, 0, samples.Length * 2);
                return data;
            }

            set
            {
                byte[] data = value;
                if (data == null)
                    throw new NullReferenceException();
                samples = new short[data.Length / 2];
                if (samples.Length > 0)
                    Buffer.BlockCopy(data, 0, samples, 0, data.Length);
            }
        }
    }
}
