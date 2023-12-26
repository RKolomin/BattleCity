using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SlimDX.Multimedia;

namespace BattleCity.Audio.Decoders
{
    /// <summary>
    /// A read-only stream of WAVE data based on a wave file
    /// with an associated WaveFormat
    /// </summary>
    public class WaveFileReader : IAudioSource, IDisposable
    {
        private readonly bool convert8To16Bit = false;
        private readonly bool convert24To16Bit = false;
        private readonly bool convert32To16Bit = false;
        private readonly Hashtable metadata;
        private readonly int m_inputBitDepth;
        private readonly WaveFormat waveFormat;
        private readonly WaveFormat realWaveFormat;
        private Stream waveStream;
        private readonly bool ownInput;
        private readonly long dataPosition;
        private readonly int dataChunkLength;
        private readonly List<RiffChunk> chunks = new List<RiffChunk>();
        private readonly double diff = 1;
        private readonly double resizeCoeff = 1;
        private readonly long dataLength;
        private readonly long fsSize;

        /// <summary>Supports opening a WAV file</summary>
        /// <remarks>The WAV file format is a real mess, but we will only
        /// support the basic WAV file format which actually covers the vast
        /// majority of WAV files out there. For more WAV file format information
        /// visit www.wotsit.org. If you have a WAV file that can't be read by
        /// this class, email it to the nAudio project and we will probably
        /// fix this reader to support it
        /// </remarks>
        public WaveFileReader(string waveFile, long offset, bool convertTo16Bit) :
            this(new FileStream(waveFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), offset,
            convertTo16Bit, convertTo16Bit, convertTo16Bit)
        {
            ownInput = true;
        }

        public WaveFileReader(string waveFile, long offset,
            bool convert8To16Bit, bool convert24To16Bit, bool convert32To16Bit) :
            this(new FileStream(waveFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), offset,
            convert8To16Bit, convert24To16Bit, convert32To16Bit)
        {
            ownInput = true;
        }

        public long FileLength
        {
            get { return fsSize; }
        }

        /// <summary>
        /// Creates a Wave File Reader based on an input stream
        /// </summary>
        /// <param name="inputStream">The input stream containing a WAV file including header</param>
        public WaveFileReader(Stream inputStream, long offset)
            : this(inputStream, offset, true, true, true)
        {

        }


        public WaveFileReader(Stream waveStream, WaveFormat realWaveFormat)
        {
            this.waveStream = waveStream;
            this.realWaveFormat = realWaveFormat;
            waveFormat = realWaveFormat;
            dataLength = waveStream.Length;
            dataChunkLength = (int)dataLength;
            dataPosition = 0;
            m_inputBitDepth = realWaveFormat.BitsPerSample;
        }

        /// <summary>
        /// Creates a Wave File Reader based on an input stream
        /// </summary>
        /// <param name="inputStream">The input stream containing a WAV file including header</param>
        public WaveFileReader(Stream inputStream, long offset,
            bool convert8To16Bit, bool convert24To16Bit, bool convert32To16Bit)
        {
            inputStream.Position = offset;
            fsSize = inputStream.Length;
            this.convert8To16Bit = convert8To16Bit;
            this.convert24To16Bit = convert24To16Bit;
            this.convert32To16Bit = convert32To16Bit;

            waveStream = inputStream;
            try
            {
                ReadWaveHeader(waveStream, out realWaveFormat, out dataPosition, out dataChunkLength, chunks);
            }
            catch (Exception e)
            {
                Dispose();
                throw e;
            }
            foreach (RiffChunk chunk in chunks)
            {
                if (chunk.StreamPosition > dataPosition && chunk.StreamPosition > dataLength)
                    dataLength = chunk.StreamPosition;
            }
            if (dataLength == 0)
                dataLength = waveStream.Length - dataPosition;
            metadata = RiffChunkParser.ParseMeta(chunks, inputStream);
            m_inputBitDepth = realWaveFormat.BitsPerSample;
            bool convert =
                (realWaveFormat.BitsPerSample == 8 && convert8To16Bit) ||
                (realWaveFormat.BitsPerSample == 24 && convert24To16Bit) ||
                (realWaveFormat.BitsPerSample == 32 && convert32To16Bit);

            if (convert)
            {
                waveFormat = new WaveFormat()
                {
                    SamplesPerSecond = realWaveFormat.SamplesPerSecond,
                    BitsPerSample = 16,
                    Channels = realWaveFormat.Channels,
                    BlockAlignment = (short)((16 / 8) * realWaveFormat.Channels)
                };

                diff = 16.0d / realWaveFormat.BitsPerSample;
                resizeCoeff = realWaveFormat.BitsPerSample / 16.0d;
            }
            else
            {
                waveFormat = realWaveFormat;
                diff = 1;
                resizeCoeff = 1;
            }

            Position = 0;
        }

