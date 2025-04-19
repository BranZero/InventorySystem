using Sql.SqlInterface;
using Sql.SqlDataTypes;
using InventorySystem.ServerScripts;

namespace InventorySystem.ServerScripts.Server;
public partial class InventoryServer
{
    private readonly DirectoryInMemory _sourceFiles;
    private readonly DirectoryInMemory _pageFiles;
    private readonly SqlController _sqlController;

    /// <summary>
    /// Creates a new server instance
    /// </summary>
    public InventoryServer(string sourcePath, string pagePath, string sqlDBConnectionPath)
    {
        _sourceFiles = new DirectoryInMemory(sourcePath);
        _pageFiles = new DirectoryInMemory(pagePath);
        _sqlController = new SqlController(sqlDBConnectionPath);

        //Initilize the files needed to be stored in memory before creating the server
        BootSequence();
    }

    private static string? GetContentType(string fileName)
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
            // ".svg" => "image/svg+xml",
            // ".wasm" => "application/wasm",
            // ".pck" => "application/octet-stream",
            _ => null, // unsupported file type
        };
    }

    private static bool StringValidation(string value)
    {
        //contains only letters, numbers and dots
        return value.All(c => char.IsLetterOrDigit(c) || c == ' ' || c == '.' || c == '-');
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
}