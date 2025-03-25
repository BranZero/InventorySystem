using System.Text.Json;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ServerHead.Scripts;
using Sql.SqlInterface;
using Sql.SqlDataTypes;
// using Microsoft.AspNetCore.Mvc;

public class InventoryServer
{
    private readonly DirectoryInMemory _sourceFiles;
    private readonly DirectoryInMemory _pageFiles;
    private readonly SqlController _sqlController;

    /// <summary>
    /// Creates a new server instance
    /// </summary>
    /// <param name="args"></param>
    public InventoryServer()
    {
        _sourceFiles = new DirectoryInMemory(@"SourceFiles");
        _pageFiles = new DirectoryInMemory(@"Pages");
        _sqlController = new SqlController();

        //Initilize the files needed to be stored in memory before creating the server
        BootSequence();
    }

    public void ConfigureWebHost(WebApplication app, IWebHostEnvironment env)
    {
        app.UseRouting();

        #region WebSite
        //Used to get html pages fro browsing the web page
        app.MapGet("/{filename}", async context =>
        {
            //validate network path
            string? fileName = context.Request.RouteValues["fileName"]?.ToString();

            if (string.IsNullOrWhiteSpace(fileName) || !StringValidation(fileName))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            if (Path.GetExtension(fileName) != string.Empty)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            //get html pages
            string htmlFilePath = fileName + ".html";

            //get page from memory if exists
            ReadOnlyMemory<byte> content = _pageFiles.GetFile(htmlFilePath);
            if (content.Length == 0)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
            context.Response.ContentType = "text/html";
            await context.Response.Body.WriteAsync(content);

        });
        #endregion

        #region Source Pages
        //Retrieve sourceFiles
        app.MapGet("/SourceFiles/{fileName}", async context =>
        {
            //validate network path
            string? fileName = context.Request.RouteValues["fileName"]?.ToString();

            if (string.IsNullOrWhiteSpace(fileName) || !StringValidation(fileName))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            //get content type
            string? contentType = GetContentType(fileName);
            if (contentType == null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            //get file from memory if exists
            context.Response.ContentType = contentType;
            ReadOnlyMemory<byte> content = _sourceFiles.GetFile(fileName);
            if (content.Length == 0)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
            await context.Response.Body.WriteAsync(content);
        });
        #endregion

        #region Sql DataBase Requests
        //post requests
        app.MapPost("/api/login", async context =>
        {
            string? username = context.Request.Query["username"].ToString();
            string? password = context.Request.Query["password"].ToString();
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid");
                return;
            }
            //contact the database to see if valid username and password
            CookieOptions cookieOptions = new()
            {
                Expires = DateTime.Now.AddHours(24)
            };
            context.Response.Cookies.Append("Token", "CookieValue".GetHashCode().ToString(), cookieOptions);
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsync("Good");

        });
        //get requests
        app.MapGet("/api/inventory", async context =>
        {
            string? location = context.Request.Query["location"].ToString();
            if (string.IsNullOrEmpty(location))
            {
                context.Response.StatusCode = 400; // Bad Request
                await context.Response.WriteAsync("Location query parameter is required.");
                return;
            }

            // Get an output of Json data from the sqlite DataBase
            string inventoryAsJson = "{";
            List<ISqlDataType> inventory = await _sqlController.GetSortedRecords(new SqlInventoryRecord(), "name");
            if (inventory.Capacity == 0)
            {
                context.Response.StatusCode = 404; // Not Found
                await context.Response.WriteAsync($"No inventory found for location: {location}");
                return;
            }
            
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(inventoryAsJson);
        });


        //Add new inventory record to database
        app.MapPost("/api/add-inventory-record", async context =>
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
                Logger.Instance.Log(ServerHead.Scripts.LogLevel.Error, ex.Message);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            int result = await _sqlController.InsertInventoryRecord(record);
            switch (result)
            {
                case 1:
                    context.Response.StatusCode = StatusCodes.Status201Created;
                    await context.Response.WriteAsync("Record was succussfully added.");
                    break;
                case 0:
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await context.Response.WriteAsync("Error: Failed to add record");
                    break;
                case -1:
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await context.Response.WriteAsync("Error: Warehouse doesn't exist yet");
                    break;
                case -2:
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await context.Response.WriteAsync("Error: Item doesn't exist in item list yet ");
                    break;
                default:
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await context.Response.WriteAsync("Error: Record failed to add!");
                    Logger.Instance.Log(ServerHead.Scripts.LogLevel.Warning, $"{result} is an unexpected output of add-inventory-record api\n" +
                    $"Sql={record.ToSql}");
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
                Logger.Instance.Log(ServerHead.Scripts.LogLevel.Error, ex.Message);
                context.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                return;
            }

            //Add new inventory item
            int amount = await _sqlController.AddRecord(record);
            if (amount > 0)
            {
                context.Response.StatusCode = StatusCodes.Status201Created;
                await context.Response.WriteAsync("Item added successfully!");
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
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
                Logger.Instance.Log(ServerHead.Scripts.LogLevel.Error, ex.Message);
                context.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                return;
            }

            //Add new inventory item
            int amount = await _sqlController.AddRecord(record);
            if (amount > 0)
            {
                context.Response.StatusCode = StatusCodes.Status201Created;
                await context.Response.WriteAsync("Item added successfully!");
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsync("Item failed to add!");
            }

        });


        #endregion
    }
    private string? GetContentType(string fileName)
    {
        string ext = Path.GetExtension(fileName);
        return ext switch
        {
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".html" => "text/html",
            ".json" => "application/json",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            // ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".wasm" => "application/wasm",
            ".pck" => "application/octet-stream",
            _ => null, // unsupported file type
        };
    }
    private bool StringValidation(string value)
    {
        //contains only letters, numbers and dots
        return value.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-');
    }
    private async void BootSequence()
    {
        Task sourceTask = _sourceFiles.LoadFilesIntoMemory();
        Task pageTask = _pageFiles.LoadFilesIntoMemory();
        Task sqlTask = _sqlController.InitDataBaseConection();

        await sourceTask;
        await pageTask;
        await sqlTask;

#if DEBUG
        _ = Task.Run(async () =>
        { //include in debug only for live changes to website's front end
            while (true)
            {
                await Task.Delay(3000);
                Task sourceTask = _sourceFiles.RefreshFilesInMemory();
                Task pageTask = _pageFiles.RefreshFilesInMemory();

                await sourceTask;
                await pageTask;
            }
        });
#endif
    }


    public static void Main(string[] args)
    {
        Logger.Instance.Log(ServerHead.Scripts.LogLevel.Info, "Starting Server");
        InventoryServer inventoryServer = new InventoryServer();
        var builder = CreateWebHostBuilder(args);
        var app = builder.Build();
        inventoryServer.ConfigureWebHost(app, builder.Environment);
        app.Run();
    }

    public static WebApplicationBuilder CreateWebHostBuilder(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
        builder.WebHost.UseUrls();
        builder.Services.AddAuthentication();
        builder.Services.AddRouting();
        return builder;
    }
}
