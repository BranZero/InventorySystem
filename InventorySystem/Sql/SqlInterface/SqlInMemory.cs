
using Microsoft.Data.Sqlite;

namespace Sql.SqlInterface;
public class SqlInMemory
{
    public HashSet<string> _data;
    private bool _ready;
    public bool Ready => _ready;
    public SqlInMemory(){
        _data = new HashSet<string>();
        _ready = false;
    }
    public async Task Init(string tableName){
        _ready = false;
        _data = new HashSet<string>();
        string sqlCommand = $"SELECT * FROM {tableName}";
        var reader = await SqlAdapter.Instance.SqlQueryResult(sqlCommand);
        if (reader == null){
            return;
        }

        while (await reader.ReadAsync())
        {
            string name = reader.GetString(reader.GetOrdinal("name"));
            _data.Add(name);
        }
        _ready = true;
    }
    public bool Contains(string name){
        if(!_ready){
            throw new Exception("Data of list isn't ready yet");
        }
        return _data.Contains(name);
    }
}