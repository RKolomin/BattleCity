namespace BattleCity.Enums
{
    /// <summary>
    /// Перечисление экранов
    /// </summary>
    public enum GameScreenEnum
    {
        None,
        /// <summary>
        /// Титульный экран
        /// </summary>
        Title,

        /// <summary>
        /// Редактор уровней
        /// </summary>
        LevelEditor,

        /// <summary>
        /// Запуск игры в соло
        /// </summary>
        StartSinglePlayer,

        /// <summary>
        /// Запуск игры в режиме нескольких игроков
        /// </summary>
        StartMultiplayer,

        /// <summary>
        /// Выход из игры
        /// </summary>
        ExitGame,

        /// <summary>
        /// Экран перехода
        /// </summary>
        ScreenTransition,

        /// <summary>
        /// Экран начала игры
        /// </summary>
        StartLevel,

        /// <summary>
        /// Экран достижений
        /// </summary>
        HiScores,

        /// <summary>
        /// Экрна в режиме игры (битвы)
        /// </summary>
        PlayGame,

        /// <summary>
        /// Экрна подведения итогов
        /// </summary>
        StageResult,

        /// <summary>
        /// Экран Game over
        /// </summary>
        GameOver,

        /// <summary>
        /// Настройки
        /// </summary>
        Settings
    }
}