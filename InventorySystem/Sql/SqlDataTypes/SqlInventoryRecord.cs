
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;

namespace Sql.SqlDataTypes;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(SqlInventoryRecord))]
[JsonSerializable(typeof(string))]
internal partial class SqlInventoryRecordSerializationOptions : JsonSerializerContext
{
}

public struct SqlInventoryRecord : ISqlDataType
{
    public string Location;
    public string Name;
    public string Rarity;
    public int Quantity;
    public int Price;

    private SqlInventoryRecord(string location, string name, string rarity, int quantity, int price)
    {
        Location = location;
        Name = name;
        Rarity = rarity;
        Quantity = quantity;
        Price = price;
    }

    public string SqlTable => "Inventory_Records";


    //returns true if it was succussful else it failed to parse
    public static bool IsValidRarity(string rarity)
    {
        return rarity switch
        {
            "common" or "uncommon" or "rare" or "heroic" or "epic" or "legendary" => true,
            _ => false,
        };
    }

    public ISqlDataType FromSql(SqliteDataReader reader)
    {
        return new SqlInventoryRecord{
            Location = reader.GetString(reader.GetOrdinal("location")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Rarity = reader.GetString(reader.GetOrdinal("name")),
            Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
            Price = reader.GetInt32(reader.GetOrdinal("price")),
        };
    }

    public string ToSql()
    {
        return $"{Location},{Name},{Rarity},{Quantity},{Price},Date.Now()";
    }

    public override readonly string ToString()
    {
        return $"[{Location},{Name},{Rarity},{Quantity},{Price}]";
    }
}