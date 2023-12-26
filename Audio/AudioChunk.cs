using BattleCity.Enums;

namespace BattleCity.Audio
{
    /// <summary>
    /// Фрагмент аудио данных
    /// </summary>
    public class AudioChunk
    {
        public int ByteCount { get; private set; }
        public int ShortCount { get; private set; }
        public int FloatCount { get; private set; }

        /// <summary>
        /// Audio data
        /// </summary>
        public AudioSamplesMulticast Data { get; }

        /// <summary>
        /// Time position (In seconds)
        /// </summary>
        public double TimePosition { get; }

        /// <summary>
        /// Duration (In seconds)
        /// </summary>
        public double Duration { get; }

        /// <summary>
        /// Тип аудио потока
        /// </summary>
        public SoundType AudioStreamType { get; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="data">Фрагмент аудио данных в виде массива байтов</param>
        /// <param name="timePosition">Метка времени</param>
        /// <param name="duration">Продолжительность фрагмента в миллисекундах</param>
        /// <param name="streamId">Тип аудио потока</param>
        public AudioChunk(byte[] data, double timePosition, double duration, SoundType streamId)
        {
            AudioStreamType = streamId;
            TimePosition = timePosition;
            Duration = duration;
            ByteCount = data.Length;
            ShortCount = data.Length / sizeof(short);
            FloatCount = data.Length / sizeof(float);
            Data = new AudioSamplesMulticast() { Bytes = data };
        }
    }
}