namespace FileMover.Interface
{
    public interface IFileSystem
    {
        Task CopyFileAsync(string sourcePath, string destinationPath);

        void DeleteFile(string path);

        bool FileExists(string path);

        bool DirectoryExists(string path);

        string[] GetDirectoryFiles(string path);
    }
}
