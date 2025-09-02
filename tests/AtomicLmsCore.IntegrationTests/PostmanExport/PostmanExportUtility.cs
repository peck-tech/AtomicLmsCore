namespace AtomicLmsCore.IntegrationTests.PostmanExport;

/// <summary>
/// Utility class for exporting Postman collections standalone
/// </summary>
public static class PostmanExportUtility
{
    /// <summary>
    /// Export Postman collections as a standalone operation
    /// </summary>
    public static async Task ExportCollections(string[] args)
    {
        var outputDir = args.Length > 0
            ? args[0]
            : Path.Combine(Environment.CurrentDirectory, "PostmanCollections");

        Console.WriteLine($"Exporting Postman collections to: {outputDir}");
        await AtomicLmsPostmanExporter.ExportAllCollections(outputDir);

        Console.WriteLine("\n📁 Files created:");
        foreach (var file in Directory.GetFiles(outputDir))
        {
            Console.WriteLine($"  - {Path.GetFileName(file)}");
        }

        Console.WriteLine("\n📌 Import instructions:");
        Console.WriteLine("1. Open Postman");
        Console.WriteLine("2. Click 'Import' button");
        Console.WriteLine("3. Select all .json files from the output directory");
        Console.WriteLine("4. Click 'Import'");
        Console.WriteLine("\n✅ Done!");
    }
}
