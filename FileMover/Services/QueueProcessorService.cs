using FileMover.Data.Entities;
using FileMover.Enums;
using FileMover.Interface;
using System.Collections.Concurrent;

namespace FileMover.Services
{
    public class QueueProcessorService : IQueueProcessor<FileTransfer>
    {
        private IFileSystem FileSystem { get; }
        private ISession Session { get; }
        private IFileTransferDataAccess FileTransferDataAccess { get; }

        public QueueProcessorService(IFileSystem fileSystem, ISession session, IFileTransferDataAccess fileTransferDataAccess)
        {
            FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            Session = session ?? throw new ArgumentNullException(nameof(session));
            FileTransferDataAccess = fileTransferDataAccess ?? throw new ArgumentNullException(nameof(fileTransferDataAccess));
        }

        public async Task ProcessQueueAsync(BlockingCollection<FileTransfer> queue)
        {
            FileTransfer item;

            while (queue.TryTake(out item, int.MaxValue))
            {
                try
                {
                    if (!FileSystem.DirectoryExists(Path.GetDirectoryName(item.DestinationPath)))
                    {
                        item.Status = TransferStatus.Error;
                        item.ErrorMessage = "Destination directory does not exist";
                    }

                    else if (!FileSystem.FileExists(item.SourcePath))
                    {
                        item.Status = TransferStatus.Error;
                        item.ErrorMessage = "File does not exist in source directory";
                    }

                    else if (FileSystem.FileExists(item.DestinationPath))
                    {
                        item.Status = TransferStatus.Error;
                        item.ErrorMessage = "File already exists in destination directory";
                    }

                    else
                    {
                        item.Status = TransferStatus.Copying;
                        await FileTransferDataAccess.UpdateFileTransferAsync(item);
                        await FileSystem.CopyFileAsync(item.SourcePath, item.DestinationPath);
                        FileSystem.DeleteFile(item.SourcePath);
                        item.Status = TransferStatus.Done;
                    }

                    Session.TransferComplete(item.SourcePath);
                    await FileTransferDataAccess.UpdateFileTransferAsync(item);
                }

                catch (Exception exception)
                {
                    if (item != null)
                    {
                        item.Status = TransferStatus.Error;
                        item.ErrorMessage = exception.Message;
                        Session.TransferComplete(item.SourcePath);
                        await FileTransferDataAccess.UpdateFileTransferAsync(item);
                    }

                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
