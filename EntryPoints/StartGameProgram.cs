using BattleCity.Helpers.Windows;
using BattleCity.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace BattleCity.EntryPoints
{
    static class StartGameProgram
    {
        // Идентификатор сборки
        static string AssemblyGuid
        {
            get
            {
                object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(GuidAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((GuidAttribute)attributes[0]).Value;
            }
        }

        [STAThread]
        static void Main()
        {
            // получаем массив аргументов, заданных в переменной окружения
            string[] argLines = Environment.GetCommandLineArgs();

            // формируем список для обработки аргументов
            List<string> cmdLine = new List<string>(0);
            if (argLines != null && argLines.Length > 0) 
                cmdLine = new List<string>(argLines);

            // выполняем обработку списка аргументов
            ProcessCmdArgs(cmdLine);

            // ограничиваем количество запускаемых экземпляров приложения до одного
            Mutex mutex = new Mutex(true, AssemblyGuid, out bool mutexCreated);

            // если стали владельцем первого экземпляра приложения
            if (mutexCreated)
            {
                // оставляем объект Mutex в живых до завершения работы приложения
                GC.KeepAlive(mutex);

                // проверяем условие отображения окна консоли
                if (Debugger.IsAttached || (!WinConsole.IsEnabled() && argLines != null && argLines.Any(x => x == "-console")))
                    WinConsole.Show();

                // определяем признак сброса контента по умолчанию
                bool resetContent = argLines != null && argLines.Any(x => x == "-reset");

                // определяем необходимость подключения сервиса логирования для вывода в окно консоли
                ILogger logger = null;
                if (Debugger.IsAttached || WinConsole.IsEnabled())
                    logger = new ConsoleLogger();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                try
                {
                    Application.Run(new GameForm(logger, resetContent));
                }
                catch (Exception ex)
                {
                    logger?.WriteLine("Application error: " + ex, LogLevel.Error);
                }

                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();
            }
        }

        static void ProcessCmdArgs(List<string> cmdLine)
        {
            string remline;
            int count = cmdLine.Count;
            for (int i = 0; i < count; i++)
            {
                remline = cmdLine[i].ToLower();
                if (string.IsNullOrEmpty(remline))
                {
                    cmdLine.Remove(cmdLine[i]);
                    count = cmdLine.Count;
                    continue;
                }
            }
            if (cmdLine == null || cmdLine.Count == 0 || string.IsNullOrEmpty(cmdLine[0])) return;

            string vshostFilePath = $"{cmdLine[0]}.vshost.exe";
            string exeFilePathLowerCase = Application.ExecutablePath.ToLower();

            count = cmdLine.Count;
            for (int i = 0; i < count; i++)
            {
                remline = cmdLine[i].ToLower();
                if (remline == vshostFilePath || remline == exeFilePathLowerCase)
                {
                    cmdLine.Remove(cmdLine[i]);
                    break;
                }
            }
        }
    }
}
