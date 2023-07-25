using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using Microsoft.Data.Sqlite;
using System.Data.SQLite;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using LiveChartsCore.Defaults;
using Microsoft.EntityFrameworkCore;
using QoreLib.Models;
using QoreLib.Services.JsonService;
using Exception = System.Exception;

namespace QoreLib.Services;

public interface IDatabaseService
{
    static ScientificDatabaseContext? SciDB { get; set; }

    public delegate void DatabaseConnectedChangedEventHandler(bool isConnected);

    event DatabaseConnectedChangedEventHandler DatabaseConnectedChanged;
    bool IsDatabaseConnected { get; set; }
    SpectrumModel[]? SpectrumTable { get; set; }
    int DatasetLength { get; set; }
    string BaseFolderPath { get; set; }
    void ConnectToScientificDatabase(string folderPath, string dbName);
    void CreateAndConnectToScientificDatabase(string folderPath, string dbName);
    void CloseScientificDatabaseConnection();
    ObservableCollection<ObservablePoint> SelectSpectrumBaseDataToObservableCollectionById(int id);
    int GetSpectrumDataNextId(int currentId);
    int GetSpectrumDataPreviousId(int currentId);
    int GetSpectrumDataNextUnfilledId(int currentId);
    int GetSpectrumDataPreviousUnfilledId(int currentId);
    int GetSpectrumDataLastDataId();
    int GetSpectrumDataFirstDataId();
    int GetSpectrumDataGroupIdById(int id);
    string GetSpectrumDataNameById(int id);
    int GetSpectrumDataIdByGroupIdAndName(int groupId, string name);
    Dictionary<string, string> GetSpectrumDataAppAndTypeById(int id);
    bool GetSpectrumDataIsFilledById(int id);
    ObservableCollection<ObservablePoint> SelectSpectrumOmega01DataToObservableCollectionById(int id);
    ObservableCollection<ObservablePoint> SelectSpectrumOmega12DataToObservableCollectionById(int id);
    Dictionary<string, List<double>> SelectSpectrumAdditionalToDictionaryListDoubleById(int id);

    void SaveSpectrumAdditionalDataToJsonAndUpdateSqlById(int id, SpectrumAdditionalDataModel spectrumAdditionalData,
        bool isUseful, bool isBadData);

    Dictionary<string, bool> GetSpectrumDataIsUsefulAndIsWrongDataById(int id);
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
    public int DatasetLength { get; set; } = 0;
    public string BaseFolderPath { get; set; } = "";

