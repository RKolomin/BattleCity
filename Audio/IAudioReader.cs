using BattleCity.Enums;
using System;

namespace BattleCity.Audio
{
    /// <summary>
    /// Интерфейс считывателя аудио данных
    /// </summary>
    public interface IAudioReader : IDisposable
    {
        /// <summary>
        /// Название
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Признак конца потока
        /// </summary>
        bool IsEof { get; }

        /// <summary>
        /// Продолжительность в миллисекундах
        /// </summary>
        int Duration { get; }

        /// <summary>
        /// Признак единоразового повтора при достижении конца потока
        /// </summary>
        bool RepeatOnce { get; set; }

        /// <summary>
        /// Признак приостановки воспроизведения
        /// </summary>
        bool Paused { get; }

        /// <summary>
        /// Тип аудио потока
        /// </summary>
        SoundType AudioStreamType { get; }

        /// <summary>
        /// Сброс позиции в аудио потоке
        /// </summary>
        void ResetPosition();

        /// <summary>
        /// Приостановить воспроизведение
        /// </summary>
        void Pause();

        /// <summary>
        /// Продолжить воспроизведение
        /// </summary>
        void Resume();

        /// <summary>
        /// Сброс
        /// </summary>
        void Reset();

        /// <summary>
        /// Считать фрагмент аудио данных
        /// </summary>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        AudioChunk Read(int byteCount);
    }
}
