using FileMover.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileMover.Data
{
    public class FileMoverDbContext : DbContext
    {
        public const string Schema = "dbo";
        public DbSet<FileTransfer> FileTransfers { get; set; }

        public FileMoverDbContext(DbContextOptions<FileMoverDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            modelBuilder.Entity<FileTransfer>(a =>
            {
                a.ToTable("FileTransfers", Schema);
                a.HasKey(b => b.Id);
                a.Property(b => b.SourcePath).HasMaxLength(1000).IsRequired();
                a.Property(b => b.DestinationPath).HasMaxLength(1000).IsRequired();
                a.Property(b => b.Status).HasColumnType("VARCHAR(20)").HasConversion<string>();
            });
        }
    }
}