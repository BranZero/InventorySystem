using System;
using Microsoft.Data.Sqlite;
// using ServerHead.Scripts;

public class SqlAdapter : IDisposable
{

    public const string Connection_Path = "Data Source=Inventory.db";
    private static readonly SqlAdapter _instance = new();
    private readonly SqliteConnection _dbConnection;

    private SqlAdapter()
    {
        _dbConnection = new SqliteConnection(Connection_Path);
        _dbConnection.Open();
    }
    public static SqlAdapter Instance => _instance;
    
    public async Task<string> SqlNoQueryResults(string sql)
    {
        await using SqliteCommand? command = _dbConnection.CreateCommand();
        command.CommandText = sql;
        try
        {
            string rows = command.ExecuteNonQuery().ToString();
            return rows;
        }
        catch (Exception e)
        {
            // Logger.Instance.Log(LogLevel.Error, e.Message);
            return e.Message;
        }
    }

    public async Task<SqliteDataReader?> SqlQueryResult(string sql)
    {
        await using SqliteCommand? command = new SqliteCommand(sql, _dbConnection);
        try
        {
            return command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }
        catch (Exception e)
        {
            // Logger.Instance.Log(LogLevel.Error, e.Message);
            return null;
        }
    }

    public async void Dispose()
    {
        await _dbConnection.CloseAsync();
        await _dbConnection.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
