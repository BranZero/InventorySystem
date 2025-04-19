using Sql.SqlDataTypes;

namespace Sql.SqlInterface;
public class SqlCreateTable
{
    //Create the default tables for the database
    public static async Task InitialDataBaseTables(SqlAdapter sqlAdapter)
    {
        string result;
        string sqlLine;
        //Users table
        // sqlLine = 
        //     "Create Table IF NOT EXISTS users(" +
        //     "id INTEGER PRIMARY KEY AUTOINCREMENT," +
        //     "name VARCHAR(20) NOT NULL," +
        //     "password VARCHAR(48) NOT NULL," +
        //     "token VARCHAR(48)," +
        //     "tokenexpires Date);";
        // result = await SqlAdapter.Instance.SqlNoQueryResults(sqlLine);
        // Console.WriteLine(result);
        //Token table connected to user table



        //Warehouse table
        sqlLine = $"CREATE TABLE IF NOT EXISTS {SqlWarehouse.SqlTable} (" +
            "name TEXT NOT NULL PRIMARY KEY);";
        result = await sqlAdapter.SqlNoQueryResults(sqlLine);
        Console.WriteLine(result);

        //Inventory Item Table
        sqlLine = $"CREATE TABLE IF NOT EXISTS {SqlInventoryItem.SqlTable} (" +
            "name TEXT NOT NULL PRIMARY KEY," +
            "type TEXT NOT NULL," +
            "desc TEXT NOT NULL);";
        result = await sqlAdapter.SqlNoQueryResults(sqlLine);
        Console.WriteLine(result);

        //Inventory Table
        sqlLine = $"CREATE TABLE IF NOT EXISTS {SqlInventoryRecord.SqlTable} (" +
            "id INTEGER PRIMARY KEY AUTOINCREMENT," +
            "warehouse_id TEXT," + 
            "item_id TEXT," +
            "rarity TEXT," +
            "quantity INTEGER," +
            "price INTEGER," +
            "date DATE," +
            "FOREIGN KEY (warehouse_id) REFERENCES Warehouse(name)," +
            "FOREIGN KEY (item_id) REFERENCES Item(name));";
        result = await sqlAdapter.SqlNoQueryResults(sqlLine);
        Console.WriteLine(result);
    }
    public static async Task ResetTables(SqlAdapter sqlAdapter)
    {
        string sql = "DROP TABLE IF EXISTS Inventory_Records;" +
            "DROP TABLE IF EXISTS Warehouse;" +
            "DROP TABLE IF EXISTS Item;";
        string result = await sqlAdapter.SqlNoQueryResults(sql);
    }
}