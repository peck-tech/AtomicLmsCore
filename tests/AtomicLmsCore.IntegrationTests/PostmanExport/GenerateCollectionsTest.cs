using Xunit;

namespace AtomicLmsCore.IntegrationTests.PostmanExport;

public class GenerateCollectionsTest
{
    [Fact(Skip = "Run manually to generate Postman collections")]
    public async Task GeneratePostmanCollections()
    {
        var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "PostmanCollections");
        await AtomicLmsPostmanExporter.ExportAllCollections(outputDir);
        
        // Verify files were created
        Assert.True(Directory.Exists(outputDir));
        var files = Directory.GetFiles(outputDir, "*.json");
        Assert.True(files.Length > 0);
    }
}