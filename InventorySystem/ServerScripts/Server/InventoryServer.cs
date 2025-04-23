using InventorySystem.ServerScripts;
using Microsoft.AspNetCore.Mvc;
using Sql.SqlDataTypes;
using Sql.SqlInterface;

namespace InventorySystem.Controllers
{
    [Route("")]
    public partial class HomeController : Controller
    {
        private readonly DirectoryInMemory _sourceFiles;
        private readonly DirectoryInMemory _pageFiles;
        private readonly SqlController _sqlController;

        public HomeController()
        {
            _sourceFiles = new DirectoryInMemory(@"wwwroot/source-files");
            _pageFiles = new DirectoryInMemory(@"wwwroot/pages");
            _sqlController = new SqlController("Data Source=Inventory.db");

            //Initilize the files needed to be stored in memory before creating the server
            BootSequence();
        }

        [HttpGet("pages/{fileName}")]
        public IActionResult Pages(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !StringValidation(fileName))
            {
                return BadRequest("");
            }

            if (Path.GetExtension(fileName) != string.Empty)
            {
                return BadRequest();
            }

            string htmlFilePath = fileName + ".html";
            ReadOnlyMemory<byte> content = _pageFiles.GetFile(htmlFilePath);
            if (content.Length == 0)
            {
                return NotFound();
            }

            return File(content.ToArray(), "text/html");
        }

        [HttpGet("source-files/{fileName}")]
        public IActionResult SourceFiles(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !StringValidation(fileName))
            {
                return BadRequest();
            }

            string? contentType = GetContentType(fileName);
            if (contentType == null)
            {
                return BadRequest();
            }

            ReadOnlyMemory<byte> content = _sourceFiles.GetFile(fileName);
            if (content.Length == 0)
            {
                return NotFound();
            }

            return File(content.ToArray(), contentType);
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
                _ => null,
            };
        }

        private static bool StringValidation(string value)
        {
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
}
