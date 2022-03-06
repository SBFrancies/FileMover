namespace FileMover.Interface
{
    public interface IFileMoverService
    {
        Task LoadPartialDownloadsAsync();

        Task MoveFilesAsync(string sourceDirectory, string destinationDirectory);

        Task PrintFileStatusAsync();
    }
}
