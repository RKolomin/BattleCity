using System;

namespace BattleCity.Logging
{
    /// <summary>
    /// Лог в консоль
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        public static ConsoleLogger Instance { get; } = new ConsoleLogger();

        public LogLevel Filter { get; set; } = LogLevel.Info | LogLevel.Error | LogLevel.Warning;

        private void GetLogColor(LogLevel logLevel, out ConsoleColor foreColor, out ConsoleColor backColor)
        {
            switch (logLevel)
            {
                case LogLevel.Error:
                    foreColor = ConsoleColor.Red;
                    backColor = ConsoleColor.Black;
                    break;
                case LogLevel.Warning:
                    foreColor = ConsoleColor.Yellow;
                    backColor = ConsoleColor.Black;
                    break;
                case LogLevel.Info:
                    foreColor = ConsoleColor.Cyan;
                    backColor = ConsoleColor.Black;
                    break;
                default:
                    foreColor = ConsoleColor.Gray;
                    backColor = ConsoleColor.Black;
                    break;
            }
        }

        public void WriteLine(string text, LogLevel logLevel = LogLevel.Default)
        {
            if (logLevel == 0) return;

            var oldForeColor = Console.ForegroundColor;
            var oldBackColor = Console.BackgroundColor;

            GetLogColor(logLevel, out ConsoleColor foreColor, out ConsoleColor backColor);
            Console.ForegroundColor = foreColor;
            Console.BackgroundColor = backColor;

            string date = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff");
            Console.WriteLine($"[{date}] {text}");

            Console.ForegroundColor = oldForeColor;
            Console.BackgroundColor = oldBackColor;
        }

        public void WriteDateTime()
        {
            var oldForeColor = Console.ForegroundColor;
            var oldBackColor = Console.BackgroundColor;

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            string date = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff");
            Console.Write($"[{date}] ");

            Console.ForegroundColor = oldForeColor;
            Console.BackgroundColor = oldBackColor;
        }

        public void Write(string text, LogLevel logLevel = LogLevel.Default)
        {
            if (logLevel == 0) return;

            var oldForeColor = Console.ForegroundColor;
            var oldBackColor = Console.BackgroundColor;

            GetLogColor(logLevel, out ConsoleColor foreColor, out ConsoleColor backColor);
            Console.ForegroundColor = foreColor;
            Console.BackgroundColor = backColor;

            Console.Write($"{text}");

            Console.ForegroundColor = oldForeColor;
            Console.BackgroundColor = oldBackColor;
        }
    }
}
