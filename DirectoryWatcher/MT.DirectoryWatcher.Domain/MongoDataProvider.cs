using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MT.DirectoryWatcher.Backend;
using MT.DirectoryWatcher.Common;

namespace MT.DirectoryWatcher.Domain
{
    public class MongoDataProvider : IDataProvider
    {
        private readonly ILogger<MongoDataProvider> _logger;
        public FileHashInfo FileHashInfo { get; }
        public IRepository Repository { get; }
        public IMapper Mapper { get; }

        public MongoDataProvider(IRepository repository, IMapper mapper, ILogger<MongoDataProvider> logger)
        {
            this.Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public FileHashInfoDto GetFileInfo(string appName, FileHashInfoDto fileHashInfo)
        {
            FileHashInfoDto result = new FileHashInfoDto();

            try
            {
                var fileInfo = this.Repository.Single<FileHashInfo>(appName, c=>c.FileName == fileHashInfo.FileName &&
                                                                                 c.DirectoryPath == fileHashInfo.DirectoryPath);
                result = this.Mapper.Map<FileHashInfoDto>(fileInfo);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }

            return result;
        }

        public IEnumerable<FileHashInfoDto> GetFileInfos(string appName)
        {
            IEnumerable<FileHashInfoDto> result = new List<FileHashInfoDto>();
            try
            {
                var fileInfos = this.Repository.All<FileHashInfo>(appName);
                result = this.Mapper.Map<IEnumerable<FileHashInfoDto>>(fileInfos);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
            
            return result;
        }

        public void AddFileInfo(string appName, FileHashInfoDto item)
        {
            try
            {
                var fileHashInfo = this.Mapper.Map<FileHashInfo>(item);
                this.Repository.Add(appName, fileHashInfo);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
        }

        public void AddFileInfos(string appName, IEnumerable<FileHashInfoDto> items)
        {
            try
            {
                var fileHashInfos = this.Mapper.Map<IEnumerable<FileHashInfo>>(items);
                this.Repository.Add(appName, fileHashInfos);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
        }

        public void UpdateFileInfo(string appName, FileHashInfoDto fileHashInfo)
        {
            try
            {
                this.Repository.Update<FileHashInfo>(appName, fileHashInfo.FileName, fileHashInfo.NewHash);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
        }

        public void RemoveFileInfos(string appName, FileHashInfoDto item)
        {
            try
            {
                this.Repository.Delete<FileHashInfo>(appName, 
                    x=>x.FileId == item.FileId &&
                            x.FileName == item.FileName);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
        }

        public void RemoveFileInfos(string appName, IEnumerable<FileHashInfoDto> items)
        {
            try
            {
                var fileHashInfos = this.Mapper.Map<IEnumerable<FileHashInfo>>(items);
                fileHashInfos.ToList().ForEach(x=> this.Repository.Delete<FileHashInfo>(appName,
                    c => c.FileId == x.FileId &&
                            c.FileName == x.FileName));
                
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
        }
    }
}
