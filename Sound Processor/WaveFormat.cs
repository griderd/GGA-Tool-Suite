using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sound_Processor
{
    public class WaveFormat
    {
        public WaveFormat(Samples samples, ushort channels = 1, uint sampleRate = 44100)
        {
            SampleRate = sampleRate;
            NumberOfChannels = channels;
            AssignSamples(samples.Data);
        }

        public WaveFormat(byte[] file)
        {
            
        }

        #region RIFF Header
        /// <summary>
        /// "RIFF" header
        /// </summary>
        byte[] chunkID = new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' };

        /// <summary>
        /// Size of the file in bytes, minus 8 bytes
        /// </summary>
        public uint chunkSize
        {
            get
            {
                return (uint)(36 + subchunk2Size);
            }
        }
    
        /// <summary>
        /// "WAVE" format header
        /// </summary>
        byte[] format = new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' };

        #endregion

        #region Subchunk 1

        byte[] subchunk1ID = new byte[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' };
        uint subchunk1Size = 16;    // PCM
        ushort audioFormat = 1;     // PCM

        public ushort NumberOfChannels { get; set; }
        public uint SampleRate { get; set; }

        uint ByteRate 
        { 
            get 
            { 
                return (uint)(SampleRate * NumberOfChannels * (bitsPerSample / 8)); 
            } 
        }

        ushort BlockAlign 
        { 
            get 
            { 
                return (ushort)(NumberOfChannels * (bitsPerSample / 8)); 
            }
        }

        ushort bitsPerSample = 16;

        #endregion

        #region Data subchunk

        byte[] subchunk2ID = new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' };

        uint subchunk2Size
        {
            get 
            { 
                return (uint)data.Length;
            } 
        }


        byte[] data;
        public void AssignSamples(short[] samples)
        {
            data = new byte[samples.Length * 2];
            Buffer.BlockCopy(samples, 0, data, 0, samples.Length * 2);
        }

        #endregion

        public byte[] ToBinary()
        {  
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(chunkID);
                writer.Write(chunkSize);
                writer.Write(format);
                writer.Write(subchunk1ID);
                writer.Write(subchunk1Size);
                writer.Write(audioFormat);
                writer.Write(NumberOfChannels);
                writer.Write(SampleRate);
                writer.Write(ByteRate);
                writer.Write(BlockAlign);
                writer.Write(bitsPerSample);
                writer.Write(subchunk2ID);
                writer.Write(subchunk2Size);
                writer.Write(data);

                return stream.ToArray();
            }
        }
    }
}
