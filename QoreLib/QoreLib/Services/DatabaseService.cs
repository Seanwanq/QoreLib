using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using Microsoft.Data.Sqlite;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Linq;
using LiveChartsCore.Defaults;
using Microsoft.EntityFrameworkCore;
using QoreLib.Models;

namespace QoreLib.Services;

public interface IDatabaseService
{
    static ScientificDatabaseContext? SciDB { get; set; }

    public delegate void DatabaseConnectedChangedEventHandler(bool isConnected);

    event DatabaseConnectedChangedEventHandler DatabaseConnectedChanged;
    bool IsDatabaseConnected { get; set; }
    SpectrumModel[]? SpectrumTable { get; set; }
    TestModel[]? TestTable { get; set; }
    int DatasetLength { get; set; }
    string BaseFolderPath { get; set; }
    void ConnectScientificDatabase(string folderPath, string dbName);
    void CreateAndConnectScientificDatabase(string folderPath, string dbName);
    void CloseScientificDatabaseConnection();
    void AddDataToTestTable(string? name, bool isMale);
    ObservableCollection<ObservablePoint> SelectDataToObservableCollectionById(int id);
    int GetNextId(int currentId);
    int GetPreviousId(int currentId);
    int GetNextUnfilledId(int currentId);
    int GetPreviousUnfilledId(int currentId);
    int GetLastDataId();
    int GetFirstDataId();
}

public class DatabaseService : IDatabaseService
{
    public static ScientificDatabaseContext? SciDB { get; set; }
    public event IDatabaseService.DatabaseConnectedChangedEventHandler DatabaseConnectedChanged;
    private bool _isDatabaseConnected;

    public bool IsDatabaseConnected
    {
        get { return _isDatabaseConnected; }
        set
        {
            if (_isDatabaseConnected != value)
            {
                _isDatabaseConnected = value;
                DatabaseConnectedChanged?.Invoke(value);
            }
        }
    }

    public SpectrumModel[]? SpectrumTable { get; set; }
    public TestModel[]? TestTable { get; set; }
    public int DatasetLength { get; set; } = 0;
    public string BaseFolderPath { get; set; } = "";

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
    IsWrongData INTEGER DEFAULT 0,
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
        SpectrumTable = SciDB.SpectrumTable.ToArray();
        TestTable = SciDB.TestTable.ToArray();
        DatasetLength = SpectrumTable.Length;
        BaseFolderPath = folderPath;
        IsDatabaseConnected = true;
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
        SpectrumTable = null;
        TestTable = null;
        DatasetLength = 0;
        BaseFolderPath = "";
        IsDatabaseConnected = false;
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

    public ObservableCollection<ObservablePoint> SelectDataToObservableCollectionById(int id)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        var selectedData = SpectrumTable.FirstOrDefault(s => s.Id == id);
        if (selectedData != null)
        {
            List<double> indexList =
                JsonService.ReadBaseDataJson(Path.Join(BaseFolderPath, selectedData.DataFile))["Index"];
            List<double> valueReal =
                JsonService.ReadBaseDataJson(Path.Join(BaseFolderPath, selectedData.DataFile))["ValueReal"];
            if (indexList.Count != valueReal.Count)
            {
                throw new Exception($"Inconsistent length of horizontal and vertical data with Id: {id}");
            }

            double[,] mergedArray = new double[indexList.Count, 2];
            for (int i = 0; i < indexList.Count; i++)
            {
                mergedArray[i, 0] = indexList[i];
                mergedArray[i, 1] = valueReal[i];
            }

            ObservableCollection<ObservablePoint> observableCollection = new();
            for (int i = 0; i < mergedArray.GetLength(0); i++)
            {
                double x = mergedArray[i, 0];
                double y = mergedArray[i, 1];
                observableCollection.Add(new ObservablePoint(x, y));
            }

            return observableCollection;
        }
        else
        {
            throw new Exception($"No data found with Id: {id}");
        }
    }

    public int GetNextId(int currentId)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        var laterData = SpectrumTable.Where(s => s.Id > currentId).OrderBy(s => s.Id);
        var nextData = laterData.FirstOrDefault();
        if (nextData != null)
        {
            return nextData.Id;
        }
        else
        {
            return -1;
        }
    }

    public int GetPreviousId(int currentId)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        var pData = SpectrumTable.Where(s => s.Id < currentId)
            .OrderByDescending(s => s.Id);
        var previousData = pData.LastOrDefault();
        if (previousData != null)
        {
            return previousData.Id;
        }
        else
        {
            return -2;
        }
    }

    public int GetNextUnfilledId(int currentId)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        var unfilledData = SpectrumTable.Where(s => s.Id > currentId && s.IsFilled == false).OrderBy(s => s.Id);
        var nextUnfilledData = unfilledData.FirstOrDefault();
        if (nextUnfilledData != null)
        {
            return nextUnfilledData.Id;
        }
        else
        {
            return -1;
        }
    }

    public int GetPreviousUnfilledId(int currentId)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        var unfilledData = SpectrumTable.Where(s => s.Id < currentId && s.IsFilled == false)
            .OrderByDescending(s => s.Id);
        var previousUnfilledData = unfilledData.LastOrDefault();
        if (previousUnfilledData != null)
        {
            return previousUnfilledData.Id;
        }
        else
        {
            return -2;
        }
    }

    public int GetLastDataId()
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        var lastData = SpectrumTable.OrderByDescending(s => s.Id).FirstOrDefault();
        if (lastData != null)
        {
            return lastData.Id;
        }
        else
        {
            throw new Exception("No data in database");
        }
    }

    public int GetFirstDataId()
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        var firstData = SpectrumTable.OrderBy(s => s.Id).FirstOrDefault();
        if (firstData != null)
        {
            return firstData.Id;
        }
        else
        {
            throw new Exception("No data in database");
        }
    }
}