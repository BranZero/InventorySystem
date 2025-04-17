
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
    public string Location { get; set; }
    public string Item { get; set; }
    public string Rarity { get; set; }
    public int Quantity { get; set; } //can't be negative
    public int Price { get; set; } //can't be negative
    public DateTime Date { get; set; }

    public SqlInventoryRecord(string location, string item, string rarity, int quantity, int price)
    {
        Location = location;
        Item = item;
        Rarity = rarity;
        Quantity = quantity;
        Price = price;
    }

    public static string SqlTable => "Inventory_Records";
    public static string SqlColomns => "warehouse_id,item_id,rarity,quantity,price,date";

    public static T FromSql<T>(SqliteDataReader reader) where T : ISqlDataType
    {
        return (T)(ISqlDataType) new SqlInventoryRecord{
            Location = reader.GetString(reader.GetOrdinal("warehouse_id")),
            Item = reader.GetString(reader.GetOrdinal("item_id")),
            Rarity = reader.GetString(reader.GetOrdinal("rarity")),
            Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
            Price = reader.GetInt32(reader.GetOrdinal("price")),
            Date = reader.GetDateTime(reader.GetOrdinal("date")),
        };
    }


    //returns true if it was succussful else it failed to parse
    public static bool IsValidRarity(string rarity)
    {
        return rarity switch
        {
            "Common" or "Uncommon" or "Rare" or "Heroic" or "Epic" or "Legendary" => true,
            _ => false,
        };
    }

    public string ToSql()
    {
        return $"'{Location}','{Item}','{Rarity}','{Quantity}','{Price}',CURRENT_TIMESTAMP";
    }

    public override readonly string ToString()
    {
        return $"[{Location},{Item},{Rarity},{Quantity},{Price}]";
    }
}

public enum SqlInventoryRecordResult
{
    InvalidWarehouse,
    InvalidItem,
    InvalidRarity,
    QuantityAtOrBelowZero,
    PriceBelowZero,
    NothingHappend,
    Success,
    ManyChanges,
}