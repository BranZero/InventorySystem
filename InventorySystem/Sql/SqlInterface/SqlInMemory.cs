

using Sql.SqlDataTypes;

namespace Sql.SqlInterface;
public class SqlInMemory<T> where T : ISqlDataType
{
    private SqlController _sqlController;
    public HashSet<string> _data;
    private Mutex _mutex;
    public SqlInMemory(SqlController sqlController)
    {
        _sqlController = sqlController;
        _data = new HashSet<string>();
        _mutex = new Mutex();
    }
    public async Task Init()
    {

        var records = await _sqlController.GetSortedRecords<T>("name");
        if (records == null || records.Count == 0)
        {
            return;
        }

        //update memory
        _mutex.WaitOne();
        _data.Clear();
        foreach (T record in records)
        {
            if (record is SqlWarehouse)
            {
                SqlWarehouse tmp = (SqlWarehouse)(ISqlDataType)record;
                _data.Add(tmp.Name);//this won't be null in this case
            }
            else if (record is SqlInventoryItem)
            {
                SqlInventoryItem tmp = (SqlInventoryItem)(ISqlDataType)record;
                _data.Add(tmp.Name);//this won't be null in this case
            }
            else
            {
                throw new Exception($"Unsupported In Memory ISqlDataType: {record}");
            }
        }

        _mutex.ReleaseMutex();
    }
    public bool Contains(string name)
    {
        _mutex.WaitOne();
        //prevent from entering when rebuilding "_data"
        bool result = _data.Contains(name);
        _mutex.ReleaseMutex();
        return result;
    }
}