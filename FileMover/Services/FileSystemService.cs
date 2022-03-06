using FileMover.Interface;

namespace FileMover.Services
{
    public class FileSystemService : IFileSystem
    {
        public async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            using var source = File.OpenRead(sourcePath);
            using var destination = File.Create(destinationPath);

            await source.CopyToAsync(destination);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string[] GetDirectoryFiles(string path)
        {
            if(!Directory.Exists(path))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
        }
    }
}
