
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
    public string Name { get; set; }

    public SqlWarehouse(string name){
        Name = name;
    }

    public static string SqlTable => "Warehouse";

    public static string SqlColomns => "name";

    public static T FromSql<T>(SqliteDataReader reader) where T : ISqlDataType
    {
        return (T)(ISqlDataType) new SqlWarehouse{
            Name = reader.GetString(reader.GetOrdinal("name")),
        };
    }

    public string ToSql()
    {
        return $"'{Name}'";
    }
}