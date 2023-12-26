namespace BattleCity.Logging
{
    public interface ILogger
    {
        LogLevel Filter { get; set; }
        void WriteLine(string text, LogLevel logLevel = LogLevel.Default);
        void WriteDateTime();
        void Write(string text, LogLevel logLevel = LogLevel.Default);
    }
}