    public void ConnectToScientificDatabase(string folderPath, string dbName)
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
    Omega01AndOmega12File TEXT,
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
");
        SpectrumTable = SciDB.SpectrumTable.ToArray();
        DatasetLength = SpectrumTable.Length;
        BaseFolderPath = folderPath;
        IsDatabaseConnected = true;
    }

    public void CreateAndConnectToScientificDatabase(string folderPath, string dbName)
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

        ConnectToScientificDatabase(folderPath, dbName);
    }

    public void CloseScientificDatabaseConnection()
    {
        SciDB?.Disconnect();
        SciDB = null;
        SpectrumTable = null;
        DatasetLength = 0;
        BaseFolderPath = "";
        IsDatabaseConnected = false;
    }

    public ObservableCollection<ObservablePoint> SelectSpectrumBaseDataToObservableCollectionById(int id)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
            var selectedData = SpectrumTable.FirstOrDefault(s => s.Id == id);
            if (selectedData != null)
            {
                List<double> indexList = SpectrumJsonService
                    .ReadSpectrumBaseDataJson(Path.Join(BaseFolderPath, selectedData.DataFile)).Index;
                List<double> valueReal = SpectrumJsonService
                    .ReadSpectrumBaseDataJson(Path.Join(BaseFolderPath, selectedData.DataFile)).ValueReal;
                if (indexList.Count != valueReal.Count)
                {
                    throw new Exception($"Inconsistent length of horizontal and vertical data with Id: {id}");
                }

                double[,] mergedArray = new double[indexList.Count, 2];
                for (int i = 0; i < indexList.Count; i++)
                {
                    mergedArray[i, 0] = Math.Abs(indexList[i]);
                    mergedArray[i, 1] = Math.Abs(valueReal[i]);
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
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public int GetSpectrumDataNextId(int currentId)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
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
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public int GetSpectrumDataPreviousId(int currentId)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
            var pData = SpectrumTable.Where(s => s.Id < currentId).OrderByDescending(s => s.Id);
            var previousData = pData.FirstOrDefault();
            if (previousData != null)
            {
                return previousData.Id;
            }
            else
            {
                return -2;
            }
        }
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public int GetSpectrumDataNextUnfilledId(int currentId)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
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
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public int GetSpectrumDataPreviousUnfilledId(int currentId)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
            var unfilledData = SpectrumTable.Where(s => s.Id < currentId && s.IsFilled == false)
                .OrderByDescending(s => s.Id);
            var previousUnfilledData = unfilledData.FirstOrDefault();
            if (previousUnfilledData != null)
            {
                return previousUnfilledData.Id;
            }
            else
            {
                return -2;
            }
        }
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public int GetSpectrumDataLastDataId()
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
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
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public int GetSpectrumDataFirstDataId()
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
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
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public int GetSpectrumDataGroupIdById(int id)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
            var selectedData = SpectrumTable.FirstOrDefault(s => s.Id == id);
            if (selectedData != null)
            {
                return selectedData.GroupId;
            }
            else
            {
                throw new Exception($"No data found with Id: {id}");
            }
        }
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public string GetSpectrumDataNameById(int id)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
            var selectedData = SpectrumTable.FirstOrDefault(s => s.Id == id);
            if (selectedData != null)
            {
                return selectedData.Name;
            }
            else
            {
                throw new Exception($"No data found with Id: {id}");
            }
        }
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public int GetSpectrumDataIdByGroupIdAndName(int groupId, string name)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
            var selectedData = SpectrumTable.FirstOrDefault(s => s.GroupId == groupId && s.Name == name);
            if (selectedData != null)
            {
                return selectedData.Id;
            }
            else
            {
                throw new Exception($"No data found with Group Id: {groupId} and Name: {name}");
            }
        }
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public Dictionary<string, string> GetSpectrumDataAppAndTypeById(int id)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
            var selectedData = SpectrumTable.FirstOrDefault(s => s.Id == id);
            if (selectedData != null)
            {
                return new() { { "APP", selectedData.APP }, { "Type", selectedData.Type } };
            }
            else
            {
                throw new Exception($"No data found with Id: {id}");
            }
        }
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public bool GetSpectrumDataIsFilledById(int id)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
            var selectedData = SpectrumTable.FirstOrDefault(s => s.Id == id);
            return selectedData is { IsFilled: true };
        }
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public ObservableCollection<ObservablePoint> SelectSpectrumOmega01DataToObservableCollectionById(int id)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
            var selectedData = SpectrumTable.FirstOrDefault(s => s.Id == id);
            if (selectedData != null)
            {
                List<double> indexList = SpectrumJsonService
                    .ReadSpectrumAdditionalDataJson(Path.Join(BaseFolderPath, selectedData.Omega01AndOmega12File))
                    .Omega01Index;
                List<double> valueReal = SpectrumJsonService
                    .ReadSpectrumAdditionalDataJson(Path.Join(BaseFolderPath, selectedData.Omega01AndOmega12File))
                    .Omega01ValuReal;
                if (indexList.Count != valueReal.Count)
                {
                    throw new Exception($"Inconsistent length of horizontal and vertical Omega01 data with Id: {id}");
                }

                double[,] mergedArray = new double[indexList.Count, 2];
                for (int i = 0; i < indexList.Count; i++)
                {
                    mergedArray[i, 0] = Math.Abs(indexList[i]);
                    mergedArray[i, 1] = Math.Abs(valueReal[i]);
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
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public ObservableCollection<ObservablePoint> SelectSpectrumOmega12DataToObservableCollectionById(int id)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
            var selectedData = SpectrumTable.FirstOrDefault(s => s.Id == id);
            if (selectedData != null)
            {
                List<double> indexList = SpectrumJsonService
                    .ReadSpectrumAdditionalDataJson(Path.Join(BaseFolderPath, selectedData.Omega01AndOmega12File))
                    .Omega12Index;
                List<double> valueReal = SpectrumJsonService
                    .ReadSpectrumAdditionalDataJson(Path.Join(BaseFolderPath, selectedData.Omega01AndOmega12File))
                    .Omega12ValueReal;
                if (indexList.Count != valueReal.Count)
                {
                    throw new Exception($"Inconsistent length of horizontal and vertical Omega01 data with Id: {id}");
                }

                double[,] mergedArray = new double[indexList.Count, 2];
                for (int i = 0; i < indexList.Count; i++)
                {
                    mergedArray[i, 0] = Math.Abs(indexList[i]);
                    mergedArray[i, 1] = Math.Abs(valueReal[i]);
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
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public Dictionary<string, List<double>> SelectSpectrumAdditionalToDictionaryListDoubleById(int id)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
            var selectedData = SpectrumTable.FirstOrDefault(s => s.Id == id);
            if (selectedData != null)
            {
                return new Dictionary<string, List<double>>()
                {
                    {
                        "Omega01Index",
                        JsonService.SpectrumJsonService
                            .ReadSpectrumAdditionalDataJson(Path.Join(BaseFolderPath,
                                selectedData.Omega01AndOmega12File)).Omega01Index
                    },
                    {
                        "Omega01ValueReal",
                        JsonService.SpectrumJsonService
                            .ReadSpectrumAdditionalDataJson(Path.Join(BaseFolderPath,
                                selectedData.Omega01AndOmega12File)).Omega01ValuReal
                    },
                    {
                        "Omega12Index",
                        JsonService.SpectrumJsonService
                            .ReadSpectrumAdditionalDataJson(Path.Join(BaseFolderPath,
                                selectedData.Omega01AndOmega12File)).Omega12Index
                    },
                    {
                        "Omega12ValueReal",
                        JsonService.SpectrumJsonService
                            .ReadSpectrumAdditionalDataJson(Path.Join(BaseFolderPath,
                                selectedData.Omega01AndOmega12File)).Omega12ValueReal
                    },
                };
            }
            else
            {
                throw new Exception($"No data found with Id: {id}");
            }
        }
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public void SaveSpectrumAdditionalDataToJsonAndUpdateSqlById(int id,
        SpectrumAdditionalDataModel spectrumAdditionalData, bool isUseful, bool isBadData)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
            var selectedData = SpectrumTable.FirstOrDefault(s => s.Id == id);
            if (selectedData != null)
            {
                if (isBadData)
                {
                    selectedData.IsWrongData = true;
                    selectedData.IsFilled = true;
                    selectedData.IsUseful = false;
                    SciDB?.SaveChanges();
                }
                else
                {
                    string relativeFileDir = Path.GetDirectoryName(selectedData.DataFile) ??
                                             throw new SqlNullValueException();
                    string additionalDataFilePath =
                        SpectrumJsonService.WriteSpectrumAdditionalDataJson(BaseFolderPath, relativeFileDir,
                            spectrumAdditionalData);
                    selectedData.Omega01AndOmega12File = additionalDataFilePath;
                    selectedData.IsFilled = true;
                    selectedData.IsUseful = isUseful;
                    SciDB?.SaveChanges();
                }
            }
            else
            {
                throw new Exception($"No data found with Id: {id}");
            }
        }
        else
        {
            throw new Exception("Database connection is not established");
        }
    }

    public Dictionary<string, bool> GetSpectrumDataIsUsefulAndIsWrongDataById(int id)
    {
        if (SciDB == null)
        {
            throw new Exception("Database connection is not established");
        }

        if (SpectrumTable != null)
        {
            var selectedData = SpectrumTable.FirstOrDefault(s => s.Id == id);
            if (selectedData != null)
            {
                if (selectedData.IsFilled)
                {
                    return new Dictionary<string, bool>()
                    {
                        { "IsUseful", (bool)selectedData.IsUseful }, { "IsWrongData", selectedData.IsWrongData }
                    };
                }
                else
                {
                    throw new Exception($"Wrongly use function {nameof(GetSpectrumDataIsUsefulAndIsWrongDataById)}.");
                }
            }
            else
            {
                throw new Exception($"No data found with Id: {id}");
            }
        }
        else
        {
            throw new Exception("Database connection is not established");
        }
    }
}