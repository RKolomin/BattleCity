namespace BattleCity.Common
{
    /// <summary>
    /// Основные конфигурации
    /// </summary>
    public class CommonConfig
    {
        /// <summary>
        /// Размер звукового буффера в миллисекундах
        /// </summary>
        public int SoundEngineLatency { get; set; } = 40;

        /// <summary>
        /// Звуковой файл для проверки уровня громкости звука
        /// </summary>
        public string CheckSoundLevelFileName { get; set; } = "hit_wall.wav";

        /// <summary>
        /// Звуковой файл для проверки уровня громкости музыки
        /// </summary>
        public string CheckMusicLevelFileName { get; set; } = "hit_wall.wav";

        /// <summary>
        /// Ширина оконного режима
        /// </summary>
        public int WindowWidth { get; set; } = 800;

        /// <summary>
        /// Высота оконного режима
        /// </summary>
        public int WindowHeight { get; set; } = 600;

        /// <summary>
        /// Максимальное количество одновременно воспроизводимых звуков/музыки
        /// </summary>
        public int MaxAudioSlots { get; set; } = 16;

        /// <summary>
        /// Размер шрифта по умолчанию
        /// </summary>
        public float DefaultFontSize { get; set; } = 15f;

        /// <summary>
        /// Размер шрифта в редакторе уровней
        /// </summary>
        public float LevelEditorFontSize { get; set; } = 12f;

        /// <summary>
        /// Файл шрифта по умолчанию
        /// </summary>
        public string DefaultFontFileName { get; set; } = "prstart.ttf";

        /// <summary>
        /// Файл шрифта для лого
        /// </summary>
        public string LogoFontFileName { get; set; } = "prstart.ttf";

        /// <summary>
        /// Цвет лого
        /// </summary>
        public string LogoFontColor { get; set; } = "#FF893629";
    }
}
