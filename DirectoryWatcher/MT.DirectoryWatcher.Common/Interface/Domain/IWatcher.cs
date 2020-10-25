using System;
using System.Collections.Generic;
using System.Text;

namespace MT.DirectoryWatcher.Common
{
    public interface IWatcher
    {
        /// <summary>
        /// Gets if started monitoring or not.
        /// </summary>
        bool IsMonitoring { get; }
        /// <summary>
        /// Starts watcher.
        /// </summary>
        void StartWatcher();
        /// <summary>
        /// Stops watcher.
        /// </summary>
        void StopWatcher();
    }
}
