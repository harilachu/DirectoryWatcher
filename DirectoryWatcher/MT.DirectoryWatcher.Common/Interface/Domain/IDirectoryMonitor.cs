using System;
using System.Collections.Generic;
using System.Text;

namespace MT.DirectoryWatcher.Common
{
    public interface IDirectoryMonitor : IDisposable
    {
        string DirectoryPath { get; set; }

        void InitMonitor(string directoryPath);
        void StartMonitor();
        void StopMonitor();
        void CheckAndStopInvalidMonitors();
    }
}
