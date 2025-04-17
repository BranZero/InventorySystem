
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;

namespace Sql.SqlDataTypes;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(SqlInventoryItem))]
[JsonSerializable(typeof(string))]
internal partial class SqlInventoryItemSerializationOptions : JsonSerializerContext
{
}
public struct SqlInventoryItem : ISqlDataType
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }

    public static string SqlTable => "Item";
    public static string SqlColomns => "name,type,desc";

    public static T FromSql<T>(SqliteDataReader reader) where T : ISqlDataType
    {
        return (T)(ISqlDataType)new SqlInventoryItem{
            Name = reader.GetString(reader.GetOrdinal("name")),
            Type = reader.GetString(reader.GetOrdinal("type")),
            Description = reader.GetString(reader.GetOrdinal("desc"))
        };
    }

    public readonly string ToSql()
    {
        return $"'{Name}','{Type}','{Description}'";
    }
}
