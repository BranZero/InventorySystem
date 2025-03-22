using Microsoft.Data.Sqlite;
using Sql.SqlDataTypes;

namespace Sql.SqlInterface;

public class SqlController
{
    public SqlController()
    {
        SqlCreateTable.InitialDataBaseTables();
    }
#region Insert
    private int UpdateInventory()
    {
        return 0;
    }
    public int NewRecord(string name, ISqlDataType sqlData)
    {
        string SqlCommand = $"Insert Into {sqlData.SqlTable} Values({sqlData.ToSql()})";
        return 0;
    }
#endregion

#region Requests

#endregion

}