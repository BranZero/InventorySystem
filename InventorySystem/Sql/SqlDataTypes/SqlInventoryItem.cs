
using System.Text.Json.Serialization;

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

    public readonly string SqlTable => "Item";

    public readonly string ToSql()
    {
        return $"{Name},{Type},{Description}";
    }
}
