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
    internal class LexiScanDbContext : DbContext
    {
        public DbSet<Sentences> Sentences { get; set; }

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
    }
}
