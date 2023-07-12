using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using QoreLib.Models;

namespace QoreLib.Services;

public class ScientificDatabaseContext : DbContext
{
    public DbSet<SpectrumModel> SpectrumTable { get; set; }
    
    public string DbPath { get; }

    public ScientificDatabaseContext(string folderPath, string dbName)
    {
        DbPath = Path.Combine(folderPath, dbName);
    }

    public void Disconnect()
    {
        Dispose();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (string.IsNullOrWhiteSpace(DbPath) || !System.IO.File.Exists(DbPath))
        {
            throw new Exception("Database is not existed.");
            return;
        }
        options.UseSqlite($"Data Source={DbPath}");
    }
}