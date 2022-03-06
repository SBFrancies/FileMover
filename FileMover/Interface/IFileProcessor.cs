namespace FileMover.Interface
{
    public interface IFileProcessor
    {
        Task CopyFilesAsync(string[] files, string destinationFolder, Guid? id = null);
    }
}
