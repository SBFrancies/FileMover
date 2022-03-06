using FileMover.Data;
using FileMover.Data.Entities;
using FileMover.Enums;
using FileMover.Interface;
using Microsoft.EntityFrameworkCore;

namespace FileMover.DataAccess
{
    public class FileTransferDataAccess : IFileTransferDataAccess
    {
        private Func<FileMoverDbContext> DbContextFactory { get; }

        public FileTransferDataAccess(Func<FileMoverDbContext> dbContextFactory)
        {
            DbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public async Task<FileTransfer> SaveFileTransferAsync(FileTransfer entity)
        {
            await using var context = DbContextFactory();

            context.FileTransfers.Add(entity);

            await context.SaveChangesAsync();

            return entity;
        }

        public async Task<FileTransfer> UpdateFileTransferAsync(FileTransfer entity)
        {
            await using var context = DbContextFactory();

            var savedEntity = await context.FileTransfers.FirstOrDefaultAsync(a => a.Id == entity.Id);

            if(savedEntity == null)
            {
                return null;
            }

            savedEntity.Status = entity.Status;
            savedEntity.ErrorMessage = entity.ErrorMessage;

            await context.SaveChangesAsync();

            return savedEntity;
        }

        public async Task<IEnumerable<FileTransfer>> GetSessionFileTransfersAsync(Guid sessionId)
        {
            await using var context = DbContextFactory();

            var list = await context.FileTransfers
                .Where(a => a.OriginalSessionId == sessionId || a.CurrentSessionId == sessionId)
                .OrderBy(a => a.Created.Date).ToListAsync();

            return list;
        }

        public async Task<IEnumerable<FileTransfer>> GetIncompleteTransfersAsync(Guid sessionId)
        {
            await using var context = DbContextFactory();

            var list = await context.FileTransfers
                .Where(a => a.Status == TransferStatus.Awaiting || a.Status == TransferStatus.Copying)
                .ToListAsync();

            foreach(var item in list)
            {
                item.CurrentSessionId = sessionId;
            }

            await context.SaveChangesAsync();

            return list;
        }
    }
}
