
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
    public string Name;
    public string Type;
    public string Description;

    readonly string ISqlDataType.SqlTable => "Item";

    public ISqlDataType FromSql(SqliteDataReader reader)
    {
        return new SqlInventoryItem{
            Name = reader.GetString(reader.GetOrdinal("name")),
            Type = reader.GetString(reader.GetOrdinal("type")),
            Description = reader.GetString(reader.GetOrdinal("desc"))
        };
    }

    public readonly string ToSql()
    {
        return $"{Name},{Type},{Description}";
    }
}
