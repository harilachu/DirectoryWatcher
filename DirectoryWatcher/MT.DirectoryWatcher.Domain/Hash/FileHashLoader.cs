using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MT.DirectoryWatcher.Common;
using MT.DirectoryWatcher.Common.Interface.Hash;

namespace MT.DirectoryWatcher.Domain.Hash
{
    public class FileHashLoader : IHashLoader
    {
        private readonly ILogger<FileHashLoader> _logger;
        private readonly IHashGenerator _hashGenerator;
        private readonly IDataProvider _dataProvider;
        private Dictionary<string, HashSet<FileHashInfoDto>> FileHashDict { get; set; }

        public FileHashLoader(IHashGenerator hashGenerator, IDataProvider dataProvider, ILogger<FileHashLoader> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._hashGenerator = hashGenerator ?? throw new ArgumentNullException(nameof(hashGenerator));
            this._dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.FileHashDict = new Dictionary<string, HashSet<FileHashInfoDto>>();
        }

        public async Task GetFileHashInfoFromDB()
        {
            await Task.Run(() =>
            {
                try
                {
                    var directoryList = DirectoryConfigurator.GetDirectoryList();
                    if (directoryList == null) return;
                    var appList = directoryList.DirectoryPath.Apps?.ToList();
                    if (appList == null) return;
                    var fileHashSet = new HashSet<FileHashInfoDto>();
                    foreach (var app in appList)
                    {
                        var fileInfos = this._dataProvider.GetFileInfos(app.AppName).ToList();
                        if (fileInfos?.Count > 0)
                            fileInfos.ForEach(x => fileHashSet.Add(x));

                        if (fileHashSet.Count > 0)
                            FileHashDict.Add(app.AppName, fileHashSet);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            });
        }

        public async Task LoadInitialFileHashesToDB()
        {
            await Task.Run(() =>
            {
                var directoryList = DirectoryConfigurator.GetDirectoryList();
                if (directoryList == null) return;

                var appList = directoryList.DirectoryPath?.Apps?.ToList();
                if (appList == null)
                {
                    _logger.LogWarning("No directory paths configured to generate hashes."); 
                    return;
                }

                try
                {
                    HashDirectory(appList);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            });
        }

        private void HashDirectory(List<Apps> appList)
        {
            foreach (var app in appList)
            {
                if (!Directory.Exists(app.Location)) continue;
                var dir = new DirectoryInfo(app.Location);
                GetDirectoryAndHash(dir, app);
            }
        }

        private void GetDirectoryAndHash(DirectoryInfo dir, Apps app)
        {
            FileInfo[] dirFiles = dir.GetFiles();
            GetFileAndHash(dirFiles, app);

            var subDirectories = dir.GetDirectories();
            var subDirList = subDirectories.ToList();
            if (subDirList.Count > 0)
            {
                subDirList.ForEach(x=>
                {
                    GetDirectoryAndHash(x, app);
                });
                
            }
        }

        private void GetFileAndHash(FileInfo[] files, Apps app)
        {
            string hashString = string.Empty;
            List<FileHashInfoDto> beAddedList = new List<FileHashInfoDto>();

            try
            {
                foreach (FileInfo fInfo in files)
                {
                    var fileHashSet = FileHashDict.ContainsKey(app.AppName)? FileHashDict[app.AppName] : new HashSet<FileHashInfoDto>();
                    FileHashInfoDto fileHashInfoDto = null;
                    if (fileHashSet?.Count > 0)
                    {
                        fileHashInfoDto = fileHashSet.FirstOrDefault(x =>
                            x.FileName.Equals(fInfo.Name, StringComparison.OrdinalIgnoreCase) && 
                            x.DirectoryPath.Equals(fInfo.DirectoryName, StringComparison.OrdinalIgnoreCase));
                    }

                    hashString = GenerateHash(fInfo);

                    if (fileHashInfoDto != null)
                    {
                        UpdateNewHashInDb(app, fileHashInfoDto, hashString);
                    }
                    else
                    {
                        fileHashInfoDto = new FileHashInfoDto
                        {
                            FileName = fInfo.Name,
                            DirectoryPath = fInfo.DirectoryName,
                            OldHash = hashString,
                            NewHash = hashString,
                            FileChange = ChangeType.Created,
                            ParentDirectory = app.Location
                        };
                        beAddedList.Add(fileHashInfoDto);
                    }
                }

                AddNewHashListInDb(app, beAddedList);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
            
        }

        private void AddNewHashListInDb(Apps app, List<FileHashInfoDto> beAddedList)
        {
            if (app == null) throw new ArgumentException(@"app is null", nameof(app));
            if (beAddedList == null) throw new ArgumentException(@"beAddedList is null", nameof(beAddedList));

            if (beAddedList.Count > 0)
            {
                this._dataProvider.AddFileInfos(app.AppName, beAddedList);
                if (FileHashDict.ContainsKey(app.AppName))
                    beAddedList.ForEach(x => this.FileHashDict[app.AppName].Add(x));
                else
                    this.FileHashDict.Add(app.AppName, new HashSet<FileHashInfoDto>(beAddedList));
            }
        }

        private void UpdateNewHashInDb(Apps app, FileHashInfoDto fileHashInfoDto, string hashString)
        {
            if (app == null) throw new ArgumentException(@"app is null", nameof(app));
            if (fileHashInfoDto == null) throw new ArgumentException(@"fileHashInfoDto is null", nameof(fileHashInfoDto));
            if (hashString == null) throw new ArgumentException(@"hashString is null", nameof(hashString));

            if (!fileHashInfoDto.OldHash.Equals(hashString))
            {
                fileHashInfoDto.NewHash = hashString;
                fileHashInfoDto.FileChange = ChangeType.Modified;
                this._dataProvider.UpdateFileInfo(app.AppName, fileHashInfoDto);
                //ToDo: Add file tampered list in blockchain
            }
        }

        private string GenerateHash(FileInfo fInfo)
        {
            string hashString;
            if(fInfo == null) return String.Empty;
            using (FileStream fileStream = fInfo.Open(FileMode.Open))
            {
                var hash = this._hashGenerator.GetHashFromFileStream(fileStream);
                hashString = string.Concat(hash.Select(x => x.ToString("X2")));
            }

            return hashString;
        }

        public void GenerateNewHash(FileInfo fileInfo, string directoryPath)
        {
            try
            {
                var app = GetApp(directoryPath);
                if (app != null)
                {
                    FileInfo[] fileInfos = new[] { fileInfo };
                    GetFileAndHash(fileInfos, app);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        public void RenameFile(string oldFileName, FileInfo newFileInfo, string directoryPath)
        {
            try
            {
                var app = GetApp(directoryPath);
                if (app != null)
                {
                    var fileHashSet = FileHashDict.ContainsKey(app.AppName)
                        ? FileHashDict[app.AppName]
                        : new HashSet<FileHashInfoDto>();
                    FileHashInfoDto fileHashInfoDto = null;
                    if (fileHashSet?.Count > 0)
                    {
                        fileHashInfoDto = fileHashSet.FirstOrDefault(x =>
                            x.FileName.Equals(oldFileName, StringComparison.OrdinalIgnoreCase) &&
                            x.DirectoryPath.Equals(newFileInfo.DirectoryName, StringComparison.OrdinalIgnoreCase));
                        if (fileHashInfoDto != null)
                        {
                            fileHashInfoDto.FileName = newFileInfo.Name;
                            fileHashInfoDto.FileChange = ChangeType.Renamed;
                            var oldFileInfoDto = new FileHashInfoDto()
                                { DirectoryPath = fileHashInfoDto.DirectoryPath, FileName = oldFileName };
                            var oldFileInfo = _dataProvider.GetFileInfo(app.AppName, oldFileInfoDto);
                            //Remove and add renamed file
                            _dataProvider.RemoveFileInfos(app.AppName, oldFileInfo);
                            _dataProvider.AddFileInfo(app.AppName, fileHashInfoDto);
                            //ToDo: Add file tampered list in blockchain
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        public void RenameDirectoryWithinFiles(string fileName, string oldDirPath, DirectoryInfo newDirInfo, string directoryPath)
        {
            try
            {
                var app = GetApp(directoryPath);
                if (app != null)
                {
                    var fileHashSet = FileHashDict.ContainsKey(app.AppName)
                        ? FileHashDict[app.AppName]
                        : new HashSet<FileHashInfoDto>();
                    FileHashInfoDto fileHashInfoDto = null;
                    if (fileHashSet?.Count > 0)
                    {
                        fileHashInfoDto = fileHashSet.FirstOrDefault(x =>
                            x.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) &&
                            x.DirectoryPath.StartsWith(oldDirPath, StringComparison.OrdinalIgnoreCase));
                        if (fileHashInfoDto != null)
                        {
                            fileHashInfoDto.FileChange = ChangeType.Renamed;
                            var oldFileInfoDto = new FileHashInfoDto()
                                { DirectoryPath = fileHashInfoDto.DirectoryPath, FileName = fileHashInfoDto.FileName };
                            var oldFileInfo = _dataProvider.GetFileInfo(app.AppName, oldFileInfoDto);
                            //Change new directory path after getting old fileinfo
                            fileHashInfoDto.DirectoryPath = newDirInfo.FullName;
                            //Remove and add renamed file
                            _dataProvider.RemoveFileInfos(app.AppName, oldFileInfo);
                            _dataProvider.AddFileInfo(app.AppName, fileHashInfoDto);
                            //ToDo: Add file tampered list in blockchain
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        private void GetDirectoryAndRename(string oldDirPath, DirectoryInfo newDirInfo, string directoryPath)
        {
            FileInfo[] dirFiles = newDirInfo.GetFiles();
            foreach (var fileInfo in dirFiles)
            {
                RenameDirectoryWithinFiles(fileInfo.Name, oldDirPath, newDirInfo, directoryPath);
            }
            //ToDo: Add to Directory tampered list in blockchain

            var subDirectories = newDirInfo.GetDirectories();
            var subDirList = subDirectories.ToList();
            if (subDirList.Count > 0)
            {
                subDirList.ForEach(x =>
                {
                    GetDirectoryAndRename(oldDirPath, x, directoryPath);
                });
            }
        }

        public void RenameDirectory(string oldDirPath, DirectoryInfo newDir, string directoryPath)
        {
            try
            {
                var app = GetApp(directoryPath);
                if (app != null && newDir != null && !string.IsNullOrEmpty(oldDirPath))
                {
                    GetDirectoryAndRename(oldDirPath, newDir, directoryPath);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        public void DeleteFile(FileInfo f, string directoryPath)
        {
            try
            {
                var app = GetApp(directoryPath);
                if (app != null)
                {
                    var fileHashSet = FileHashDict.ContainsKey(app.AppName)
                        ? FileHashDict[app.AppName]
                        : new HashSet<FileHashInfoDto>();
                    FileHashInfoDto fileHashInfoDto = null;
                    if (fileHashSet?.Count > 0)
                    {
                        fileHashInfoDto = fileHashSet.FirstOrDefault(x =>
                            x.FileName.Equals(f.Name, StringComparison.OrdinalIgnoreCase) &&
                            x.DirectoryPath.Equals(f.DirectoryName, StringComparison.OrdinalIgnoreCase));
                        if (fileHashInfoDto != null)
                        {
                            fileHashInfoDto.FileName = f.Name;
                            fileHashInfoDto.FileChange = ChangeType.Deleted;
                            var oldFileInfoDto = new FileHashInfoDto()
                                { DirectoryPath = fileHashInfoDto.DirectoryPath, FileName = fileHashInfoDto.FileName };
                            var oldFileInfo = _dataProvider.GetFileInfo(app.AppName, oldFileInfoDto);
                            //Remove and add renamed file
                            _dataProvider.RemoveFileInfos(app.AppName, oldFileInfo);
                            _dataProvider.AddFileInfo(app.AppName, fileHashInfoDto);
                            //ToDo: Add file tampered list in blockchain
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        public void DeleteDirectory(DirectoryInfo directoryInfo, string directoryPath)
        {
            try
            {
                //ToDo: Add to Directory tampered list in blockchain

                var app = GetApp(directoryPath);
                if (app != null && directoryInfo != null)
                {
                    var fileHashSet = FileHashDict.ContainsKey(app.AppName)
                        ? FileHashDict[app.AppName]
                        : new HashSet<FileHashInfoDto>();
                    if (fileHashSet?.Count > 0)
                    {
                        var fileHashInfoList = fileHashSet.Where(x =>
                            x.DirectoryPath.StartsWith(directoryInfo.FullName, StringComparison.OrdinalIgnoreCase)).ToList();

                        foreach (var fileHashInfoDto in fileHashInfoList)
                        {
                            fileHashInfoDto.FileChange = ChangeType.Deleted;
                            var oldFileInfoDto = new FileHashInfoDto()
                                { DirectoryPath = fileHashInfoDto.DirectoryPath, FileName = fileHashInfoDto.FileName };
                            var oldFileInfo = _dataProvider.GetFileInfo(app.AppName, oldFileInfoDto);
                            //Remove and add renamed file
                            _dataProvider.RemoveFileInfos(app.AppName, oldFileInfo);
                            _dataProvider.AddFileInfo(app.AppName, fileHashInfoDto);
                            //ToDo: Add file tampered list in blockchain
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        private Apps GetApp(string directoryPath)
        {
            var directoryList = DirectoryConfigurator.GetDirectoryList();
            if (directoryList == null) return null;

            var appList = directoryList.DirectoryPath?.Apps?.ToList();
            if (appList == null)
            {
                _logger.LogWarning("No directory paths configured to generate hashes.");
                return null;
            }

            var app = appList.FirstOrDefault(x => x.Location.Equals(directoryPath));
            return app;
        }
    }
}
