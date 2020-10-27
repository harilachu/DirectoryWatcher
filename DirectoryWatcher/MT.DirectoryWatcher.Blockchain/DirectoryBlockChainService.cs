using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MT.DirectoryWatcher.Blockchain.DirectoryWatcherContract;
using MT.DirectoryWatcher.Blockchain.DirectoryWatcherContract.ContractDefinition;
using MT.DirectoryWatcher.Common;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace MT.DirectoryWatcher.Blockchain
{
    public class DirectoryBlockChainService : IDirectoryBlockchainService
    {
        private const string ConstBlockchainUrl = "http://127.0.0.1:8504";
        private const string ConstPrivateKey= "0x3cc5bfac7471a4726dfee070bad124fb6e7e06c6f05957d9f3fbbcb840065d00";
        private const string ConstContractAddress = "0x37e35caEf786CE4c96A99C75DA25AF2940dCf23C";

        private string m_contractAddress;
        private readonly ILogger<DirectoryBlockChainService> _logger;

        public string ContractAddress
        {
            get
            {
                if (string.IsNullOrEmpty(m_contractAddress)) 
                    return ConstContractAddress;
                else 
                    return this.m_contractAddress;
            }
            set { this.m_contractAddress = value; }
        }

        public DirectoryBlockChainService(ILogger<DirectoryBlockChainService> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.m_contractAddress = string.Empty;
        }

        public async Task DeployNewContract()
        {
            try
            {
                // Setup
                var account = new Account(ConstPrivateKey);
                var web3 = new Web3(account, ConstBlockchainUrl);

                CancellationTokenSource cancellation = new CancellationTokenSource(10000);
                var service = await DirectoryWatcherContractService.DeployContractAndGetServiceAsync(web3, new DirectoryWatcherContractDeployment(), cancellation);
                this.ContractAddress = service.ContractHandler.ContractAddress;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }


        public async Task<FetchListCountsOutputDTO> FetchTamperedListCounts()
        {
            try
            {
                var account = new Account(ConstPrivateKey);
                var web3 = new Web3(account, ConstBlockchainUrl);
                //SetGasPrice(web3);
                var service = new DirectoryWatcherContractService(web3, this.ContractAddress);

                var listCountDTO = await service.FetchListCountsQueryAsync();
                return listCountDTO;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            return new FetchListCountsOutputDTO();
        }

      

        public async Task<FetchTamperedDirectoriesOutputDTO> FetchTamperedDirectories(int index)
        {
            try
            {
                var account = new Account(ConstPrivateKey);
                var web3 = new Web3(account, ConstBlockchainUrl);
                //SetGasPrice(web3);
                var service = new DirectoryWatcherContractService(web3, this.ContractAddress);

                var directoriesDto = await service.FetchTamperedDirectoriesQueryAsync(new BigInteger(index));
                return directoriesDto;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            return new FetchTamperedDirectoriesOutputDTO();
        }

        public async Task<FetchTamperedFilesOutputDTO> FetchTamperedFiles(int index)
        {
            try
            {
                var account = new Account(ConstPrivateKey);
                var web3 = new Web3(account, ConstBlockchainUrl);
                //SetGasPrice(web3);
                var service = new DirectoryWatcherContractService(web3, this.ContractAddress);

                var filesDto = await service.FetchTamperedFilesQueryAsync(new BigInteger(index));
                return filesDto;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            return new FetchTamperedFilesOutputDTO();
        }

        public async Task<ReceiptStatus> InsertTamperedDirectory(DirectoryInfoDto directoryInfoDto)
        {
            try
            {
                var account = new Account(ConstPrivateKey);
                var web3 = new Web3(account, ConstBlockchainUrl);

                //SetGasPrice(web3);
                var service = new DirectoryWatcherContractService(web3, this.ContractAddress);

                var receipt = await service.InsertTamperedDirectoryRequestAndWaitForReceiptAsync(
                    directoryInfoDto.DirectoryName,
                    directoryInfoDto.DirectoryPath,
                    directoryInfoDto.DirectoryChange.ToString());

                var status = GetReceiptStatus(receipt);
                return status;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            return ReceiptStatus.Failure;
        }


        public async Task<ReceiptStatus> InsertTamperedFiles(FileHashInfoDto filleHashInfoDto)
        {
            try
            {
                var account = new Account(ConstPrivateKey);
                var web3 = new Web3(account, ConstBlockchainUrl);

               //SetGasPrice(web3);
                var service = new DirectoryWatcherContractService(web3, this.ContractAddress);
                
                var receipt = await service.InsertTamperedFileRequestAndWaitForReceiptAsync(
                    filleHashInfoDto.FileName,
                    filleHashInfoDto.ParentDirectory,
                    filleHashInfoDto.DirectoryPath,
                    filleHashInfoDto.OldHash,
                    filleHashInfoDto.NewHash,
                    filleHashInfoDto.FileChange.ToString());
                
                var status = GetReceiptStatus(receipt);
                return status;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            return ReceiptStatus.Failure;
        }

        private void SetGasPrice(Web3 web3)
        {
            web3.TransactionManager.DefaultGasPrice = new BigInteger(10);
            web3.TransactionManager.DefaultGas = new BigInteger(10);
        }

        private ReceiptStatus GetReceiptStatus(TransactionReceipt receipt)
        {
            if (receipt == null) return ReceiptStatus.Failure;

            bool result = int.TryParse(receipt.Status.Value.ToString(), out var statusInt);
            ReceiptStatus status;
            if (result)
                status = (ReceiptStatus)statusInt;
            else
                status = Common.ReceiptStatus.Failure;

            return status;
        }
    }
}
