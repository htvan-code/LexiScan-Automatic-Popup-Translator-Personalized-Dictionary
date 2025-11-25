// Project: LexiScan.Data
// File: LexiScanDbContext.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public class LexiScanDbContext : DbContext
{
    public DbSet<Sentence> Sentences { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Vui lòng thay thế bằng chuỗi kết nối SQL Server của bạn
        optionsBuilder.UseSqlServer("Server=Your_Server_Name;Database=LexiScanDB;Trusted_Connection=True;");
    }
}