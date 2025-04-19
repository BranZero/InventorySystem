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
        _sqlController = new("Data Source=InventoryTest.db");
        _sqlAdpter = _sqlController.GetAdpter();
        await SqlCreateTable.ResetTables(_sqlAdpter);

        await _sqlController.InitDataBaseConection();
        await BuildTestDataBase();
    }

    [TearDown]
    public async Task TearDown()
    {
        await SqlCreateTable.ResetTables(_sqlAdpter);
        _sqlAdpter.Dispose();
        try
        {
            File.Delete(@".\Inventory.db");
        }
        catch (System.Exception)
        {
            //most likely a used by another process
        }
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
                Description = "NA"
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
            Description = "NA"
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
            Description = "NA"
        };
        int result = await _sqlController.InsertItem(sqlInventoryItem);
        Assert.That(result, Is.EqualTo(0));
    }


    [TestCase("Winter")]
    [TestCase("Summer")]
    public async Task AddWarehouse_New(string name){
        SqlWarehouse sqlWarehouse = new(){
            Name = name,
        };
        int result = await _sqlController.InsertWarehouse(sqlWarehouse);
        Assert.That(result, Is.EqualTo(1));
    }

    [TestCase("West")]
    [TestCase("South")]
    public async Task AddWarehouse_Existing(string name){
        SqlWarehouse sqlWarehouse = new(){
            Name = name,
        };
        int result = await _sqlController.InsertWarehouse(sqlWarehouse);
        Assert.That(result, Is.EqualTo(0));
    }

    [TestCase("West","Bronze","Rare", 10,99)]
    [TestCase("East","Boots","Epic", 1,88)]
    public async Task AddInventoryRecords_New(string location, string item, string rarity, int quantity, int price){
        SqlInventoryRecord sqlInventoryRecord = new(){
            Location = location,
            Item = item,
            Rarity = rarity,
            Quantity = quantity,
            Price = price,
        };
        SqlInventoryRecordResult result = await _sqlController.InsertInventoryRecord(sqlInventoryRecord);
        Assert.That(result, Is.EqualTo(SqlInventoryRecordResult.Success));
    }

    [TestCase("Wes","Bronze","Rare", 10,99)]
    [TestCase("Maxwell","Boots","Epic", 1,88)]
    public async Task AddInventoryRecords_InvalidWarehouse(string location, string item, string rarity, int quantity, int price){
        SqlInventoryRecord sqlInventoryRecord = new(){
            Location = location,
            Item = item,
            Rarity = rarity,
            Quantity = quantity,
            Price = price,
        };
        SqlInventoryRecordResult result = await _sqlController.InsertInventoryRecord(sqlInventoryRecord);
        Assert.That(result, Is.EqualTo(SqlInventoryRecordResult.InvalidWarehouse));
    }

    [TestCase("West","Candy","Rare", 10,99)]
    [TestCase("East","Root","Epic", 1,88)]
    public async Task AddInventoryRecords_InvalidItem(string location, string item, string rarity, int quantity, int price){
        SqlInventoryRecord sqlInventoryRecord = new(){
            Location = location,
            Item = item,
            Rarity = rarity,
            Quantity = quantity,
            Price = price,
        };
        SqlInventoryRecordResult result = await _sqlController.InsertInventoryRecord(sqlInventoryRecord);
        Assert.That(result, Is.EqualTo(SqlInventoryRecordResult.InvalidItem));
    }

    [TestCase("West","Bronze","None", 10,99)]
    [TestCase("East","Boots","Epi", 1,88)]
    public async Task AddInventoryRecords_InvalidRarity(string location, string item, string rarity, int quantity, int price){
        SqlInventoryRecord sqlInventoryRecord = new(){
            Location = location,
            Item = item,
            Rarity = rarity,
            Quantity = quantity,
            Price = price,
        };
        SqlInventoryRecordResult result = await _sqlController.InsertInventoryRecord(sqlInventoryRecord);
        Assert.That(result, Is.EqualTo(SqlInventoryRecordResult.InvalidRarity));
    }

    [TestCase("West","Bronze","Rare", -1,99)]
    [TestCase("East","Boots","Epic", 0,88)]
    public async Task AddInventoryRecords_InvalidQuantity(string location, string item, string rarity, int quantity, int price){
        SqlInventoryRecord sqlInventoryRecord = new(){
            Location = location,
            Item = item,
            Rarity = rarity,
            Quantity = quantity,
            Price = price,
        };
        SqlInventoryRecordResult result = await _sqlController.InsertInventoryRecord(sqlInventoryRecord);
        Assert.That(result, Is.EqualTo(SqlInventoryRecordResult.QuantityAtOrBelowZero));
    }

    [TestCase("West","Bronze","Rare", 10,-22229)]
    [TestCase("East","Boots","Epic", 1,-1)]
    public async Task AddInventoryRecords_InvalidPrice(string location, string item, string rarity, int quantity, int price){
        SqlInventoryRecord sqlInventoryRecord = new(){
            Location = location,
            Item = item,
            Rarity = rarity,
            Quantity = quantity,
            Price = price,
        };
        SqlInventoryRecordResult result = await _sqlController.InsertInventoryRecord(sqlInventoryRecord);
        Assert.That(result, Is.EqualTo(SqlInventoryRecordResult.PriceBelowZero));
    }

    [TestCase("sss');" + $"INSERT INTO Item (name,type,desc) VALUES ('was','a','success")]
    public async Task AddWarehouse_SqlInjection(string location)
    {
        SqlWarehouse sqlWarehouse = new(location);
        int result = await _sqlController.InsertWarehouse(sqlWarehouse);
        Assert.That(result, Is.EqualTo(0));
    }
}