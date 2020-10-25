using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using JKang.IpcServiceFramework.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MT.DirectoryWatcher.Backend;
using MT.DirectoryWatcher.Common;
using MT.DirectoryWatcher.Common.Interface.Hash;
using MT.DirectoryWatcher.Domain;
using MT.DirectoryWatcher.Domain.Hash;

namespace MT.DirectoryWatcher.Service
{
    public class Program
    {
        static IServiceProvider serviceProvider;
        static IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<DirectoryWatcherService>()
                        .AddSingleton<IDirectoryWatcher, Domain.DirectoryWatcher>()
                        .AddSingleton<IDataProvider, MongoDataProvider>()
                        .AddSingleton<IHashLoader, FileHashLoader>()
                        .AddTransient<IDirectoryMonitor, DirectoryMonitor>()
                        .AddTransient<IHashGenerator, Sha256HashGenerator>()
                        .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

                    //Do not change this init order.
                    serviceProvider = services.BuildServiceProvider();
                    Configuration = serviceProvider.GetService<IConfiguration>();
                    services.Configure<MongoDatabaseSettings>(
                        Configuration.GetSection(nameof(MongoDatabaseSettings)));

                    services.AddSingleton<IDatabaseSettings>(sp =>
                            sp.GetRequiredService<IOptions<MongoDatabaseSettings>>().Value)
                        .AddSingleton<IRepository, FileInfoRepository>();
                })
                .ConfigureLogging(logBuilder =>
                {
                    logBuilder.SetMinimumLevel(LogLevel.Information);
                    logBuilder.AddLog4Net("log4net.config");

                })
                .ConfigureIpcHost(builder => builder.AddTcpEndpoint<ICommunicationService>(ipEndpoint: IPAddress.Loopback, port: 45684));
    }
}
