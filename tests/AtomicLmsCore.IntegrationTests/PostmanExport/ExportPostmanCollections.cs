namespace AtomicLmsCore.IntegrationTests.PostmanExport;

/// <summary>
/// Run this test to generate Postman collections from integration test scenarios
/// </summary>
public class ExportPostmanCollections
{

    [Fact] // Remove skip to run the export
    public async Task ExportAllPostmanCollections()
    {
        // Set the output directory (relative to project root)
        var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "PostmanCollections");

        await AtomicLmsPostmanExporter.ExportAllCollections(outputDir);

        Assert.True(Directory.Exists(outputDir), "Output directory should be created");
        Assert.True(File.Exists(Path.Combine(outputDir, "AtomicLMS-Tenants-API.postman_collection.json")));
        Assert.True(File.Exists(Path.Combine(outputDir, "AtomicLMS-Users-API.postman_collection.json")));
        Assert.True(File.Exists(Path.Combine(outputDir, "AtomicLMS-LearningObjects-API.postman_collection.json")));
        Assert.True(File.Exists(Path.Combine(outputDir, "AtomicLMS-Dev.postman_environment.json")));
    }

}
