
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;

namespace Sql.SqlDataTypes;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(SqlWarehouse))]
[JsonSerializable(typeof(string))]
internal partial class SqlWarehouseSerializationOptions : JsonSerializerContext
{
}
public struct SqlWarehouse : ISqlDataType
{
    public string Name;

    public SqlWarehouse(string name){
        Name = name;
    }

    readonly string ISqlDataType.SqlTable => "Warehouse";

    public ISqlDataType FromSql(SqliteDataReader reader)
    {
        return new SqlWarehouse{
            Name = reader.GetString(reader.GetOrdinal("name")),
        };
    }

    public string ToSql()
    {
        return $"{Name}";
    }
}