using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MT.DirectoryWatcher.Common
{
    public interface IDirectoryWatcher : IWatcher
    {
        List<IDirectoryMonitor> DirectoryMonitors { get; set; }

        Task InitFileHashesToDbAsync();
        void CheckAndStopInvalidMonitors();
        Task CheckFilesAndComputeHash();
    }
}
