using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;

namespace ConFileExplorer
{
    internal class DataList
    {
        private class Data
        {
            // Directories and files
            private readonly List<DirectoryInfo> directoryDatas;
            public List<DirectoryInfo> DirectoryDatas => directoryDatas;

            private readonly List<FileInfo> fileDatas;
            public List<FileInfo> FileDatas => fileDatas;

            public Data(List<DirectoryInfo> directoryDatas, List<FileInfo> fileDatas)
            {
                this.directoryDatas = directoryDatas;
                this.fileDatas = fileDatas;
            }

            /// <summary>
            /// Calculate the size of the file to the string value.
            /// </summary>
            /// <param name="index">Index of the displayed file.</param>
            /// <returns>Returns the size of the file as a string value.</returns>
            public string GetFileSizeToString(int index)
            {
                long size = fileDatas[index].Length;
                string sizeString = "0 Bytes";

                if (size >= 1073741824)
                {
                    sizeString = $"{size / 1073741824:##.##} GB";
                }
                else if (size >= 1048576)
                {
                    sizeString = $"{size / 1048576:##.##} MB";
                }
                else if (size >= 1024)
                {
                    sizeString = $"{size / 1024:##.##} KB";
                }
                else if (size < 1024 && size > 0)
                {
                    sizeString = $"{size} Bytes";
                }

                return sizeString;
            }
        }

        private Data data = new Data(new List<DirectoryInfo>(), new List<FileInfo>());

        // Flags
        private bool hasParent;

        public int Load(DirectoryInfo directory)
        {
            if (directory == null)
            {
                return -1;
            }

            data.DirectoryDatas.Clear();
            data.FileDatas.Clear();

            hasParent = false;

            // Load directories
            if (directory.Parent != null)
            {
                hasParent = true;

                DirectoryInfo parent = new DirectoryInfo(directory.Parent.FullName);

                data.DirectoryDatas.Add(parent);
            }

            try
            {
                DirectoryInfo[] directories = directory.GetDirectories();

                for (int i = 0; i < directories.Length; i++)
                {
                    data.DirectoryDatas.Add(directories[i]);
                }

                // Load files
                FileInfo[] files = directory.GetFiles();

                for (int i = 0; i < files.Length; i++)
                {
                    data.FileDatas.Add(files[i]);
                }
            }
            catch (Exception exception)
            {
                ExceptionHandler.HandleException(exception);
            }

            return data.DirectoryDatas.Count + data.FileDatas.Count;
        }

        public void ShowData(string copiedPath)
        {
            int x = 5;
            int y = 2;

            // Clear top text
            for (int i = 0; i < Screen.Width; i++)
            {
                Screen.PrintAt(" ", i, 1);
                Screen.PrintAt(" ", i, 2);
            }

            // Directories and files count text
            string countText = $"Directories : {data.DirectoryDatas.Count}    Files : {data.FileDatas.Count}    Total : {data.DirectoryDatas.Count + data.FileDatas.Count}";

            Screen.PrintAt(countText, (Screen.Width >> 1) - (countText.Length >> 1), 2);

            // Data list
            for (int i = 0; i < data.DirectoryDatas.Count; i++, y++)
            {
                if (data.DirectoryDatas[i].FullName.Equals(copiedPath))
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                Screen.PrintAt($"d_{data.DirectoryDatas[i].CreationTime}    {(hasParent && i == 0 ? ".." : data.DirectoryDatas[i].Name)}", x, y);

                if (data.DirectoryDatas[i].FullName.Equals(copiedPath))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            for (int i = 0; i < data.FileDatas.Count; i++, y++)
            {
                if (data.FileDatas[i].FullName.Equals(copiedPath))
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                Screen.PrintAt($"f_{data.FileDatas[i].CreationTime}    {data.FileDatas[i].Name}", x, y);
                Screen.PrintAt($"{data.GetFileSizeToString(i)}", Screen.Width - 20, y);

                if (data.FileDatas[i].FullName.Equals(copiedPath))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        public void OpenFile(int index)
        {
            try
            {
                Process.Start("Explorer.exe", data.FileDatas[index - data.DirectoryDatas.Count].FullName);
            }
            catch (Exception exception)
            {
                ExceptionHandler.HandleException(exception);
            }
        }

        #region Check

        public bool Exist(string name)
        {
            // Check directories
            for (int i = 0; i < data.DirectoryDatas.Count; i++)
            {
                if (data.DirectoryDatas[i].Name.Equals(name))
                {
                    return true;
                }
            }

            // Check files
            for (int i = 0; i < data.FileDatas.Count; i++)
            {
                if (data.FileDatas[i].Name.Equals(name))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check the index of the file.
        /// </summary>
        /// <param name="index">The index of the displayed data.</param>
        /// <returns>Returns true if the index is greater than or equal to 0, otherwise returns false.</returns>
        public bool IsFile(int index) => index - data.DirectoryDatas.Count >= 0;

        #endregion
        #region Get

        public DirectoryInfo GetDirectory(int index) => data.DirectoryDatas[index];

        public FileInfo GetFile(int index) => data.FileDatas[index - data.DirectoryDatas.Count];

        #endregion
        #region Create

        public void CreateDirectory(string root, string name)
        {
            try
            {
                DirectoryInfo directory = Directory.CreateDirectory(Path.Combine(root, name));

                data.DirectoryDatas.Add(directory);
            }
            catch (Exception exception)
            {
                ExceptionHandler.HandleException(exception);
            }
        }

        public void CreateFile(string root, string name)
        {
            try
            {
                string path = Path.Combine(root, name);

                File.Create(path).Close();

                FileInfo file = new FileInfo(path);

                data.FileDatas.Add(file);
            }
            catch (Exception exception)
            {
                ExceptionHandler.HandleException(exception);
            }
        }

        #endregion
        #region Remove

        public void RemoveDirectoryAt(int index)
        {
            try
            {
                FileSystem.DeleteDirectory(data.DirectoryDatas[index].FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                data.DirectoryDatas.RemoveAt(index);
            }
            catch (Exception exception)
            {
                ExceptionHandler.HandleException(exception);
            }
        }

        public void RemoveFileAt(int index)
        {
            try
            {
                FileSystem.DeleteFile(data.FileDatas[index - data.DirectoryDatas.Count].FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                data.FileDatas.RemoveAt(index - data.DirectoryDatas.Count);
            }
            catch (Exception exception)
            {
                ExceptionHandler.HandleException(exception);
            }
        }

        #endregion
    }
}
