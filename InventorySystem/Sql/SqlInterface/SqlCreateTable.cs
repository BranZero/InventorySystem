namespace Sql.SqlInterface;
public class SqlCreateTable
{
    //Create the default tables for the database
    public static async Task InitialDataBaseTables()
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
        sqlLine = "CREATE TABLE IF NOT EXISTS Warehouse (" +
            "id INTEGER PRIMARY KEY AUTOINCREMENT," +
            "name TEXT NOT NULL);";
        result = await SqlAdapter.Instance.SqlNoQueryResults(sqlLine);
        Console.WriteLine(result);

        //Inventory Table
        sqlLine = "CREATE TABLE IF NOT EXISTS Inventory_Records (" +
            "id INTEGER PRIMARY KEY AUTOINCREMENT," +
            "warehouse_id INTEGER," +
            "item_id INTEGER," +
            "quantity INTEGER," +
            "FOREIGN KEY (warehouse_id) REFERENCES Warehouse(id));";
        result = await SqlAdapter.Instance.SqlNoQueryResults(sqlLine);
        Console.WriteLine(result);

        //Inventory Item Table
        sqlLine = "CREATE TABLE IF NOT EXISTS Item (" +
            "id INTEGER PRIMARY KEY AUTOINCREMENT," +
            "name TEXT NOT NULL," +
            "type TEXT NOT NULL," +
            "desc TEXT NOT NULL);";
        result = await SqlAdapter.Instance.SqlNoQueryResults(sqlLine);
        Console.WriteLine(result);
    }
}