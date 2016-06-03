namespace DbAsk
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ConsoleFont
    {
        internal uint Index;
        internal short SizeX, SizeY;
    }

    internal class ConsoleWindow
    {
        private const int StdInputHandle = -10;
        private const int QuickEditMode = 0x40 | 0x80;

        internal static void EnableQuickEditMode()
        {
            int mode;
            IntPtr handle = GetStdHandle(StdInputHandle);
            GetConsoleMode(handle, out mode);
            mode |= QuickEditMode;
            SetConsoleMode(handle, mode);
        }

        internal static bool SetConsoleIcon(Icon icon)
        {
            return SetConsoleIcon(icon.Handle);
        }

        internal static bool SetConsoleFont(uint index)
        {
            return SetConsoleFont(GetStdHandle(StdHandle.OutputHandle), index);
        }

        internal static uint ConsoleFontsCount => GetNumberOfConsoleFonts();

        internal static ConsoleFont[] ConsoleFonts
        {
            get
            {
                var fonts = new ConsoleFont[GetNumberOfConsoleFonts()];

                if (fonts.Length > 0)
                {
                    GetConsoleFontInfo(
                        GetStdHandle(StdHandle.OutputHandle),
                        false,
                        (uint)fonts.Length,
                        fonts);
                }

                return fonts;
            }
        }

        internal static void AlmostMaximise()
        {
            int maxWidth = Console.LargestWindowWidth;
            int maxHeight = Console.LargestWindowHeight;

            if (maxWidth < 100 || maxHeight < 50)
            {
                return;
            }

            int minWidth = maxWidth - 20;
            int minHeight = maxHeight - 20;

            if (Console.WindowWidth < minWidth || Console.WindowHeight < minHeight)
            {
                Console.SetWindowSize(
                    Math.Max(minWidth, Console.WindowWidth),
                    Math.Max(minHeight, Console.WindowHeight));
            }

            if (Console.BufferHeight < 3000)
            {
                Console.SetBufferSize(Console.WindowWidth, 3000);
            }
        }

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int handle);

        [DllImport("kernel32")]
        private static extern bool SetConsoleIcon(IntPtr hIcon);

        [DllImport("kernel32")]
        private static extern bool SetConsoleFont(IntPtr hOutput, uint index);

        private enum StdHandle
        {
            OutputHandle = -11
        }

        [DllImport("kernel32")]
        private static extern IntPtr GetStdHandle(StdHandle index);

        [DllImport("kernel32")]
        private static extern bool GetConsoleFontInfo(
            IntPtr hOutput, 
            [MarshalAs(UnmanagedType.Bool)]bool bMaximize,
            uint count, 
            [MarshalAs(UnmanagedType.LPArray), Out] ConsoleFont[] fonts);

        [DllImport("kernel32")]
        private static extern uint GetNumberOfConsoleFonts();
    }
}