using LexiScanData.Models;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace LexiScanData
{
    public class LexiScanDbContext : DbContext
    {
        public DbSet<Sentences> Sentences { get; set; }
        public DbSet<Word> Words { get; set; }              // ✅ Word (số ít)
        public DbSet<SentenceWord> SentenceWords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionSettings =
                ConfigurationManager.ConnectionStrings["LexiScanConnection"];

            if (connectionSettings == null)
            {
                throw new InvalidOperationException(
                    "Connection string 'LexiScanConnection' not found.");
            }

            optionsBuilder.UseSqlServer(connectionSettings.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SentenceWord>()
                .HasKey(sw => new { sw.SentenceId, sw.WordId });
        }
    }
}
