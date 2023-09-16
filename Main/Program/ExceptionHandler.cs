namespace ConFileExplorer
{
    internal class ExceptionHandler
    {
        /// <summary>
        /// Handle the exception if it's not null.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        public static void HandleException(Exception exception)
        {
            int x = (Screen.Width >> 1) - (exception.Message.Length >> 1);

            // Clear previous exception message
            for (int i = 0; i < Screen.Width; i++)
            {
                Screen.PrintAt(" ", i, 1);
            }

            // Show at the top
            Console.ForegroundColor = ConsoleColor.Red;

            Screen.PrintAt(exception.Message, x, 1);

            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
