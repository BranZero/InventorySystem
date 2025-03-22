
using System.Text.Json.Serialization;

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
    public readonly string SqlTable => "Warehouse";

    public string ToSql()
    {
        throw new NotImplementedException();
    }
}