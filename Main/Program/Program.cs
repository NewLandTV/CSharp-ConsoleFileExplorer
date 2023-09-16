using Microsoft.VisualBasic.FileIO;

namespace ConFileExplorer
{
    internal class Program
    {
        private DirectoryInfo currentDirectory;
        private FileSystemWatcher watcher;

        private int dataCount;
        private int selectorIndex;

        private DirectoryInfo? copiedDirectory;
        private FileInfo? copiedFile;

        private string CopiedPath => copiedDirectory != null ? copiedDirectory.FullName : copiedFile != null ? copiedFile.FullName : string.Empty;

        private readonly ushort majorVersion;
        private readonly ushort minorVersion;
        private readonly ushort patchVersion;

        private bool cut;

        // Datas
        private DataList dataList;

        private List<DriveInfo> driveList;

        public Program(string startPath, ushort majorVersion, ushort minorVersion, ushort patchVersion)
        {
            Screen.Setup("Con File Explorer", 180, 54, 1000);

            currentDirectory = new DirectoryInfo(startPath);

            // Setup file system watcher
            watcher = new FileSystemWatcher(currentDirectory.FullName, "*.*");

            // Filters
            watcher.NotifyFilter = NotifyFilters.Attributes
                | NotifyFilters.CreationTime
                | NotifyFilters.DirectoryName
                | NotifyFilters.FileName
                | NotifyFilters.LastAccess
                | NotifyFilters.LastWrite
                | NotifyFilters.Security
                | NotifyFilters.Size;

            // Event functions
            watcher.Changed += RefreshDataList;
            watcher.Created += RefreshDataList;
            watcher.Deleted += RefreshDataList;
            watcher.Renamed += RefreshDataList;
            watcher.Error += (sender, e) => ExceptionHandler.HandleException(e.GetException());

            // Properties
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            // Setup datas
            dataList = new DataList();
            driveList = new List<DriveInfo>(DriveInfo.GetDrives());

            this.majorVersion = majorVersion;
            this.minorVersion = minorVersion;
            this.patchVersion = patchVersion;

            // Start
            Show();

            while (true)
            {
                Update();
            }
        }

        private void Update()
        {
            Screen.PrintAt("", 1, 0);

            ConsoleKey key = Console.ReadKey().Key;

            InputProcess(key);
        }

        private void InputProcess(ConsoleKey key)
        {
            switch (key)
            {
                // Move selector
                case ConsoleKey.UpArrow:
                    if (selectorIndex > 0)
                    {
                        Screen.PrintAt("  ", 1, selectorIndex + 2);

                        selectorIndex--;

                        DrawCursor();
                    }

                    break;
                case ConsoleKey.DownArrow:
                    if (selectorIndex < dataCount - 1)
                    {
                        Screen.PrintAt("  ", 1, selectorIndex + 2);

                        selectorIndex++;

                        DrawCursor();
                    }

                    break;

                // Open file or move directory
                case ConsoleKey.Enter:
                    if (selectorIndex == -1)
                    {
                        break;
                    }

                    DrawCursor();

                    if (dataList.IsFile(selectorIndex))
                    {
                        dataList.OpenFile(selectorIndex);

                        break;
                    }

                    ChangeDirectory(dataList.GetDirectory(selectorIndex).FullName);

                    break;

                // Add a file or directory
                case ConsoleKey.D:
                    CreateDirectory();

                    break;
                case ConsoleKey.F:
                    CreateFile();

                    break;

                // Cut, copy and paste
                case ConsoleKey.X:
                    if (selectorIndex == -1 || selectorIndex == 0)
                    {
                        break;
                    }

                    Cut();

                    break;
                case ConsoleKey.C:
                    if (selectorIndex == -1 || selectorIndex == 0)
                    {
                        break;
                    }

                    Copy();

                    break;
                case ConsoleKey.V:
                    Paste();

                    break;

                // Delete a file or directory
                case ConsoleKey.Delete:
                    if (selectorIndex == -1 || selectorIndex == 0)
                    {
                        break;
                    }

                    Delete();

                    break;

                // Change drive
                default:
                    for (int i = 0; i < driveList.Count; i++)
                    {
                        if (key == ConsoleKey.D0 + i && driveList[i].IsReady)
                        {
                            ChangeDirectory(driveList[i].Name);
                        }
                    }

                    break;
            }
        }

