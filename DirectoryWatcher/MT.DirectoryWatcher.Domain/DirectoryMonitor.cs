using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using MT.DirectoryWatcher.Common;
using MT.DirectoryWatcher.Common.Interface.Hash;

namespace MT.DirectoryWatcher.Domain
{
    public class DirectoryMonitor : IDirectoryMonitor
    {
        private readonly IHashLoader _hashLoader;
        private readonly ILogger<DirectoryMonitor> _logger;

        public string DirectoryPath { get; set; }
        private FileSystemWatcher m_fileWatcher;
        private bool isFileWatcherDisposed;
        private bool disposedValue;

        public DirectoryMonitor(IHashLoader hashLoader, ILogger<DirectoryMonitor> logger)
        {
            this._hashLoader = hashLoader ?? throw new ArgumentNullException(nameof(hashLoader));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void InitMonitor(string directoryPath)
        {
            this.DirectoryPath = directoryPath;
        }

        public void StartMonitor()
        {
            this.StartFileWatcher();
        }

        public void StopMonitor()
        {
            this.StopFileWatcher();
        }

        public void CheckAndStopInvalidMonitors()
        {
            if (!Directory.Exists(this.DirectoryPath))
            {
                this.StopMonitor();
                DirectoryInfo dir = new DirectoryInfo(this.DirectoryPath);
                _hashLoader.DeleteDirectory(dir, DirectoryPath);
            }
        }

        private void StopFileWatcher()
        {
            if(isFileWatcherDisposed) return;

            if (this.m_fileWatcher != null)
            {
                this.m_fileWatcher.Changed -= OnChanged;
                this.m_fileWatcher.Created -= OnChanged;
                this.m_fileWatcher.Deleted -= OnChanged;
                this.m_fileWatcher.Renamed -= OnRenamed;
                this.m_fileWatcher.EnableRaisingEvents = false;
                this.m_fileWatcher.Dispose();
                this.isFileWatcherDisposed = true;
            }
        }

        private void StartFileWatcher()
        {
            try
            {
                if(string.IsNullOrEmpty(DirectoryPath))
                    return;

                if (!Directory.Exists(this.DirectoryPath))
                {
                    _logger.LogError($"Directory \"{DirectoryPath}\" does not exists. Failed to start FileWatcher");
                    return;
                }

                // Create a new FileSystemWatcher and set its properties.
                this.m_fileWatcher = new FileSystemWatcher
                {
                    Path = this.DirectoryPath,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.CreationTime | 
                                   NotifyFilters.LastWrite |
                                   NotifyFilters.DirectoryName |
                                   NotifyFilters.FileName,
                    Filter = "*.*"
                };
                this.m_fileWatcher.Changed += OnChanged;
                this.m_fileWatcher.Created += OnChanged;
                this.m_fileWatcher.Deleted += OnChanged;
                this.m_fileWatcher.Renamed += OnRenamed;
                //Start monitoring.
                this.m_fileWatcher.EnableRaisingEvents = true;
                this.isFileWatcherDisposed = false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        public void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed.
            Console.WriteLine("{0}, with path {1} has been {2}", e.Name, e.FullPath, e.ChangeType);
            FileInfo f = new FileInfo(e.FullPath);
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Changed:
                    if ((f.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
                        _hashLoader.GenerateNewHash(f, DirectoryPath);
                    break;
                case WatcherChangeTypes.Deleted:
                    if (!string.IsNullOrEmpty(f.Extension))
                        _hashLoader.DeleteFile(f, DirectoryPath);
                    else
                    {
                        DirectoryInfo dir = new DirectoryInfo(e.FullPath);
                        _hashLoader.DeleteDirectory(dir, DirectoryPath);
                    }
                    break;
            }
        }
        public void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine(" {0} renamed to {1}", e.OldFullPath, e.FullPath);
            string oldFileName = e.OldName;
            FileInfo newFileInfo = new FileInfo(e.FullPath);
            if ((newFileInfo.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
            {
                var fileSplit = oldFileName.Split("\\");
                string oldFile = string.Empty;
                if (fileSplit.Length > 1)
                    oldFile = fileSplit[1];
                else
                    oldFile = fileSplit[0];

                _hashLoader.RenameFile(oldFile, newFileInfo, this.DirectoryPath);
            }
            else
            {
                string oldDirPath = e.OldFullPath;
                DirectoryInfo newDir = new DirectoryInfo(e.FullPath);
                _hashLoader.RenameDirectory(oldDirPath, newDir, this.DirectoryPath);
            }
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.StopFileWatcher();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DirectoryMonitor()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
