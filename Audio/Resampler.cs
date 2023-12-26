using System;

namespace BattleCity.Audio
{
    /// <summary>
    /// Ресемплер аудио данных
    /// </summary>
    public static class Resampler
    {
        /// <summary>
        /// передискретизация
        /// </summary>
        /// <param name="data"></param>
        /// <param name="srcSampleRate"></param>
        /// <param name="numChannels"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static byte[] Resample(byte[] data, int srcSampleRate, int numChannels)
        {
            if (data == null)
                return null;

            if (numChannels < 1 || numChannels > 2)
                throw new NotSupportedException($"Resampler: Not supported channels: {numChannels}");

            if (srcSampleRate == 44100 && numChannels == 2)
                return data;

            byte[] outputData;

            switch (srcSampleRate)
            {
                case 11025:
                    {
                        int dataLen = data.Length;
                        outputData = new byte[data.Length * 2];
                        UpSampleShort2X(dataLen, data, outputData, numChannels);
                        data = outputData;
                        dataLen *= 2;
                        outputData = new byte[data.Length * 2];
                        UpSampleShort2X(dataLen, data, outputData, numChannels);
                        break;
                    }
                case 22050:
                    {
                        outputData = new byte[data.Length * 2];
                        UpSampleShort2X(data.Length, data, outputData, numChannels);
                        break;
                    }
                case 44100:
                    outputData = data;
                    break;
                default:
                    throw new NotSupportedException($"Resampler: Not supported input samplerate: {srcSampleRate}");
            }

            if (numChannels == 1)
            {
                int dataLen = outputData.Length >> 1;
                AudioSamplesMulticast srcBuf = new AudioSamplesMulticast() { Bytes = outputData };
                AudioSamplesMulticast dstBuf = new AudioSamplesMulticast() { Bytes = new byte[outputData.Length * 2] };

                for (int i = 0, j = 0; i < dataLen; i++)
                {
                    dstBuf.Shorts[j++] = srcBuf.Shorts[i];
                    dstBuf.Shorts[j++] = srcBuf.Shorts[i];
                }

                outputData = dstBuf.Bytes;
            }

            return outputData;
        }


        /// <summary>
        /// Удвоение частоты дискритизации, например из 22050 в 44100
        /// </summary>
        /// <param name="sourceSize"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="channels"></param>
        static void UpSampleShort2X(int sourceSize, byte[] src, byte[] dst, int channels)
        {
            int size = sourceSize >> 1;  //size / 2 bytes => shorts
            int iy = 0;
            AudioSamplesMulticast srcBuf = new AudioSamplesMulticast() { Bytes = src };
            AudioSamplesMulticast dstBuf = new AudioSamplesMulticast() { Bytes = dst };

            if (channels == 2)
            {
                //size = size >> 1; //stereo mode

                for (int ix = 0; ix < size; ix += 2)
                {
                    dstBuf.Shorts[iy] = srcBuf.Shorts[ix];
                    dstBuf.Shorts[iy + 1] = srcBuf.Shorts[ix + 1];

                    dstBuf.Shorts[iy + 2] = (short)Math.Max(-32767, Math.Min(32767, ((int)srcBuf.Shorts[ix] + srcBuf.Shorts[ix + 2]) / 2));
                    dstBuf.Shorts[iy + 3] = (short)Math.Max(-32767, Math.Min(32767, ((int)srcBuf.Shorts[ix + 1] + srcBuf.Shorts[ix + 3]) / 2));

                    iy += 4;
                }
            }
            else if (channels == 1)
            {
                for (int ix = 0; ix < size; ix++)
                {
                    dstBuf.Shorts[iy++] = srcBuf.Shorts[ix];
                    dstBuf.Shorts[iy++] = (short)((srcBuf.Shorts[ix] + srcBuf.Shorts[ix + 1]) / 2);
                }
            }
        }
    }
}
