using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MT.DirectoryWatcher.Common
{
    public interface ICommunicationService
    {
        //Task<IEnumerable<SoftwareDto>> GetInstalledSoftwaresList();

        string GetValue();
    }
}