        /// <summary>
        /// Reads the header part of a WAV file from a stream
        /// </summary>
        /// <param name="stream">The stream, positioned at the start of audio data</param>
        /// <param name="format">The format found</param>
        /// <param name="dataChunkPosition">The position of the data chunk</param>
        /// <param name="dataChunkLength">The length of the data chunk</param>
        /// <param name="chunks">Additional chunks found</param>
        public static void ReadWaveHeader(Stream stream, out WaveFormat format, out long dataChunkPosition, out int dataChunkLength, List<RiffChunk> chunks)
        {
            dataChunkPosition = -1;
            BinaryReader br = new BinaryReader(stream);
            if (br.ReadUInt32() != FourCC.Get('R', 'I', 'F', 'F')) //Native.mmioStringToFOURCC("RIFF", 0))
            {
                throw new FormatException("Not a WAVE file - no RIFF header");
            }
            uint fileSize = br.ReadUInt32(); // read the file size (minus 8 bytes)
            if (br.ReadUInt32() != FourCC.Get('W', 'A', 'V', 'E'))
            {
                throw new FormatException("Not a WAVE file - no WAVE header");
            }

            uint f = br.ReadUInt32();
            if (f == 1954047330)//bext  =broadcast
            {
                f = br.ReadUInt32();
                br.BaseStream.Position += f;
                f = br.ReadUInt32();
            }

            // now we expect the format chunk
            if (f != FourCC.Get('f', 'm', 't', ' '))
            {
                throw new FormatException("Not a WAVE file - no fmt header");
            }
            //format = new WaveFormatExtraData(br);
            format = ReadWaveFormat(br);

            int dataChunkID = (int)FourCC.Get('d', 'a', 't', 'a');
            dataChunkLength = 0;

            // sometimes a file has more data than is specified after the RIFF header
            long stopPosition = Math.Min(fileSize + 8, stream.Length);

            // this -8 is so we can be sure that there are at least 8 bytes for a chunk id and length
            while (stream.Position < stopPosition - 8)
            {
                int chunkIdentifier = br.ReadInt32();
                int chunkLength = br.ReadInt32();
                if (chunkIdentifier == dataChunkID)
                {
                    dataChunkPosition = stream.Position;
                    dataChunkLength = chunkLength;
                }
                else
                {
                    chunks?.Add(new RiffChunk(chunkIdentifier, chunkLength, stream.Position));
                }
                stream.Position += chunkLength;
            }
        }

        /// <summary>
        /// Reads a new WaveFormat object from a stream
        /// </summary>
        /// <param name="br">A binary reader that wraps the stream</param>
        public static WaveFormat ReadWaveFormat(BinaryReader br)
        {
            int formatChunkLength = br.ReadInt32();
            if (formatChunkLength < 16)
                throw new ApplicationException("Invalid WaveFormat Structure");
            WaveFormat fmt = new WaveFormat
            {
                FormatTag = (WaveFormatTag)br.ReadUInt16(),
                Channels = br.ReadInt16(),
                SamplesPerSecond = br.ReadInt32(),
                AverageBytesPerSecond = br.ReadInt32(),
                BlockAlignment = br.ReadInt16(),
                BitsPerSample = br.ReadInt16()
            };
            if (formatChunkLength > 16)
            {
                var extraSize = (short)(formatChunkLength - 16);
                br.BaseStream.Position += extraSize;
            }

            return fmt;
        }

        /// <summary>
        /// Gets a list of the additional chunks found in this file
        /// </summary>
        public List<RiffChunk> ExtraChunks
        {
            get { return chunks; }
        }

        public Hashtable Metadata
        {
            get { return metadata; }
        }

        /// <summary>
        /// Gets the data for the specified chunk
        /// </summary>
        public byte[] GetChunkData(RiffChunk chunk)
        {
            long oldPosition = waveStream.Position;
            waveStream.Position = chunk.StreamPosition;
            byte[] data = new byte[chunk.Length];
            waveStream.Read(data, 0, data.Length);
            waveStream.Position = oldPosition;
            return data;
        }

        /// <summary>
        /// Cleans up the resources associated with this WaveFileReader
        /// </summary>
        public void Dispose()
        {
            // Release managed resources.
            if (waveStream != null)
            {
                // only dispose our source if we created it
                if (ownInput)
                {
                    waveStream.Close();
                    waveStream.Dispose();
                }
                waveStream = null;
            }
        }

        /// <summary>
        /// <see cref="WaveStream.WaveFormat"/>
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }

        public long Length
        {
            get { return (long)(dataChunkLength * diff); }
        }

        public long Remaining
        {
            get { return Length - Position; }
        }

        public bool IsEof => Position >= Length;

        /// <summary>
        /// Position in the wave file
        /// <see cref="Stream.Position"/>
        /// </summary>
        public long Position
        {
            get { return (long)((waveStream.Position - dataPosition) * diff); }
            set
            {
                lock (this)
                {
                    value = (long)Math.Min(value, dataLength);
                    // make sure we don't get out of sync
                    value = (long)(value * resizeCoeff);
                    value -= (value % realWaveFormat.BlockAlignment);
                    waveStream.Position = value + dataPosition;
                }
            }
        }


