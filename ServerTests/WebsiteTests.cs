using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using InventorySystem.ServerScripts.Server;

namespace WebsiteTests;

public class EndPointWebsiteTests
{
    private WebApplicationFactory<InventoryServer> _factory;
    private HttpClient _client;

    [SetUp]
    public void SetUp()
    {
        File.Delete("@Inventory.db");
        _factory = new WebApplicationFactory<InventoryServer>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
        File.Delete("@Inventory.db");
    }

    [Test]
    public async Task GetWebsitePage_ValidFileName_ReturnsHtmlContent()
    {
        // Setup
        string fileName = "index";
        string expectedContent = File.ReadAllText(@"Pages/index.html");

        // Act
        var response = await _client.GetAsync($"/{fileName}");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        if (response.Content.Headers.ContentType != null)
        {
            Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo("text/html"));
        }
        else
        {
            Assert.Fail("response.Content.Headers.ContentType is null");
        }
        Assert.That(content, Is.EqualTo(expectedContent));
    }

    [Test]
    public async Task GetWebsitePage_InvalidPage()
    {
        // Setup
        string fileName = "@inval$id\file.name";

        // Act
        var response = await _client.GetAsync($"/{fileName}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetWebsitePage_NotFound()
    {
        // Setup
        string fileName = "nonexistentPage";

        // Act
        var response = await _client.GetAsync($"/{fileName}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetWebsitePage_PageWithExtension()
    {
        // Setup
        string fileName = "page.html";

        // Act
        var response = await _client.GetAsync($"/{fileName}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    [Test]
    public async Task GetSourceFile_FileWithExtension()
    {
        // Setup
        string fileName = "inventory-add-item.js";
        string expectedContent = File.ReadAllText($@"SourceFiles/{fileName}");

        // Act
        var response = await _client.GetAsync($"/SourceFiles/{fileName}");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        if (response.Content.Headers.ContentType != null)
        {
            Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo("application/javascript"));
        }
        else
        {
            Assert.Fail("response.Content.Headers.ContentType is null");
        }
        Assert.That(content, Is.EqualTo(expectedContent));
    }
    [Test]
    public async Task GetSourceFile_StringContainsNullCharacters()
    {
        // Setup
        string fileName = "h.cmd001ÎÊø";

        // Act
        var response = await _client.GetAsync($"/SourceFiles/{fileName}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}