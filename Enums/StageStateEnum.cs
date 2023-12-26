namespace BattleCity.Enums
{
    /// <summary>
    /// Перечисление статуса уровня (stage)
    /// </summary>
    public enum StageStateEnum
    {
        /// <summary>
        /// Не задано
        /// </summary>
        None,

        /// <summary>
        /// Игра
        /// </summary>
        Play = 1,

        /// <summary>
        /// Игра проиграна
        /// </summary>
        GameOver = 2,

        /// <summary>
        /// Уровень завершён
        /// </summary>
        Complete = 3,

        /// <summary>
        /// Выход на главный экран
        /// </summary>
        ExitToMainScreen = 4,
    }
}
