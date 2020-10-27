using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Contracts;
using System.Threading;
using MT.DirectoryWatcher.Blockchain.DirectoryWatcherContract.ContractDefinition;

namespace MT.DirectoryWatcher.Blockchain.DirectoryWatcherContract
{
    public partial class DirectoryWatcherContractService
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.Web3 web3, DirectoryWatcherContractDeployment directoryWatcherContractDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            return web3.Eth.GetContractDeploymentHandler<DirectoryWatcherContractDeployment>().SendRequestAndWaitForReceiptAsync(directoryWatcherContractDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, DirectoryWatcherContractDeployment directoryWatcherContractDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<DirectoryWatcherContractDeployment>().SendRequestAsync(directoryWatcherContractDeployment);
        }

        public static async Task<DirectoryWatcherContractService> DeployContractAndGetServiceAsync(Nethereum.Web3.Web3 web3, DirectoryWatcherContractDeployment directoryWatcherContractDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, directoryWatcherContractDeployment, cancellationTokenSource);
            return new DirectoryWatcherContractService(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.Web3 Web3{ get; }

        public ContractHandler ContractHandler { get; }

        public DirectoryWatcherContractService(Nethereum.Web3.Web3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public Task<FetchListCountsOutputDTO> FetchListCountsQueryAsync(FetchListCountsFunction fetchListCountsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<FetchListCountsFunction, FetchListCountsOutputDTO>(fetchListCountsFunction, blockParameter);
        }

        public Task<FetchListCountsOutputDTO> FetchListCountsQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<FetchListCountsFunction, FetchListCountsOutputDTO>(null, blockParameter);
        }

        public Task<FetchTamperedDirectoriesOutputDTO> FetchTamperedDirectoriesQueryAsync(FetchTamperedDirectoriesFunction fetchTamperedDirectoriesFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<FetchTamperedDirectoriesFunction, FetchTamperedDirectoriesOutputDTO>(fetchTamperedDirectoriesFunction, blockParameter);
        }

        public Task<FetchTamperedDirectoriesOutputDTO> FetchTamperedDirectoriesQueryAsync(BigInteger index, BlockParameter blockParameter = null)
        {
            var fetchTamperedDirectoriesFunction = new FetchTamperedDirectoriesFunction();
                fetchTamperedDirectoriesFunction.Index = index;
            
            return ContractHandler.QueryDeserializingToObjectAsync<FetchTamperedDirectoriesFunction, FetchTamperedDirectoriesOutputDTO>(fetchTamperedDirectoriesFunction, blockParameter);
        }

        public Task<FetchTamperedFilesOutputDTO> FetchTamperedFilesQueryAsync(FetchTamperedFilesFunction fetchTamperedFilesFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<FetchTamperedFilesFunction, FetchTamperedFilesOutputDTO>(fetchTamperedFilesFunction, blockParameter);
        }

        public Task<FetchTamperedFilesOutputDTO> FetchTamperedFilesQueryAsync(BigInteger index, BlockParameter blockParameter = null)
        {
            var fetchTamperedFilesFunction = new FetchTamperedFilesFunction();
                fetchTamperedFilesFunction.Index = index;
            
            return ContractHandler.QueryDeserializingToObjectAsync<FetchTamperedFilesFunction, FetchTamperedFilesOutputDTO>(fetchTamperedFilesFunction, blockParameter);
        }

        public Task<string> InsertTamperedDirectoryRequestAsync(InsertTamperedDirectoryFunction insertTamperedDirectoryFunction)
        {
             return ContractHandler.SendRequestAsync(insertTamperedDirectoryFunction);
        }

        public Task<TransactionReceipt> InsertTamperedDirectoryRequestAndWaitForReceiptAsync(InsertTamperedDirectoryFunction insertTamperedDirectoryFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(insertTamperedDirectoryFunction, cancellationToken);
        }

        public Task<string> InsertTamperedDirectoryRequestAsync(string directoryName, string directoryPath, string directoryChange)
        {
            var insertTamperedDirectoryFunction = new InsertTamperedDirectoryFunction();
                insertTamperedDirectoryFunction.DirectoryName = directoryName;
                insertTamperedDirectoryFunction.DirectoryPath = directoryPath;
                insertTamperedDirectoryFunction.DirectoryChange = directoryChange;
            
             return ContractHandler.SendRequestAsync(insertTamperedDirectoryFunction);
        }

        public Task<TransactionReceipt> InsertTamperedDirectoryRequestAndWaitForReceiptAsync(string directoryName, string directoryPath, string directoryChange, CancellationTokenSource cancellationToken = null)
        {
            var insertTamperedDirectoryFunction = new InsertTamperedDirectoryFunction();
                insertTamperedDirectoryFunction.DirectoryName = directoryName;
                insertTamperedDirectoryFunction.DirectoryPath = directoryPath;
                insertTamperedDirectoryFunction.DirectoryChange = directoryChange;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(insertTamperedDirectoryFunction, cancellationToken);
        }

        public Task<string> InsertTamperedFileRequestAsync(InsertTamperedFileFunction insertTamperedFileFunction)
        {
             return ContractHandler.SendRequestAsync(insertTamperedFileFunction);
        }

        public Task<TransactionReceipt> InsertTamperedFileRequestAndWaitForReceiptAsync(InsertTamperedFileFunction insertTamperedFileFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(insertTamperedFileFunction, cancellationToken);
        }

        public Task<string> InsertTamperedFileRequestAsync(string fileName, string parentDirectory, string directoryPath, string oldHash, string newHash, string fileChange)
        {
            var insertTamperedFileFunction = new InsertTamperedFileFunction();
                insertTamperedFileFunction.FileName = fileName;
                insertTamperedFileFunction.ParentDirectory = parentDirectory;
                insertTamperedFileFunction.DirectoryPath = directoryPath;
                insertTamperedFileFunction.OldHash = oldHash;
                insertTamperedFileFunction.NewHash = newHash;
                insertTamperedFileFunction.FileChange = fileChange;
            
             return ContractHandler.SendRequestAsync(insertTamperedFileFunction);
        }

        public Task<TransactionReceipt> InsertTamperedFileRequestAndWaitForReceiptAsync(string fileName, string parentDirectory, string directoryPath, string oldHash, string newHash, string fileChange, CancellationTokenSource cancellationToken = null)
        {
            var insertTamperedFileFunction = new InsertTamperedFileFunction();
                insertTamperedFileFunction.FileName = fileName;
                insertTamperedFileFunction.ParentDirectory = parentDirectory;
                insertTamperedFileFunction.DirectoryPath = directoryPath;
                insertTamperedFileFunction.OldHash = oldHash;
                insertTamperedFileFunction.NewHash = newHash;
                insertTamperedFileFunction.FileChange = fileChange;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(insertTamperedFileFunction, cancellationToken);
        }

        public Task<BigInteger> TamperedDirectoriesCountQueryAsync(TamperedDirectoriesCountFunction tamperedDirectoriesCountFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TamperedDirectoriesCountFunction, BigInteger>(tamperedDirectoriesCountFunction, blockParameter);
        }

        
        public Task<BigInteger> TamperedDirectoriesCountQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TamperedDirectoriesCountFunction, BigInteger>(null, blockParameter);
        }

