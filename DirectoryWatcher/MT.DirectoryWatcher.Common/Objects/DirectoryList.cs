using System;
using System.Collections.Generic;
using System.Text;

namespace MT.DirectoryWatcher.Common
{
    public class DirectoryList
    {
        public DirectoryPath DirectoryPath { get; set; }
    }

    public class DirectoryPath
    {
        public Apps[] Apps { get; set; }
    }

    public class Apps
    {
        public string AppName { get; set; }
        public string Location { get; set; }
    }
}
