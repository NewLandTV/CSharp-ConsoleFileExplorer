namespace ConFileExplorer
{
    internal class Screen
    {
        // Window size
        public static int Width => Console.WindowWidth;
        public static int Height => Console.WindowHeight;

        /// <summary>
        /// Set the screen class.
        /// </summary>
        /// <param name="title">The title text.</param>
        /// <param name="width">The width of the screen.</param>
        /// <param name="height">The height of the screen</param>
        /// <param name="bufferHeight">The height of the buffer.</param>
        public static void Setup(string title, int width, int height, int bufferHeight)
        {
            Console.Title = title;
            Console.CursorVisible = false;
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.ForegroundColor = ConsoleColor.White;

            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, bufferHeight);
        }

        /// <summary>
        /// Print a message at a specific position.
        /// </summary>
        /// <param name="message">This is the message that will be printed.</param>
        /// <param name="x">The x position at which it will be printed.</param>
        /// <param name="y">The y position at which it pwill be printed.</param>
        public static void PrintAt(string message, int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(message);
        }

        /// <summary>
        /// Clear the screen.
        /// </summary>
        public static void ClearScreen() => Console.Clear();
    }
}
