using AutoMapper;
using MT.DirectoryWatcher.Backend;
using MT.DirectoryWatcher.Common;

namespace MT.DirectoryWatcher.Service
{
    public class WatcherProfile : Profile
    {
        public WatcherProfile()
        {
            this.CreateMap<FileHashInfo, FileHashInfoDto>()
                .ReverseMap();
        }
    }
}
