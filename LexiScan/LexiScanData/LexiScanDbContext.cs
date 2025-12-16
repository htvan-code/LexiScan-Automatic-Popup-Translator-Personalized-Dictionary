using LexiScanData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
namespace LexiScanData
{
    public class LexiScanDbContext : DbContext
    {
        public DbSet<Sentences> Sentences { get; set; }
        public DbSet<Words> Words { get; set; }
        public DbSet<SentenceWord> SentenceWords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings["LexiScanConnection"];

            if (connectionSettings == null)
            {
                throw new InvalidOperationException("Connection string 'LexiScanConnection' not found in App.config.");
            }

            string connectionString = connectionSettings.ConnectionString;

            optionsBuilder.UseSqlServer(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SentenceWord>()
                .HasKey(sw => new { sw.SentenceId, sw.WordId });
        }

    }
}
