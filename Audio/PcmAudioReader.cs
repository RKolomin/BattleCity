using BattleCity.Enums;
using SlimDX.Multimedia;
using System;

namespace BattleCity.Audio
{
    /// <summary>
    /// Считыватель PCM аудил данных
    /// </summary>
    public class PcmAudioReader : IAudioReader
    {
        private static readonly object syncObject = new object();
        private readonly WaveFormat waveFormat;

        private readonly int bytesPerSecond;
        private int position;
        private byte[] waveData;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="waveFormat">Формат аудио данных</param>
        /// <param name="audioStreamType">Тип потока</param>
        /// <param name="name">Название потока</param>
        /// <param name="data">PCM аудио данные</param>
        public PcmAudioReader(WaveFormat waveFormat, SoundType audioStreamType, string name, byte[] data)
        {
            this.waveFormat = waveFormat;
            AudioStreamType = audioStreamType;
            //this.bytesPerSecond = bytesPerSecond == 0 ? defaultWaveFmt.AverageBytesPerSecond : bytesPerSecond;
            bytesPerSecond = this.waveFormat.AverageBytesPerSecond;
            Name = name;
            waveData = data;
            Duration = data.Length == 0 ? 0 : Convert.ToInt32(data.Length * 1000d / this.waveFormat.AverageBytesPerSecond);
        }

        /// <inheritdoc/>
        public SoundType AudioStreamType { get; }

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public bool RepeatOnce { get; set; }

        /// <inheritdoc/>
        public bool Paused { get; private set; }

        /// <inheritdoc/>
        public int Duration { get; private set; }

        /// <summary>
        /// Позиция в потоке данных
        /// </summary>
        public int Position
        {
            get
            {
                lock (syncObject)
                    return position;
            }
            set
            {
                lock (syncObject)
                {
                    position = Math.Max(0, Math.Min(Length, value));
                }
            }
        }

        /// <summary>
        /// Размер данных в байтах
        /// </summary>
        public int Length => waveData?.Length ?? 0;

        /// <inheritdoc/>
        public bool IsEof => position >= Length;

        /// <inheritdoc/>
        public void ResetPosition()
        {
            lock (syncObject)
                position = 0;
        }

        /// <inheritdoc/>
        public void Pause()
        {
            lock (syncObject)
                Paused = true;
        }

        /// <inheritdoc/>
        public void Resume()
        {
            lock (syncObject)
                Paused = false;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            lock (syncObject)
            {
                Paused = false;
                Position = 0;
            }
        }

        /// <inheritdoc/>
        public AudioChunk Read(int byteCount)
        {
            if (byteCount < 0)
                throw new ArgumentOutOfRangeException(nameof(byteCount), "Value must be >= 0");

            byte[] data;
            double timePosition = 0;

            lock (syncObject)
            {
                data = new byte[byteCount];
                //return TimeSpan.FromSeconds((double)position / bytesPerSecond);
                timePosition = bytesPerSecond > 0 ? (double)position / bytesPerSecond : 0;

                while (byteCount > 0)
                {
                    int count = Math.Min(byteCount, Length - position);

                    if (count > 0)
                    {
                        Array.Copy(waveData, position, data, 0, count);
                        position += count;
                        byteCount -= count;
                        if (IsEof && RepeatOnce)
                        {
                            RepeatOnce = false;
                            position = 0;
                        }
                    }
                    else break;
                }
            }

            return new AudioChunk(data, timePosition, Length / (double)bytesPerSecond, AudioStreamType);
        }

        /// <summary>
        /// Очистить ресурсы, освободить память
        /// </summary>
        public void Dispose()
        {
            waveData = null;
        }
    }
}
