using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FileMover.Data
{
    public class FileMoverDbContextFactory : IDesignTimeDbContextFactory<FileMoverDbContext>
    {
        public FileMoverDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FileMoverDbContext>();
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var dbPath = Path.Join(path, "FileMoverDB.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            return new FileMoverDbContext(optionsBuilder.Options);
        }
    }
}