        private void CreateDirectory()
        {
            string name = "Directory";

            for (int i = 1; dataList.Exist(name); i++)
            {
                name = $"Directory_{i}";
            }

            dataList.CreateDirectory(currentDirectory.FullName, name);
        }

        private void CreateFile()
        {
            string name = "File.txt";

            for (int i = 1; dataList.Exist(name); i++)
            {
                name = $"File_{i}.txt";
            }

            dataList.CreateFile(currentDirectory.FullName, name);
        }

        private void Cut()
        {
            Copy();

            cut = true;
        }

        private void Copy()
        {
            cut = false;

            if (dataList.IsFile(selectorIndex))
            {
                copiedDirectory = null;
                copiedFile = dataList.GetFile(selectorIndex);
            }
            else
            {
                copiedDirectory = dataList.GetDirectory(selectorIndex);
                copiedFile = null;
            }

            RefreshDataList(null, null);
        }

        private void Paste()
        {
            try
            {
                if (copiedFile != null)
                {
                    if (cut)
                    {
                        File.Move(copiedFile.FullName, Path.Join(currentDirectory.FullName, copiedFile.Name));

                        return;
                    }

                    File.Copy(copiedFile.FullName, Path.Join(currentDirectory.FullName, copiedFile.Name));
                }
                else if (copiedDirectory != null)
                {
                    if (cut)
                    {
                        FileSystem.MoveDirectory(copiedDirectory.FullName, Path.Join(currentDirectory.FullName, copiedDirectory.Name), UIOption.OnlyErrorDialogs);

                        return;
                    }

                    FileSystem.CopyDirectory(copiedDirectory.FullName, Path.Join(currentDirectory.FullName, copiedDirectory.Name), UIOption.OnlyErrorDialogs);
                }
            }
            catch (Exception exception)
            {
                ExceptionHandler.HandleException(exception);
            }
        }

        private void Delete()
        {
            if (dataList.IsFile(selectorIndex))
            {
                dataList.RemoveFileAt(selectorIndex);

                RefreshDataList(null, null);
            }
            else
            {
                dataList.RemoveDirectoryAt(selectorIndex);

                RefreshDataList(null, null);
            }
        }

        private void Show()
        {
            Screen.ClearScreen();

            // Refresh data list
            ShowPath();

            dataCount = dataList.Load(currentDirectory);
            selectorIndex = -1;

            if (dataCount != -1)
            {
                selectorIndex = 0;

                dataList.ShowData(CopiedPath);

                DrawCursor();
            }

            ShowDisk();
            ShowVersion();
        }

        private void ShowPath()
        {
            int x = (Screen.Width >> 1) - (currentDirectory.FullName.Length >> 1);

            Console.ForegroundColor = ConsoleColor.Magenta;

            Screen.PrintAt(currentDirectory.FullName, x, 0);

            Console.ForegroundColor = ConsoleColor.White;
        }

        private void ShowDisk()
        {
            int x = Screen.Width - 8;

            for (int i = 0; i < driveList.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;

                Screen.PrintAt($"[{i}] {driveList[i].Name}", x, i);

                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void ShowVersion()
        {
            Screen.PrintAt($"v{majorVersion}.{minorVersion}.{patchVersion}", 1, 0);
        }

        private void DrawCursor()
        {
            Console.ForegroundColor = ConsoleColor.Gray;

            Screen.PrintAt("->", 1, selectorIndex + 2);

            Console.ForegroundColor = ConsoleColor.White;
        }

        private void ChangeDirectory(string path)
        {
            try
            {
                currentDirectory = new DirectoryInfo(path);

                watcher.Path = currentDirectory.FullName;
            }
            catch (Exception exception)
            {
                ExceptionHandler.HandleException(exception);
            }
            finally
            {
                Show();
            }
        }

        // Event functions
        private void RefreshDataList(object? sender, FileSystemEventArgs? e)
        {
            Screen.ClearScreen();

            // Refresh data list
            ShowPath();

            dataCount = dataList.Load(currentDirectory);

            if (dataCount != -1 && selectorIndex != -1)
            {
                // If the selector index exceeds the number of data, change the selector index to select the last data
                if (selectorIndex >= dataCount)
                {
                    selectorIndex = dataCount - 1;
                }

                dataList.ShowData(CopiedPath);

                DrawCursor();
            }

            ShowDisk();
            ShowVersion();
        }
    }
}
