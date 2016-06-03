namespace DbAsk
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ConsoleFont
    {
        public uint Index;
        public short SizeX, SizeY;
    }

    class ConsoleWindow
    {
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(int handle);

        const int STD_INPUT_HANDLE = -10;
        const int ENABLE_QUICK_EDIT_MODE = 0x40 | 0x80;

        internal static void EnableQuickEditMode()
        {
            int mode;
            IntPtr handle = GetStdHandle(STD_INPUT_HANDLE);
            GetConsoleMode(handle, out mode);
            mode |= ENABLE_QUICK_EDIT_MODE;
            SetConsoleMode(handle, mode);
        }

        [DllImport("kernel32")]
        public static extern bool SetConsoleIcon(IntPtr hIcon);

        public static bool SetConsoleIcon(Icon icon)
        {
            return SetConsoleIcon(icon.Handle);
        }

        [DllImport("kernel32")]
        private extern static bool SetConsoleFont(IntPtr hOutput, uint index);

        private enum StdHandle
        {
            OutputHandle = -11
        }

        [DllImport("kernel32")]
        private static extern IntPtr GetStdHandle(StdHandle index);

        public static bool SetConsoleFont(uint index)
        {
            return SetConsoleFont(GetStdHandle(StdHandle.OutputHandle), index);
        }

        [DllImport("kernel32")]
        private static extern bool GetConsoleFontInfo(
            IntPtr hOutput, 
            [MarshalAs(UnmanagedType.Bool)]bool bMaximize,
            uint count, 
            [MarshalAs(UnmanagedType.LPArray), Out] ConsoleFont[] fonts);

        [DllImport("kernel32")]
        private static extern uint GetNumberOfConsoleFonts();

        public static uint ConsoleFontsCount
        {
            get
            {
                return GetNumberOfConsoleFonts();
            }
        }

        public static ConsoleFont[] ConsoleFonts
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
    }
}