        /// <summary>
        /// Reads bytes from the Wave File
        /// <see cref="Stream.Read"/>
        /// </summary>
        public int Read(byte[] arrayDest, int offset, int count)
        {
            if (count % waveFormat.BlockAlignment != 0)
                throw new ApplicationException("Must read complete blocks");
            // sometimes there is more junk at the end of the file past the data chunk
            if (Position + count > dataChunkLength)
                count = dataChunkLength - (int)Position;
            /*if (m_inputBitDepth == 16 || !convert24To16Bit && !convert32To16Bit)
                return waveStream.Read(arrayDest, offset, count);
            else*/

            switch (m_inputBitDepth)
            {
                case 8:
                    if (convert8To16Bit)
                        return Read8BitTo16Bit(arrayDest, offset, count);
                    return waveStream.Read(arrayDest, offset, count);
                case 16:
                    return waveStream.Read(arrayDest, offset, count);
                case 24:
                    if (convert24To16Bit)
                        return Read24BitTo16Bit(arrayDest, offset, count);
                    return waveStream.Read(arrayDest, offset, count);
                case 32:
                    if (convert32To16Bit)
                    {
                        if (realWaveFormat.FormatTag == WaveFormatTag.IeeeFloat)
                            return Read32BitFloatTo16Bit(arrayDest, offset, count);
                        return Read32BitTo16Bit(arrayDest, offset, count);
                    }
                    return waveStream.Read(arrayDest, offset, count);

            }
            return 0;
        }

        unsafe int Read8BitTo16Bit(byte[] arrayDest, int offset, int count)
        {
            int total = 0;
            int tmp = 0;
            fixed (byte* p = arrayDest)
            {
                short* sample = (short*)(p + offset);
                ushort val;
                for (total = 0; total < count; total += 1)
                {
                    tmp = waveStream.ReadByte();
                    if (tmp == -1) break;
                    val = (ushort)(tmp * 257);
                    *sample++ = (short)(val - short.MaxValue);
                }
            }
            return total;
        }

        unsafe int Read24BitTo16Bit(byte[] arrayDest, int offset, int count)
        {
            int total = 0;
            int sample;
            byte[] channel = new byte[4];
            fixed (byte* p = arrayDest)
            {
                byte* zbuffer = p + offset;
                for (total = 0; total < count; total += 2)
                {
                    sample = waveStream.ReadByte();
                    if (sample == -1) break;
                    channel[0] = (byte)sample;

                    sample = waveStream.ReadByte();
                    if (sample == -1) break;
                    channel[1] = (byte)sample;

                    sample = waveStream.ReadByte();
                    if (sample == -1) break;
                    channel[2] = (byte)sample;

                    sample = BitConverter.ToInt32(channel, 0);

                    *(zbuffer++) = (byte)((sample >> 8) & 0xFF);
                    *(zbuffer++) = (byte)((sample >> 16) & 0xFF);
                }
            }
            return total;
        }

        private readonly byte[] channel = new byte[4];

        unsafe int Read32BitTo16Bit(byte[] arrayDest, int offset, int count)
        {
            int total = 0;
            int bytes = 0;
            //int sample;
            fixed (byte* p = arrayDest)
            {
                short* zbuffer = (short*)(p + offset);
                //byte* zbuffer = (byte*)(p + offset);
                for (total = 0; total < count; total += 2)
                {
                    bytes = waveStream.Read(channel, 0, 4);
                    if (bytes < 4) break;
                    // Create smaller array in order to add the 4th 8-bit value
                    *(zbuffer++) = (short)Math.Max(-32767, Math.Min(32768, BitConverter.ToInt16(channel, 2)));
                }
            }
            return total;
        }

        unsafe int Read32BitFloatTo16Bit(byte[] arrayDest, int offset, int count)
        {
            int total = 0;
            int bytes = 0;
            fixed (byte* p = arrayDest)
            {
                short* zbuffer = (short*)(p + offset);
                for (total = 0; total < count; total += 2)
                {
                    // Create smaller array in order to add the 4th 8-bit value
                    bytes = waveStream.Read(channel, 0, 4);
                    if (bytes < 4) break;
                    *(zbuffer++) = (short)Math.Max(-32767, Math.Min(32768, (BitConverter.ToSingle(channel, 0) * 32767)));
                }
            }
            return total;
        }

        // for the benefit of oggencoder we divide by 32768.0f;
        /// <summary>
        /// Reads floats into arrays of channels
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="samples">number of samples to read</param>
        /// <returns></returns>
        public int Read(float[][] buffer, int samples)
        {
            BinaryReader br = new BinaryReader(waveStream);
            if (waveFormat.BitsPerSample != 16)
                throw new ApplicationException("Only 16 bit audio supported");
            for (int sample = 0; sample < samples; sample++)
            {
                for (int channel = 0; channel < waveFormat.Channels; channel++)
                {
                    if (waveStream.Position < waveStream.Length)
                        buffer[channel][sample] = (float)br.ReadInt16() / 32768.0f;
                    else
                        return 0;
                }
            }
            return samples;
        }
    }
}
