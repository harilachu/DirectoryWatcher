using System;
using System.Collections.Generic;
using System.Text;

namespace MT.DirectoryWatcher.Common
{
    public class DirectoryInfoDto
    {
       public string DirectoryName { get; set; }
       public string DirectoryPath { get; set; }
       public ChangeType DirectoryChange { get; set; }
    }
}
