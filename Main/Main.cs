class Program
{
    static void Main(string[] args)
    {
        ushort majorVersion = 1;
        ushort minorVersion = 0;
        ushort patchVersion = 0;
        string startPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        // Changes the start path for the argument if it's not null.
        if (args.Length >= 1)
        {
            startPath = args[0];
        }

        new ConFileExplorer.Program(startPath, majorVersion, minorVersion, patchVersion);
    }
}