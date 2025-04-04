
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
    public string Item;
    public string Rarity;
    public int Quantity;
    public int Price;

    public SqlInventoryRecord(string location, string item, string rarity, int quantity, int price)
    {
        Location = location;
        Item = item;
        Rarity = rarity;
        Quantity = quantity;
        Price = price;
    }

    public static string SqlTable => "Inventory_Records";
    public static string SqlColomns => "";

    public static T FromSql<T>(SqliteDataReader reader) where T : ISqlDataType
    {
        return (T)(ISqlDataType) new SqlInventoryRecord{
            Location = reader.GetString(reader.GetOrdinal("location")),
            Item = reader.GetString(reader.GetOrdinal("item_id")),
            Rarity = reader.GetString(reader.GetOrdinal("rarity")),
            Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
            Price = reader.GetInt32(reader.GetOrdinal("price")),
        };
    }


    //returns true if it was succussful else it failed to parse
    public static bool IsValidRarity(string rarity)
    {
        return rarity switch
        {
            "common" or "uncommon" or "rare" or "heroic" or "epic" or "legendary" => true,
            _ => false,
        };
    }

    public string ToSql()
    {
        return $"'{Location}','{Item}','{Rarity}','{Quantity}','{Price}',Date.Now()";
    }

    public override readonly string ToString()
    {
        return $"[{Location},{Item},{Rarity},{Quantity},{Price}]";
    }
}