using System.Threading.Tasks;
using NUnit.Framework;
using Sql.SqlDataTypes;
using Sql.SqlInterface;

namespace SqlTests;

public class SqlTests
{
    private SqlController _sqlController;
    private SqlAdapter _sqlAdpter;
    [SetUp]
    public async Task SetUp()
    {
        _sqlController = new();
        _sqlAdpter = SqlAdapter.Instance;
        await ResetTables();

        await _sqlController.InitDataBaseConection();
        await BuildTestDataBase();
    }

    [TearDown]
    public async Task TearDown()
    {
        await ResetTables();
        await SqlAdapter.Instance.Dispose();
        try
        {
            File.Delete(@".\Inventory.db");
        }
        catch (System.Exception)
        {

        }
    }
    
    private async Task ResetTables()
    {
        string sql = "DROP TABLE IF EXISTS Warehouse;" +
            "DROP TABLE IF EXISTS Inventory_Records;" +
            "DROP TABLE IF EXISTS Item;";
        await SqlAdapter.Instance.SqlNoQueryResults(sql);
    }
    private async Task BuildTestDataBase()
    {
        //Add Warehouse locations
        string[] locations = ["West", "North", "East", "South"];
        foreach (var location in locations)
        {
            SqlWarehouse sqlWarehouse = new(location);
            await _sqlController.InsertWarehouse(sqlWarehouse);
        }

        //Add Items 
        string[] items = ["Milk", "Boots", "Stone", "Stick", "Iron Ore", "Bronze"];
        string[] types = ["Food", "Armor", "Material", "Material", "Material", "Material"];
        for (int i = 0; i < items.Length; i++)
        {
            
            SqlInventoryItem sqlInventoryItem = new()
            {
                Name = items[i],
                Type = types[i],
                Description = "n/a"
            };
            await _sqlController.InsertItem(sqlInventoryItem);
        }

        //Inventory Records
        SqlInventoryRecord sqlInventoryRecord = new(){
            Location = "West",
            Item = "Bronze",
            Rarity = "Common",
            Quantity = 5,
            Price = 20
        };
        await _sqlController.InsertInventoryRecord(sqlInventoryRecord);

        sqlInventoryRecord = new(){
            Location = "West",
            Item = "Bronze",
            Rarity = "Common",
            Quantity = 5,
            Price = 20
        };
        await _sqlController.InsertInventoryRecord(sqlInventoryRecord);

        sqlInventoryRecord = new(){
            Location = "North",
            Item = "Boots",
            Rarity = "Uncommon",
            Quantity = 1,
            Price = 24
        };
        await _sqlController.InsertInventoryRecord(sqlInventoryRecord);

        sqlInventoryRecord = new(){
            Location = "West",
            Item = "Bronze",
            Rarity = "Common",
            Quantity = 2,
            Price = 10
        };
        await _sqlController.InsertInventoryRecord(sqlInventoryRecord);

        sqlInventoryRecord = new(){
            Location = "West",
            Item = "Iron Ore",
            Rarity = "Rare",
            Quantity = 16,
            Price = 44
        };
        await _sqlController.InsertInventoryRecord(sqlInventoryRecord);
    }
    
    [TestCase("Clock", "Misc")]
    [TestCase("Tin", "Material")]
    [TestCase("Staff", "Ranged")]
    public async Task AddItem_New(string name, string type){
        SqlInventoryItem sqlInventoryItem = new(){
            Name = name,
            Type = type,
            Description = "N/A"
        };
        int result = await _sqlController.InsertItem(sqlInventoryItem);
        Assert.That(result, Is.EqualTo(1),$"Name={name}, Type={type}");
    }

    [TestCase("Bronze", "Material")]
    [TestCase("Boots", "Armor")]
    public async Task AddItem_Existing(string name, string type){
        SqlInventoryItem sqlInventoryItem = new(){
            Name = name,
            Type = type,
            Description = "N/A"
        };
        int result = await _sqlController.InsertItem(sqlInventoryItem);
        Assert.That(result, Is.EqualTo(0));
    }
}