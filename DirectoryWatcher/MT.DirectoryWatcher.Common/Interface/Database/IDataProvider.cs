using System;
using System.Collections.Generic;
using System.Text;

namespace MT.DirectoryWatcher.Common
{
    public interface IDataProvider
    {
        FileHashInfoDto GetFileInfo(string appName, FileHashInfoDto fileHashInfo);
        IEnumerable<FileHashInfoDto> GetFileInfos(string appName);
        void AddFileInfo(string appName, FileHashInfoDto item);
        void AddFileInfos(string appName, IEnumerable<FileHashInfoDto> items);
        void UpdateFileInfo(string appName, FileHashInfoDto fileHashInfo);
        void RemoveFileInfos(string appName, FileHashInfoDto item);
        void RemoveFileInfos(string appName, IEnumerable<FileHashInfoDto> items);
    }
}
