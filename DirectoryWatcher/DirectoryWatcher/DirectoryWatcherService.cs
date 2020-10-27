using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MT.DirectoryWatcher.Common;

namespace MT.DirectoryWatcher.Service
{
    public class DirectoryWatcherService : BackgroundService
    {
        private readonly ILogger<DirectoryWatcherService> _logger;
        private readonly IDirectoryWatcher _directoryWatcher;
        private readonly ManualResetEventSlim _manualResetEvent;

        public DirectoryWatcherService(ILogger<DirectoryWatcherService> logger, IDirectoryWatcher directoryWatcher)
        {
            _logger = logger;
            _directoryWatcher = directoryWatcher;
            _manualResetEvent = new ManualResetEventSlim();
        }

        #region Worker service
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Task dbTask = Task.Run(()=>this._directoryWatcher.InitFileHashesToDbAsync());
            dbTask.ContinueWith((x) => { _manualResetEvent.Set(); });
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _manualResetEvent.Wait(stoppingToken);
            while (this._directoryWatcher.IsMonitoring && !stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                _directoryWatcher.CheckAndStopInvalidMonitors();
                await _directoryWatcher.CheckFilesAndComputeHash();
                await Task.Delay(5000, stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            this._directoryWatcher.StopWatcher();
            return base.StopAsync(cancellationToken);
        }

        #endregion
    }
}
