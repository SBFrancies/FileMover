using FileMover.Data.Entities;
using FileMover.Interface;
using FileMover.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileMover.Factories
{
    public class QueueProcessorFactory : IQueueProcessorFactory<FileTransfer>
    {
        private IFileSystem FileSystem { get; }
        private ISession Session { get; }
        private IFileTransferDataAccess FileTransferDataAccess { get; }
       
        public QueueProcessorFactory(IFileSystem fileSystem, ISession session, IFileTransferDataAccess fileTransferDataAccess)
        {
            FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            Session = session ?? throw new ArgumentNullException(nameof(session));
            FileTransferDataAccess = fileTransferDataAccess ?? throw new ArgumentNullException(nameof(fileTransferDataAccess));
        }

        public IQueueProcessor<FileTransfer> CreateQueueProcessor()
        {
            return new QueueProcessorService(FileSystem, Session, FileTransferDataAccess);
        }
    }
}
