using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MT.DirectoryWatcher.Common.Interface.Hash
{
    public interface IHashLoader
    {
        Task GetFileHashInfoFromDB();
        Task LoadInitialFileHashesToDB();
        void GenerateNewHash(FileInfo f, string directoryPath);
        void RenameFile(string oldFileName, FileInfo newFileInfo, string directoryPath);
        void DeleteFile(FileInfo f, string directoryPath);
        void DeleteDirectory(DirectoryInfo directoryInfo, string directoryPath);
        void RenameDirectory(string oldDirPath, DirectoryInfo newDir, string directoryPath);
    }
}
