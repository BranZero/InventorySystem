using InventorySystem.ServerScripts;
using Microsoft.AspNetCore.Mvc;
using Sql.SqlInterface;

namespace InventorySystem.Controllers
{
    public partial class HomeController : Controller
    {
        private readonly DirectoryInMemory _sourceFiles;
        private readonly DirectoryInMemory _pageFiles;
        private readonly SqlController _sqlController;

        public HomeController(DirectoryInMemory sourceFiles, DirectoryInMemory pageFiles, SqlController sqlController)
        {
            _sourceFiles = sourceFiles;
            _pageFiles = pageFiles;
            _sqlController = sqlController;
        }

        public IActionResult Index(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !StringValidation(fileName))
            {
                return BadRequest();
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
    }
}
