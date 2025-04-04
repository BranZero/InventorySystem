using System;
using Microsoft.Data.Sqlite;
using Sql.SqlDataTypes;
// using ServerHead.Scripts;

public class SqlAdapter
{

    public const string Connection_Path = "Data Source=Inventory.db";
    private static SqlAdapter _instance;
    private static SqliteConnection _dbConnection;

    static SqlAdapter()
    {
        _dbConnection = new SqliteConnection(Connection_Path);
        _instance = new SqlAdapter();
    }
    private SqlAdapter()
    {
        _dbConnection.OpenAsync();
    }
    public static SqlAdapter Instance
    {
        get
        {
            if (_dbConnection.State == System.Data.ConnectionState.Closed)
            {
                _dbConnection.OpenAsync();
            }
            return _instance;
        }
    }

    public async Task<string> SqlNoQueryResults(string sql)
    {

        await using SqliteCommand? command = _dbConnection.CreateCommand();
        command.CommandText = sql;
        try
        {
            int rows = await command.ExecuteNonQueryAsync();
            return rows.ToString();
        }
        catch (Exception e)
        {
            // Logger.Instance.Log(LogLevel.Error, e.Message);
            return e.Message;
        }
    }

    public async Task<List<T>?> SqlQueryResult<T>(string sql) where T : ISqlDataType
    {
        await using SqliteCommand? command = new SqliteCommand(sql, _dbConnection);
        try
        {
            SqliteDataReader reader = await command.ExecuteReaderAsync();
            var records = new List<T>();

            if (reader != null)
            {
                while (await reader.ReadAsync())
                {
                    
                    T record = T.FromSql<T>(reader);
                    records.Add(record);
                }
            }
            return records;
        }
        catch (Exception e)
        {
            // Logger.Instance.Log(LogLevel.Error, e.Message);
            return null;
        }
    }

    public async Task Dispose()
    {
        await _dbConnection.CloseAsync();
        await _dbConnection.DisposeAsync();
    }
}
