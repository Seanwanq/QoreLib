using System;
using System.Collections.Generic;
using System.Data.Entity;
using Microsoft.Data.Sqlite;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QoreLib.Models;

namespace QoreLib.Services;

public interface IDatabaseService
{
    ScientificDatabaseContext? SciDB { get; set; }
    bool IsDatabaseConnected { get; set; }
    void ConnectScientificDatabase(string folderPath, string dbName);
    void CreateAndConnectScientificDatabase(string folderPath, string dbName);
    void CloseScientificDatabaseConnection();
    void AddDataToTestTable(string? name, bool isMale);
}

public class DatabaseService : IDatabaseService
{
    public ScientificDatabaseContext? SciDB { get; set; }
    public bool IsDatabaseConnected { get; set; }

    public void ConnectScientificDatabase(string folderPath, string dbName)
    {
        string url = Path.Combine(folderPath, dbName);
        if (string.IsNullOrWhiteSpace(url) || !File.Exists(url))
        {
            throw new Exception($"Database file {dbName} does not exist.");
        }

        try
        {
            SciDB = new ScientificDatabaseContext(folderPath, dbName);
        }
        catch (Exception e)
        {
            throw new Exception($"Error connecting the database: {e.Message}");
        }

        SciDB.Database.ExecuteSqlRaw($@"
CREATE TABLE IF NOT EXISTS SpectrumTable(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    GroupId INTEGER NOT NULL,
    DataFile TEXT NOT NULL,
    Omega01File TEXT,
    Omega12File TEXT,
    APP TEXT NOT NULL,
    Type TEXT NOT NULL,
    Name TEXT NOT NULL,
    IsFilled INTEGER NOT NULL,
    IsUseful INTEGER,
    CreateTime TEXT NOT NULL,
    AlterTime TEXT NOT NULL,
    MarkTime TEXT
);

CREATE TABLE IF NOT EXISTS TestTable(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    IsMale INTEGER NOT NULL,
    CreateTime TEXT NOT NULL 
);
");
    }

    public void CreateAndConnectScientificDatabase(string folderPath, string dbName)
    {
        var dbPath = Path.Combine(folderPath, dbName);
        if (System.IO.File.Exists(dbPath))
        {
            throw new Exception($"Database {dbName} exists.");
        }
        else if (string.IsNullOrWhiteSpace(dbPath) || string.IsNullOrWhiteSpace(folderPath) ||
                 string.IsNullOrWhiteSpace(dbName))
        {
            throw new Exception("Please provide a valid database path.");
        }
        else
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                SQLiteConnection.CreateFile(dbPath);
            }
            catch (Exception e)
            {
                throw new Exception($"Error creating database: {e.Message}");
            }
        }

        ConnectScientificDatabase(folderPath, dbName);
    }

    public void CloseScientificDatabaseConnection()
    {
        SciDB?.Disconnect();
        SciDB = null;
    }

    public void AddDataToTestTable(string? name, bool isMale)
    {
        try
        {
            SciDB?.Add(new TestModel { Name = name, IsMale = isMale, CreateTime = DateTime.Now });
            SciDB?.SaveChanges();
        }
        catch (Exception e)
        {
            throw new Exception($"Error: {e.Message}");
        }
    }
}