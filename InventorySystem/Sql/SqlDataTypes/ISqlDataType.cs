using System.Text.Json.Serialization;

namespace Sql.SqlDataTypes;
public interface ISqlDataType
{
    public abstract string SqlTable { get; }

    public abstract string ToSql();
}

