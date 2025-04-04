using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;

namespace Sql.SqlDataTypes;
public interface ISqlDataType
{
    public abstract static string SqlTable {get; }
    public abstract static string SqlColomns {get; }

    public abstract string ToSql();

    public abstract static T FromSql<T>(SqliteDataReader reader) where T : ISqlDataType;
}

