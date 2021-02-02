using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core.MvvmSample.Helpers;
using log4net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MT.DirectoryWatcher.Blockchain;
using MT.DirectoryWatcher.Blockchain.DirectoryWatcherContract.ContractDefinition;
using Serilog;

namespace MT.DirectoryWatcher.UI.ViewModels
{
    [POCOViewModel]
    public class MainWindowVM 
    {
        public IDirectoryBlockchainService BlockchainService { get; set; }
        public ObservableCollection<FetchTamperedDirectoriesOutputDTO> TamperedDirectories { get; set; }
        public ObservableCollection<FetchTamperedFilesOutputDTO> TamperedFiles { get; set; }
        public DelegateCommand RefreshCommand { get; private set; }

        public static MainWindowVM Create()
        {
            return ViewModelSource.Create(() => new MainWindowVM());
        }

        protected MainWindowVM()
        {
            var logger = new NullLogger<DirectoryBlockChainService>();
            BlockchainService = new DirectoryBlockChainService(logger);
            TamperedDirectories = new ObservableCollection<FetchTamperedDirectoriesOutputDTO>();
            TamperedFiles = new ObservableCollection<FetchTamperedFilesOutputDTO>();
            RefreshCommand = new DelegateCommand(RefreshCommandExecute);
            RefreshCommandExecute();
        }

        private async Task GetTamperedDirectoryList()
        {
           await App.Current.Dispatcher.Invoke(async () =>
            {
                var tamperedListCounts = await BlockchainService.FetchTamperedListCounts();
                if (tamperedListCounts != null)
                {
                    TamperedDirectories.Clear();
                    for (int i = 0; i < tamperedListCounts.TamperedDirectoriesCount; i++)
                    {
                        var tamperedDirectories = await BlockchainService.FetchTamperedDirectories(i);
                        TamperedDirectories.Add(tamperedDirectories);
                    }
                }
            });
        }

        private async void GetTamperedFilesList()
        {
            await App.Current.Dispatcher.Invoke(async () =>
            {
                var tamperedListCounts = await BlockchainService.FetchTamperedListCounts();
                if (tamperedListCounts != null)
                {
                    TamperedFiles.Clear();
                    for (int i = 0; i < tamperedListCounts.TamperedFilesCount; i++)
                    {
                        var tamperedFiles = await BlockchainService.FetchTamperedFiles(i);
                        TamperedFiles.Add(tamperedFiles);
                    }
                }
            });
        }

        private void RefreshCommandExecute()
        {
            Task.Run(() =>
            {
                GetTamperedDirectoryList();
                GetTamperedFilesList();
            });
        }
    }
}

