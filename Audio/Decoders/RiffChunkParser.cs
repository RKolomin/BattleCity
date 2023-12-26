using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BattleCity.Audio.Decoders
{
    /// <summary>
    /// Анализатор Riff фрагментов
    /// </summary>
    public class RiffChunkParser
    {
        static readonly Encoding encoding = Encoding.Default;

        public static Hashtable ParseMeta(List<RiffChunk> chunks, Stream stream)
        {
            long position = stream.Position;
            Hashtable wav_meta_data = new Hashtable();
            foreach (RiffChunk chunk in chunks)
            {
                if (chunk.IdentifierAsString == "LIST")
                {
                    byte[] buffer = new byte[chunk.Length];
                    stream.Position = chunk.StreamPosition;
                    int len = stream.Read(buffer, 0, buffer.Length);
                    Parse(buffer, len, ref wav_meta_data);
                    stream.Position = position;
                    break;
                }
            }
            return wav_meta_data;
        }

        private static void Parse(byte[] buffer, int length, ref Hashtable wav_meta_data)
        {
            const int INFO = 0x4F464E49;

            try
            {
                int i = 0;
                string id = null;
                string value = null;
                uint len = 0;
                
                while (i < length)
                {
                    if (BitConverter.ToInt32(buffer, i) == INFO)
                    {
                        i += 4;
                        continue;
                    }
                    id = encoding.GetString(buffer, i, 4);
                    i += 4;
                    len = BitConverter.ToUInt32(buffer, i);
                    i += 4;
                    value = encoding.GetString(buffer, i, (int)len);
                    if (len % 2 != 0) len++;
                    i += (int)len;
                    if (string.IsNullOrEmpty(value))
                        continue;
                    value = value.TrimEnd('\0');
                    if (string.IsNullOrEmpty(value))
                        continue;
                    wav_meta_data.Add(id, value);
                }
            }
            catch (Exception) { }
        }
    }
}
