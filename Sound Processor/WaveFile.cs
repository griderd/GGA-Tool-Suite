﻿// Generated by FileFormat 1.0.5.41
// FileFormat is available at http://www.gridersoftware.com/
// Generated on 3/22/2020 9:41:05 PM

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Sound_Processor
{
    public class WaveFile
    {
        private Encoding ENCODING = Encoding.UTF8;
        public const UInt32 MAGICNUMBER = 0x46464952;
        private const UInt32 FORMAT = 0x45564157;
        private const UInt32 SUBCHUNK1ID = 0x20746d66;
        private const UInt32 SUBCHUNK2ID = 0x61746164;

        public UInt32 Magic { get; private set; }
        public UInt32 ChunkSize { get; private set; }
        public UInt32 Format { get; private set; }
        public UInt32 Subchunk1id { get; private set; }
        public Int32 Subchunk1size { get; private set; }
        public Int16 Audioformat { get; private set; }
        public Int16 Numchannels { get; private set; }
        public Int32 Samplerate { get; private set; }
        public Int32 Byterate { get; private set; }
        public Int16 Blockalign { get; private set; }
        public Int16 Bitspersample { get; private set; }
        public UInt32 Subchunk2id { get; private set; }
        public Int32 Subchunk2size { get; private set; }
        public Byte[] Data { get; private set; }

        public WaveFile(Int16 numchannels, Int32 samplerate, Int16 bitspersample, Byte[] data)
        {
            Magic = MAGICNUMBER;
            ChunkSize = (uint)(36 + data.Length);
            Format = FORMAT;
            Subchunk1id = SUBCHUNK1ID;
            Subchunk1size = 16; // PCM
            Audioformat = 1; // PCM
            Numchannels = numchannels;
            Samplerate = samplerate;
            Byterate = samplerate * numchannels * (bitspersample / 8);
            Blockalign = (short)(numchannels * (bitspersample / 8));
            Bitspersample = bitspersample;
            Subchunk2id = SUBCHUNK2ID;
            Subchunk2size = data.Length;
            Data = data;
        }

        public WaveFile(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open), ENCODING))
                    {
                        Magic = reader.ReadUInt32();
                        ChunkSize = reader.ReadUInt32();
                        Format = reader.ReadUInt32();
                        Subchunk1id = reader.ReadUInt32();
                        Subchunk1size = reader.ReadInt32();
                        Audioformat = reader.ReadInt16();
                        Numchannels = reader.ReadInt16();
                        Samplerate = reader.ReadInt32();
                        Byterate = reader.ReadInt32();
                        Blockalign = reader.ReadInt16();
                        Bitspersample = reader.ReadInt16();
                        Subchunk2id = reader.ReadUInt32();
                        Subchunk2size = reader.ReadInt32();
                        Data = reader.ReadBytes(Subchunk2size);

                        reader.Close();
                    }
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
            catch
            {
                throw;
            }

            if (Magic != MAGICNUMBER)
                throw new FormatException("RIFF header is not valid.");
            if (Format != FORMAT)
                throw new FormatException("Format is not \"WAVE\".");
            if (Subchunk1id != SUBCHUNK1ID)
                throw new FormatException("FMT header is not valid.");
            if (Subchunk2id != SUBCHUNK2ID)
                throw new FormatException("DATA header is not valid.");

            if (Subchunk1size != 16)
                throw new FormatException("Format is not PCM.");
            if (Audioformat != 1)
                throw new FormatException("Format is not PCM.");
            if (Samplerate != 44100)
                throw new FormatException("Only 44.1 kHz sample rate is supported.");
        }

        public void SaveFile(string filename, FileMode filemode)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(filename, filemode), ENCODING))
                {
                    writer.Write(MAGICNUMBER);
                    writer.Write(ChunkSize);
                    writer.Write(FORMAT);
                    writer.Write(SUBCHUNK1ID);
                    writer.Write(Subchunk1size);
                    writer.Write(Audioformat);
                    writer.Write(Numchannels);
                    writer.Write(Samplerate);
                    writer.Write(Byterate);
                    writer.Write(Blockalign);
                    writer.Write(Bitspersample);
                    writer.Write(SUBCHUNK2ID);
                    writer.Write(Subchunk2size);
                    for (int i = 0; i < Subchunk2size; i++)
                    {
                        writer.Write(Data[i]);
                    }

                    writer.Close();
                }
            }
            catch
            {
                throw;
            }
        }

        public byte[] ToBinary()
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(MAGICNUMBER);
                    writer.Write(ChunkSize);
                    writer.Write(FORMAT);
                    writer.Write(SUBCHUNK1ID);
                    writer.Write(Subchunk1size);
                    writer.Write(Audioformat);
                    writer.Write(Numchannels);
                    writer.Write(Samplerate);
                    writer.Write(Byterate);
                    writer.Write(Blockalign);
                    writer.Write(Bitspersample);
                    writer.Write(SUBCHUNK2ID);
                    writer.Write(Subchunk2size);
                    for (int i = 0; i < Subchunk2size; i++)
                    {
                        writer.Write(Data[i]);
                    }

                    writer.Close();

                    return stream.ToArray();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
