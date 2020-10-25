using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MT.DirectoryWatcher.Common
{
    public interface IHashGenerator
    {
        byte[] GetHashFromData(string data);
        byte[] GetHashFromFileStream(FileStream stream);
    }
}
