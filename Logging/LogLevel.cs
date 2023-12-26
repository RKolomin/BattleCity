namespace BattleCity.Logging
{
    /// <summary>
    /// Флаги уровней логирования
    /// </summary>
    public enum LogLevel
    {
        None = 0,

        /// <summary>
        /// По умолчанию
        /// </summary>
        Default = 1,

        /// <summary>
        /// Выводить инфо
        /// </summary>
        Info = 1,

        /// <summary>
        /// Выводить предупреждения
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Выводить ошибки
        /// </summary>
        Error = 4,

        /// <summary>
        /// Выводить всё
        /// </summary>
        All = Default | Info | Warning | Error
    }
}
