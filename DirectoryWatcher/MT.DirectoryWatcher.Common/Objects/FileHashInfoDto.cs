using System;
using System.Collections.Generic;
using System.Text;

namespace MT.DirectoryWatcher.Common
{
    public class FileHashInfoDto
    {
        public string FileId { get; set; }
        public string FileName { get; set; }
        public string ParentDirectory { get; set; }
        public string DirectoryPath { get; set; }
        public string OldHash { get; set; }
        public string NewHash { get; set; }
        public ChangeType FileChange { get; set; }
    }
}
