using SlimDX.Multimedia;

namespace BattleCity.Audio
{
    /// <summary>
    /// Интерфейс выходного потока PCM аудио данных
    /// </summary>
    public interface IPcmOutputStream
    {
        /// <summary>
        /// Формат аудио данных
        /// </summary>
        WaveFormat WaveFormat { get; }

        /// <summary>
        /// Считать аудио данные
        /// </summary>
        /// <param name="buffer">буффер для считываемых данных</param>
        /// <param name="offset">смещение в позиции буффера</param>
        /// <param name="count">количество данных для считывания</param>
        /// <returns></returns>
        int Read(byte[] buffer, int offset, int count);
    }
}
