using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;

namespace Sql.SqlDataTypes;
public interface ISqlDataType
{
    public abstract string SqlTable { get; }

    public abstract string ToSql();

    public abstract ISqlDataType FromSql(SqliteDataReader reader);
}

