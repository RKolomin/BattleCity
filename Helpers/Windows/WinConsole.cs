using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BattleCity.Helpers.Windows
{
    static class WinConsole
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr GetStdHandle(int nStdHandle);
        private const int STD_OUTPUT_HANDLE = -11;
        //private const int MY_CODE_PAGE = 1251;// 437;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("kernel32.dll")] static extern IntPtr GetConsoleWindow();

        const int SW_HIDE = 0;
        //const int SW_SHOW = 5;
        static IntPtr handle = GetConsoleWindow();

        public static bool IsEnabled() => GetConsoleWindow() != IntPtr.Zero;

        public static void Show()
        {
            AllocConsole();
            handle = GetConsoleWindow();
            IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            Microsoft.Win32.SafeHandles.SafeFileHandle safeFileHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(stdHandle, true);
            FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            //System.Text.Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
            Encoding encoding = Console.OutputEncoding;
            StreamWriter standardOutput = new StreamWriter(fileStream, encoding)
            {
                AutoFlush = true
            };
            Console.SetOut(standardOutput);
        }

        public static void Hide()
        {
            if (handle != IntPtr.Zero)
                ShowWindow(handle, SW_HIDE);
        }
    }
}
