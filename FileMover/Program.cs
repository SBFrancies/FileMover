using FileMover.Interface;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using FileMover.Services;
using FileMover.CommandReaders;
using FileMover.DataAccess;
using FileMover.Factories;
using FileMover.Data.Entities;
using FileMover.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FileMover
{
    public class Program
    {
        private static IServiceProvider ServiceProvider { get; set; }
        private static IConfiguration Configuration { get; set; }

        public static async Task Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appSettings.json", true)
              .AddJsonFile("appSettings.test.json", true).Build();

            RegisterServices();
            await ServiceProvider.GetRequiredService<FileMoverDbContext>().Database.MigrateAsync();
            await LoadIncompleteTransfersAsync();
            RegisterObserver();

            new AutoResetEvent(false).WaitOne();
        }

        private static async Task LoadIncompleteTransfersAsync()
        {
            var fileMover = ServiceProvider.GetRequiredService<IFileMoverService>();
            await fileMover.LoadPartialDownloadsAsync();
        }

        private static void RegisterObserver()
        {
            var consoleWatcher = Observable
                .Defer(() =>
                    Observable
                        .Start(ServiceProvider!.GetRequiredService<IReader>().ReadLine)).Repeat().Publish().RefCount();

            var commandReaders = ServiceProvider.GetServices<ICommandReader>();

            foreach (var reader in commandReaders)
            {
                consoleWatcher.Subscribe(input => new Task(() => reader.ValidateAndRunAsync(input), TaskCreationOptions.LongRunning).Start());
            }
        }

        private static void RegisterServices()
        {
            var collection = new ServiceCollection();
            var consoleService = new ConsoleService();
            collection.AddSingleton<IReader>(consoleService);
            collection.AddSingleton<IWriter>(consoleService);
            collection.AddSingleton<ICommandReader, ExitCommandReader>();
            collection.AddSingleton<ICommandReader, GetStatusCommandReader>();
            collection.AddSingleton<ICommandReader, TransferCommandReader>();
            collection.AddSingleton<IDateTimeProvider, ClockService>();
            collection.AddSingleton<IFileMoverService, FileMoverService>();
            collection.AddSingleton<IFileProcessor, FileProcessorService>();
            collection.AddSingleton<IFileSystem, FileSystemService>();
            collection.AddSingleton<IFileTransferDataAccess, FileTransferDataAccess>();
            collection.AddSingleton<IIdGenerator, IdGeneratorService>();
            collection.AddSingleton<IQueueProcessorFactory<FileTransfer>, QueueProcessorFactory>();
            collection.AddSingleton<ISession, SessionService>();
            collection.AddSingleton<ITaskFactory, Factories.TaskFactory>();
            collection.AddSingleton<IApplicationProcesses, ApplicationProcesses>();

            collection.AddDbContext<FileMoverDbContext>(a =>
            {
                var dbLocation = Configuration["DbLocation"];

                if (dbLocation == null || dbLocation == "default")
                {
                    var folder = Environment.SpecialFolder.LocalApplicationData;
                    var path = Environment.GetFolderPath(folder);
                    dbLocation = Path.Join(path, "FileMoverDB.db");
                }                
                a.UseSqlite($"Data Source={dbLocation}");
            }, ServiceLifetime.Transient, ServiceLifetime.Singleton);

            collection.AddSingleton(a => () => a.GetRequiredService<FileMoverDbContext>());

            ServiceProvider = collection.BuildServiceProvider();
        }
    }
}