using System.Text.Json;
using Sql.SqlInterface;
using Sql.SqlDataTypes;


namespace InventorySystem.ServerScripts.Server;
public partial class InventoryServer
{
    public void ConfigureWebHostDB(WebApplication app, IWebHostEnvironment env)
    {
        #region Sql DataBase Get
        app.MapGet("/api/inventory", async context =>
        {
            string? location = context.Request.Query["location"].ToString();
            if (string.IsNullOrEmpty(location))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest; // Bad Request
                await context.Response.WriteAsync("Location query parameter is required.");
                return;
            }

            // Get an output of Json data from the sqlite DataBase
            List<SqlInventoryRecord>? inventory = await _sqlController.GetSortedRecords<SqlInventoryRecord>("name");
            if (inventory == null || inventory.Capacity == 0)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound; // Not Found
                await context.Response.WriteAsync($"No inventory found for location: {location}");
                return;
            }

            string json = JsonSerializer.Serialize(inventory, SqlInventoryRecordSerializationOptions.Default.SqlInventoryRecord);

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        });

        app.MapGet("/api/search", async context =>
        {
            string? substring = context.Request.Query["search"].ToString();
            string? type = context.Request.Query["type"].ToString();
            if (string.IsNullOrEmpty(substring) || !StringValidation(substring))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest; // Bad Request
                await context.Response.WriteAsync("Search query parameter is required.");
                return;
            }
            if (string.IsNullOrEmpty(type) || !StringValidation(type))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest; // Bad Request
                await context.Response.WriteAsync("Type query parameter is required.");
                return;
            }

            //Find the Sql Table
            string json;
            if (type == "Item")
            {
                // Get an output of Json data from the Item Table
                List<SqlInventoryItem>? inventory = await _sqlController.GetSortedSubList<SqlInventoryItem>(substring, "name");
                if (inventory == null || inventory.Capacity == 0)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound; // Not Found
                    await context.Response.WriteAsync($"No inventory found for location: {substring}");
                    return;
                }

                json = JsonSerializer.Serialize(inventory, SqlInventoryRecordSerializationOptions.Default.SqlInventoryRecord);
            }
            else if (type == "Warehouse")
            {
                // Get an output of Json data from the Warehouse Table
                List<SqlWarehouse>? inventory = await _sqlController.GetSortedSubList<SqlWarehouse>(substring, "name");
                if (inventory == null || inventory.Capacity == 0)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound; // Not Found
                    await context.Response.WriteAsync($"No inventory found for location: {substring}");
                    return;
                }

                json = JsonSerializer.Serialize(inventory, SqlInventoryRecordSerializationOptions.Default.SqlInventoryRecord);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest; // Bad Request
                await context.Response.WriteAsync("Invalid type query parameter.");
                return;
            }

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        });

        #endregion
        #region Sql DataBase Post
        //login system is to be done after website functionality 
        // app.MapPost("/api/login", async context => 
        // {
        //     string? username = context.Request.Query["username"].ToString();
        //     string? password = context.Request.Query["password"].ToString();
        //     if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        //     {
        //         context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //         await context.Response.WriteAsync("Invalid");
        //         return;
        //     }
        //     //contact the database to see if valid username and password
        //     CookieOptions cookieOptions = new()
        //     {
        //         Expires = DateTime.Now.AddHours(24)
        //     };
        //     context.Response.Cookies.Append("Token", "CookieValue".GetHashCode().ToString(), cookieOptions);
        //     context.Response.StatusCode = StatusCodes.Status200OK;
        //     await context.Response.WriteAsync("Good");

        // });
        //Add new inventory record to database
        app.MapPost("/api/add-record", async context =>
        {
            if (context.Request.ContentType != "application/json")
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            //get inputed data
            string requestBody;
            SqlInventoryRecord record;
            try
            {
                using StreamReader reader = new(context.Request.Body);
                requestBody = await reader.ReadToEndAsync();
                record = JsonSerializer.Deserialize<SqlInventoryRecord>(requestBody, SqlInventoryRecordSerializationOptions.Default.SqlInventoryRecord);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(LogLevel.Error, ex.Message);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }

            SqlInventoryRecordResult result = await _sqlController.InsertInventoryRecord(record);
            switch (result)
            {
                case SqlInventoryRecordResult.Success:
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await context.Response.WriteAsync("Record was succussfully added.");
                    break;
                case SqlInventoryRecordResult.ManyChanges:
                    Logger.Instance.Log(LogLevel.Error, $"an Insert resulted in many changes to the database from the following: {record}");
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Error: Failed to add record");
                    break;
                case SqlInventoryRecordResult.NothingHappend:
                    Logger.Instance.Log(LogLevel.Error, $"an Insert resulted in no changes to the database from the following: {record}");
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Error: Failed to add record");
                    break;
                default:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Error: Failed to add record");
                    break;
            }
        });

        // for adding new item to the list of items allowed in the database
        app.MapPost("/api/add-item", async context =>
        {
            if (context.Request.ContentType != "application/json")
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            //get inputed data
            string requestBody;
            SqlInventoryItem record;
            try
            {
                using StreamReader reader = new(context.Request.Body);
                requestBody = await reader.ReadToEndAsync();
                record = JsonSerializer.Deserialize<SqlInventoryItem>(requestBody, SqlInventoryItemSerializationOptions.Default.SqlInventoryItem);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(LogLevel.Error, ex.Message);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }

            //Add new inventory item
            int amount = await _sqlController.InsertItem(record);
            if (amount == 1)
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync("Item added successfully!");
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Item failed to add!");
            }

        });

        // for adding new storage locations to the database
        app.MapPost("/api/add-warehouse", async context =>
        {
            if (context.Request.ContentType != "application/json")
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            //get inputed data
            string requestBody;
            SqlWarehouse record;
            try
            {
                using StreamReader reader = new(context.Request.Body);
                requestBody = await reader.ReadToEndAsync();
                record = JsonSerializer.Deserialize<SqlWarehouse>(requestBody, SqlWarehouseSerializationOptions.Default.SqlWarehouse);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(LogLevel.Error, ex.Message);
                context.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                return;
            }

            //Add new inventory item
            int amount = await _sqlController.InsertWarehouse(record);
            if (amount == 1)
            {
                context.Response.StatusCode = StatusCodes.Status201Created;
                await context.Response.WriteAsync("Item added successfully!");
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync("Item failed to add!");
            }

        });

        #endregion
        #region Sql DataBase Updates

        #endregion
        #region Sql DataBase Deletions

        #endregion
    }
}