        public Task<TamperedDirectoryListOutputDTO> TamperedDirectoryListQueryAsync(TamperedDirectoryListFunction tamperedDirectoryListFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<TamperedDirectoryListFunction, TamperedDirectoryListOutputDTO>(tamperedDirectoryListFunction, blockParameter);
        }

        public Task<TamperedDirectoryListOutputDTO> TamperedDirectoryListQueryAsync(BigInteger returnValue1, BlockParameter blockParameter = null)
        {
            var tamperedDirectoryListFunction = new TamperedDirectoryListFunction();
                tamperedDirectoryListFunction.ReturnValue1 = returnValue1;
            
            return ContractHandler.QueryDeserializingToObjectAsync<TamperedDirectoryListFunction, TamperedDirectoryListOutputDTO>(tamperedDirectoryListFunction, blockParameter);
        }

        public Task<TamperedFileListOutputDTO> TamperedFileListQueryAsync(TamperedFileListFunction tamperedFileListFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<TamperedFileListFunction, TamperedFileListOutputDTO>(tamperedFileListFunction, blockParameter);
        }

        public Task<TamperedFileListOutputDTO> TamperedFileListQueryAsync(BigInteger returnValue1, BlockParameter blockParameter = null)
        {
            var tamperedFileListFunction = new TamperedFileListFunction();
                tamperedFileListFunction.ReturnValue1 = returnValue1;
            
            return ContractHandler.QueryDeserializingToObjectAsync<TamperedFileListFunction, TamperedFileListOutputDTO>(tamperedFileListFunction, blockParameter);
        }

        public Task<BigInteger> TamperedFilesCountQueryAsync(TamperedFilesCountFunction tamperedFilesCountFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TamperedFilesCountFunction, BigInteger>(tamperedFilesCountFunction, blockParameter);
        }

        
        public Task<BigInteger> TamperedFilesCountQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TamperedFilesCountFunction, BigInteger>(null, blockParameter);
        }
    }
}
