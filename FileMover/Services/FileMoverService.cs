using FileMover.Enums;
using FileMover.Interface;

namespace FileMover.Services
{
    public class FileMoverService : IFileMoverService
    {
        private IFileProcessor FileProcessor { get; }
        private IFileSystem FileSystem { get; }
        private ISession Session { get; }
        private IFileTransferDataAccess FileTransferDataAccess { get; }
        private IWriter Writer { get; }
        private ITaskFactory TaskFactory { get; }

        public FileMoverService(IFileProcessor fileProcessor, IFileSystem fileSystem, ISession session, IFileTransferDataAccess fileTransferDataAccess, IWriter writer, ITaskFactory taskFactory)
        {
            FileProcessor = fileProcessor ?? throw new ArgumentNullException(nameof(fileProcessor));
            FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            Session = session ?? throw new ArgumentNullException(nameof(session));
            FileTransferDataAccess = fileTransferDataAccess ?? throw new ArgumentNullException(nameof(fileTransferDataAccess));
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            TaskFactory = taskFactory ?? throw new ArgumentNullException(nameof(taskFactory));
        }

        public async Task MoveFilesAsync(string sourceDirectory, string destinationDirectory)
        {
            if(sourceDirectory == null)
            {
                throw new ArgumentNullException(nameof(sourceDirectory));
            }

            if(destinationDirectory == null)
            {
                throw new ArgumentNullException(nameof(destinationDirectory));
            }

            if(string.Equals(sourceDirectory, destinationDirectory, StringComparison.OrdinalIgnoreCase))
            {
                Writer.WriteLine("Source directory must be different from destination directory");
                return;
            }

            else
            {
                Writer.WriteLine($"Transferring files from {sourceDirectory} to {destinationDirectory}");
            }

            var files = FileSystem.GetDirectoryFiles(sourceDirectory);
            
            if(files.Any())
            {
                Session.AddToSession(sourceDirectory, files.ToList());

                await FileProcessor.CopyFilesAsync(files, destinationDirectory);
            }
        }

        public async Task PrintFileStatusAsync()
        {
            var items = await FileTransferDataAccess.GetSessionFileTransfersAsync(Session.SessionId);

            if(!items.Any())
            {
                Writer.WriteLine("No transfers to display");
            }

            foreach(var item in items)
            {
                Writer.WriteLine(item.ToString());
            }
        }

        public async Task LoadPartialDownloadsAsync()
        {
            var items = await FileTransferDataAccess.GetIncompleteTransfersAsync(Session.SessionId);

            foreach (var item in items)
            {
                if (item.Status == TransferStatus.Copying && FileSystem.FileExists(item.DestinationPath))
                {
                    FileSystem.DeleteFile(item.DestinationPath);
                }

                TaskFactory.CreateAndStartTask(() => FileProcessor.CopyFilesAsync(new[] { item.SourcePath }, Path.GetDirectoryName(item.DestinationPath), item.Id), TaskCreationOptions.LongRunning);
            }
        }
    }
}
