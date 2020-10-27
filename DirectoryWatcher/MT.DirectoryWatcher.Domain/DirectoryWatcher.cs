using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MT.DirectoryWatcher.Common;
using MT.DirectoryWatcher.Common.Interface.Hash;

namespace MT.DirectoryWatcher.Domain
{
    public class DirectoryWatcher : IDirectoryWatcher
    {
        private readonly ILogger<DirectoryWatcher> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHashLoader _hashLoader;


        public bool IsMonitoring { get; set; }
        public List<IDirectoryMonitor> DirectoryMonitors { get; set; }
        public DirectoryWatcher(IServiceProvider serviceProvider, IHashLoader hashLoader, ILogger<DirectoryWatcher> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._hashLoader = hashLoader ?? throw new ArgumentNullException(nameof(hashLoader));
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            this.DirectoryMonitors = new List<IDirectoryMonitor>();
            InitMonitors();
        }

        private void InitMonitors()
        {
            try
            {
                var directoryList = DirectoryConfigurator.GetDirectoryList();
                if (directoryList == null) return;

                var appList = directoryList.DirectoryPath?.Apps?.ToList();
                if (appList == null)
                {
                    _logger.LogWarning("No directory paths configured to monitor.");
                    return;
                }

                foreach (var app in appList)
                {
                    IDirectoryMonitor monitor = ServiceProviderServiceExtensions.GetService<IDirectoryMonitor>(_serviceProvider);
                    monitor.InitMonitor(app.Location);
                    DirectoryMonitors.Add(monitor);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
            
        }

        public void StartWatcher()
        {
            try
            {
                this.DirectoryMonitors.ForEach(x => x.StartMonitor());
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        public void StopWatcher()
        {
            try
            {
                this.DirectoryMonitors.ForEach(x => x.StopMonitor());
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }


        public async Task InitFileHashesToDbAsync()
        {
            await _hashLoader.GetFileHashInfoFromDB();
            await _hashLoader.LoadInitialFileHashesToDB();
            this.StartWatcher();
            this.IsMonitoring = true;
        }

        public async Task CheckFilesAndComputeHash()
        {
            await _hashLoader.LoadInitialFileHashesToDB();
        }

        public void CheckAndStopInvalidMonitors()
        {
            var directoryList = DirectoryConfigurator.GetDirectoryList();
            if (directoryList == null) return;

            var appList = directoryList.DirectoryPath?.Apps?.ToList();
            if (appList == null)
            {
                _logger.LogWarning("No directory paths configured to monitor.");
                return;
            }

            foreach (var app in appList)
            {
                var monitor = this.DirectoryMonitors.FirstOrDefault(x => x.DirectoryPath.Equals(app.Location));
                monitor?.CheckAndStopInvalidMonitors();
            }
        }
    }
}
