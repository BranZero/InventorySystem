using System;
using Microsoft.Data.Sqlite;
using InventorySystem.ServerScripts;
using Sql.SqlDataTypes;
// using ServerHead.Scripts;

public class SqlAdapter
{
    private SqliteConnection _dbConnection;

    public SqlAdapter(string connectionPath)
    {
        _dbConnection = new SqliteConnection(connectionPath);
        _dbConnection.Open();
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
            Logger.Instance.Log(LogLevel.Error, e.Message);
            return e.Message;
        }
    }

    public async Task<List<T>> SqlQueryResult<T>(string sql) where T : ISqlDataType
    {
        await using SqliteCommand? command = new SqliteCommand(sql, _dbConnection);
        var records = new List<T>();
        try
        {
            SqliteDataReader reader = await command.ExecuteReaderAsync();
            if (reader != null)
            {
                while (await reader.ReadAsync())
                {
                    
                    T record = T.FromSql<T>(reader);
                    records.Add(record);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Instance.Log(LogLevel.Error, e.Message);
            return new List<T>();//return empty
        }
        return records;
    }

    public void Dispose()
    {
        _dbConnection.Close();
        _dbConnection.Dispose();
        SqliteConnection.ClearAllPools();
    }
}
