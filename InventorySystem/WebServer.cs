using InventorySystem.ServerScripts;
using InventorySystem.ServerScripts.Server;
// using Microsoft.AspNetCore.Mvc;

public class Program
{
    public static void Main(string[] args)
    {
        Logger.Instance.Log(LogLevel.Information, "Starting Server");
        InventoryServer inventoryServer = new InventoryServer(@"SourceFiles",@"Pages","Data Source=Inventory.db");
        var builder = CreateWebHostBuilder(args);
        var app = builder.Build();
        inventoryServer.ConfigureWebHost(app, builder.Environment);
        inventoryServer.ConfigureWebHostDB(app, builder.Environment);
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
