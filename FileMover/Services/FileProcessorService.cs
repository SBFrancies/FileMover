using FileMover.Data.Entities;
using FileMover.Enums;
using FileMover.Interface;
using Microsoft.Extensions.Internal;
using System.Collections.Concurrent;

namespace FileMover.Services
{
    public class FileProcessorService : IFileProcessor
    {
        private IQueueProcessorFactory<FileTransfer> QueueProcessorFactory { get; }
        private IFileTransferDataAccess FileTransferDataAccess { get; }
        private IIdGenerator IdGenerator { get; }
        private IDateTimeProvider Clock { get; }
        private ISession Session { get; }
        private ITaskFactory TaskFactory { get; }
        private ConcurrentDictionary<string, BlockingCollection<FileTransfer>> FileTransferDictionary { get; } = new ConcurrentDictionary<string, BlockingCollection<FileTransfer>>();

        public FileProcessorService(IQueueProcessorFactory<FileTransfer> queueProcessorFactory, IFileTransferDataAccess fileTransferDataAccess, IIdGenerator idGenerator, IDateTimeProvider clock, ISession session, ITaskFactory taskFactory)
        {
            QueueProcessorFactory = queueProcessorFactory ?? throw new ArgumentNullException(nameof(queueProcessorFactory));
            FileTransferDataAccess = fileTransferDataAccess ?? throw new ArgumentNullException(nameof(fileTransferDataAccess));
            IdGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            Session = session ?? throw new ArgumentNullException(nameof(session));
            TaskFactory = taskFactory ?? throw new ArgumentNullException(nameof(taskFactory));
        }

        public async Task CopyFilesAsync(string[] files, string destinationFolder, Guid? id = null)
        {
            var tasks = files.Select(async a =>
            { 
                var entity = new FileTransfer
                {
                    Id = id ?? IdGenerator.GenerateId(),
                    OriginalSessionId = Session.SessionId,
                    Created = Clock.UtcNow.UtcDateTime,
                    Status = TransferStatus.Awaiting,
                    SourcePath = a,
                    DestinationPath = Path.Combine(destinationFolder, Path.GetFileName(a)),
                };

                if (id == null)
                {
                    await FileTransferDataAccess.SaveFileTransferAsync(entity);
                }

                var extension = Path.GetExtension(a).ToUpperInvariant();
                var collection = new BlockingCollection<FileTransfer>(new ConcurrentQueue<FileTransfer>());

                if (FileTransferDictionary.TryAdd(extension, collection))
                {
                    var queueProcessor = QueueProcessorFactory.CreateQueueProcessor();
                    TaskFactory.CreateAndStartTask(() => queueProcessor.ProcessQueueAsync(collection), TaskCreationOptions.LongRunning);
                }

                FileTransferDictionary[extension].Add(entity);
            });

            await Task.WhenAll(tasks);
        }
    }
}
