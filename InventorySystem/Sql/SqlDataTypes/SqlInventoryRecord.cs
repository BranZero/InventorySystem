
using System.Text.Json.Serialization;

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
    public readonly string Location;
    public readonly string Name;
    public readonly string Rarity;
    public readonly int Quantity;
    public readonly int Price;

    private SqlInventoryRecord(string location, string name, string rarity, int quantity, int price)
    {
        Location = location;
        Name = name;
        Rarity = rarity;
        Quantity = quantity;
        Price = price;
    }

    public readonly string SqlTable => "Inventory_Records";

    //returns true if it was succussful else it failed to parse
    public static bool TryParseData(string dataString, out SqlInventoryRecord record)
    {
        string[] dataArray = dataString.Split(",");
        if(dataArray.Length != 5)
        {
            record = new();
            return false;
        }
        else
        {
            if(!IsValidRarity(dataArray[2]) || 
                !Int32.TryParse(dataArray[3], out int quantity) || 
                !Int32.TryParse(dataArray[4], out int price))
            {
                record = new();
                return false;
            }


            record = new(
                dataArray[0],
                dataArray[1],
                dataArray[2],
                quantity,
                price
            );
            return true;
        }
    }
    private static bool IsValidRarity(string rarity)
    {
        return rarity switch
        {
            "common" or "uncommon" or "rare" or "heroic" or "epic" or "legendary" => true,
            _ => false,
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