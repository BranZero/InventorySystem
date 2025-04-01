
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
    public int Id;
    public string Name;

    public SqlWarehouse(string name){
        Name = name;
    }

    public static string SqlTable => "Warehouse";

    public static string SqlColomns => "name";

    public static T FromSql<T>(SqliteDataReader reader) where T : ISqlDataType
    {
        return (T)(ISqlDataType) new SqlWarehouse{
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
        };
    }

    public string ToSql()
    {
        return $"'{Name}'";
    }
}