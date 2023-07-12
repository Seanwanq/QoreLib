using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QoreLib.Services;

namespace QoreLib.ViewModels.PagesViewModels;

public partial class SQLConfigurationPageViewModel : PageViewModelBase
{
    private readonly IDatabaseService _databaseService;

    public SQLConfigurationPageViewModel(IDatabaseService databaseService)
    {
        IsActive = true;
        _databaseService = databaseService;
    }

    
    [ObservableProperty] private string _databasePath;
    
    // TODO 删掉上面的变量
    
    [ObservableProperty] private string _databaseFolderPath;

    [ObservableProperty] private string _databaseName;

    [ObservableProperty] private string _message;
    
    [ObservableProperty] private string _data;

    [RelayCommand]
    private void Connect()
    {
        try
        {
            _databaseService.ConnectScientificDatabase(DatabaseFolderPath, DatabaseName);
            Message = $"Database {DatabaseName} has been connected.";
        }
        catch (Exception e)
        {
            Message = e.Message;
        }
    }

    [RelayCommand]
    private void ReadData()
    {
        // if (string.IsNullOrWhiteSpace(DatabasePath) || !File.Exists(DatabasePath))
        // {
        //     Data = "Database file does not exist";
        //     return;
        // }
        //
        // try
        // {
        //     using (var connection = new SQLiteConnection($"Data Source={DatabasePath}"))
        //     {
        //         connection.Open();
        //
        //         using (var command = new SQLiteCommand("SELECT * FROM table1", connection))
        //         using (var reader = command.ExecuteReader())
        //         {
        //             var query = from IDataRecord record in reader
        //                 select record.GetString(1);
        //
        //             Data = string.Join(", ", query);
        //         }
        //     }
        // }
        // catch (Exception ex)
        // {
        //     Data = $"Error reading data from database: {ex.Message}";
        // }
    }
    

    [RelayCommand]
    private void CreateAndConnectDatabase()
    {
        try
        {
            _databaseService.CreateAndConnectScientificDatabase(DatabaseFolderPath, DatabaseName);
            Message = $"Database {DatabaseName} has been created and connected.";
        }
        catch (Exception e)
        {
            Message = e.Message;
        }
    }

    [RelayCommand]
    private void DisconnectDatabase()
    {
        try
        {
            _databaseService.CloseScientificDatabaseConnection();
            Message = $"Database {DatabaseName} has been closed.";
        }
        catch (Exception e)
        {
            Message = e.Message;
        }
    }

    [ObservableProperty] private string _testTableName;

    [ObservableProperty] private bool _testTableIsMale = true;

    [ObservableProperty] private int _testTableBoolIndex = 0;

    partial void OnTestTableBoolIndexChanged(int value)
    {
        Message = TestTableBoolIndex.ToString();
        if (TestTableBoolIndex == 0)
        {
            TestTableIsMale = true;
        }
        else if (TestTableBoolIndex == 1)
        {
            TestTableIsMale = false;
        }
    }

    [RelayCommand]
    private void AddDataToTestTable()
    {
        try
        {
            _databaseService.AddDataToTestTable(TestTableName, TestTableIsMale);
            Message = "Data Added.";
            TestTableName = null;
            TestTableBoolIndex = 0;
            TestTableIsMale = true;
        }
        catch(Exception e)
        {
            Message = e.Message;
        }
    }
}