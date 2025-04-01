
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Data.Sqlite;
using Sql.SqlDataTypes;

namespace Sql.SqlInterface;
public class SqlInMemory<T> where T : ISqlDataType
{
    private SqlController _sqlController;
    public Dictionary<string, int> _data;
    private Mutex _mutex;
    public SqlInMemory(SqlController sqlController)
    {
        _sqlController = sqlController;
        _data = new Dictionary<string, int>();
        _mutex = new Mutex();
    }
    public async Task Init(string tableName)
    {
        _mutex.WaitOne();
        _data = new Dictionary<string, int>();
        var records = await _sqlController.GetSortedRecords<T>("name");
        if (records == null || records.Count == 0)
        {
            _mutex.ReleaseMutex();
            return;
        }
        //update memory
        foreach (var item in records)
        {
            if (item is SqlWarehouse)
            {
                SqlWarehouse tmp = (SqlWarehouse)(ISqlDataType)item;
                _data.TryAdd(tmp.Name, tmp.Id);//this won't be null in this case
            }
            else if (item is SqlInventoryItem)
            {
                SqlInventoryItem tmp = (SqlInventoryItem)(ISqlDataType)item;
                _data.TryAdd(tmp.Name, tmp.Id);//this won't be null in this case
            }
            else
            {
                throw new UnsupportedContentTypeException($"{item}");
            }
        }

        _mutex.ReleaseMutex();
    }
    public bool Contains(string name, out int id)
    {
        _mutex.WaitOne();
        //prevent form entering when rebuilding "_data"
        _mutex.ReleaseMutex();
        if (_data.ContainsKey(name))
        {
            id = _data[name];
            return true;
        }
        else
        {
            id = -1;
            return false;
        }
    }
}