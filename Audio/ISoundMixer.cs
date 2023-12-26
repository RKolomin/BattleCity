using BattleCity.Enums;

namespace BattleCity.Audio
{
    /// <summary>
    /// Интерфейс звукового микшера
    /// </summary>
    public interface ISoundMixer : IPcmOutputStream
    {
        /// <summary>
        /// Установить уровень громкости
        /// </summary>
        /// <param name="type"></param>
        /// <param name="level"></param>
        void SetSoundLevel(SoundType type, float level);

        /// <summary>
        /// Получить уровень громкости
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        float GetSoundLevel(SoundType type);

        /// <summary>
        /// Добавить аудио поток
        /// </summary>
        /// <param name="stream">Аудио поток</param>
        /// <param name="reuseExistSlot">Повтоное использование активного слота с тем же именем</param>
        /// <param name="restartExistSlot">При повторном использовании слота выполнить сброс позиции потока данных</param>
        void Add(IAudioReader stream, bool reuseExistSlot = false, bool restartExistSlot = false);

        /// <summary>
        /// Удалить аудио поток
        /// </summary>
        /// <param name="stream"></param>
        void Remove(IAudioReader stream);

        /// <summary>
        /// Удалить все активные потоки
        /// </summary>
        /// <param name="streamId"></param>
        void Clear(SoundType? streamId);

        /// <summary>
        /// Определяет, воспроизводится ли аудио поток по указанному названию и типу
        /// </summary>
        /// <param name="sndName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        bool Contains(string sndName, SoundType type);
    }
}
