using FileMover.Data.Entities;

namespace FileMover.Interface
{
    public interface IFileTransferDataAccess
    {
        Task<FileTransfer> SaveFileTransferAsync(FileTransfer entity);

        Task<FileTransfer> UpdateFileTransferAsync(FileTransfer entity);

        Task<IEnumerable<FileTransfer>> GetSessionFileTransfersAsync(Guid sessionId);

        Task<IEnumerable<FileTransfer>> GetIncompleteTransfersAsync(Guid sessionId);
    }
}
