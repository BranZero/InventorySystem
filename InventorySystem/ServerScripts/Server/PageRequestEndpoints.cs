

namespace InventorySystem.ServerScripts.Server;
public partial class InventoryServer{
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
    }
}