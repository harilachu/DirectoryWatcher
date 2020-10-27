using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MT.DirectoryWatcher.Blockchain.DirectoryWatcherContract.ContractDefinition;
using MT.DirectoryWatcher.Common;

namespace MT.DirectoryWatcher.Blockchain
{
    public interface IDirectoryBlockchainService : IBlockchainService
    {
        Task<FetchListCountsOutputDTO> FetchTamperedListCounts();
        Task<FetchTamperedDirectoriesOutputDTO> FetchTamperedDirectories(int index);
        Task<FetchTamperedFilesOutputDTO> FetchTamperedFiles(int index);
        Task<ReceiptStatus> InsertTamperedDirectory(DirectoryInfoDto directoryInfoDto);
        Task<ReceiptStatus> InsertTamperedFiles(FileHashInfoDto filleHashInfoDto);
    }
}
