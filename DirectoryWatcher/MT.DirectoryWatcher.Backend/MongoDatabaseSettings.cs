using System;
using MT.DirectoryWatcher.Common;

namespace MT.DirectoryWatcher.Backend
{
    public class MongoDatabaseSettings : IDatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
