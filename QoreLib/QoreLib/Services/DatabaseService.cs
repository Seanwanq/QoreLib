using System;
using System.Collections.Generic;
using System.Data.Entity;
using Microsoft.Data.Sqlite;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Linq;

namespace QoreLib.Services;

public interface IDatabaseService
{
    ScientificDatabaseContext SciDB { get; set; }
    
    bool IsDatabaseConnected { get; set; }
    
    void ConnectScientificDatabase(string folderPath, string dbName);

    void CreateAndConnectScientificDatabase(string folderPath, string dbName);

    void CloseScientificDatabaseConnection();
}

public class DatabaseService : IDatabaseService
{
    public ScientificDatabaseContext SciDB { get; set; }
    
    public bool IsDatabaseConnected { get; set; }
    
    public void ConnectScientificDatabase(string folderPath, string dbName)
    {
        var url = Path.Combine(folderPath, dbName);

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
    }

    public void CreateAndConnectScientificDatabase(string folderPath, string dbName)
    {
        var dbPath = Path.Combine(folderPath, dbName);
        if (System.IO.File.Exists(dbPath))
        {
            throw new Exception($"Database {dbName} exists.");
            return;
        }
        else if(string.IsNullOrWhiteSpace(dbPath) || string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(dbName))
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
}