using AtomicLmsCore.IntegrationTests.PostmanExport;

namespace PostmanExporter;

class Program
{
    static async Task Main(string[] args)
    {
        var outputDir = args.Length > 0 
            ? args[0] 
            : Path.Combine(Directory.GetCurrentDirectory(), "PostmanCollections");
        
        Console.WriteLine("🚀 AtomicLMS Postman Collection Exporter");
        Console.WriteLine("========================================");
        Console.WriteLine($"Output directory: {outputDir}");
        Console.WriteLine();

        try
        {
            await AtomicLmsPostmanExporter.ExportAllCollections(outputDir);
            
            Console.WriteLine("\n📁 Successfully exported:");
            foreach (var file in Directory.GetFiles(outputDir, "*.json"))
            {
                var fileInfo = new FileInfo(file);
                Console.WriteLine($"  ✓ {fileInfo.Name} ({fileInfo.Length / 1024}KB)");
            }
            
            Console.WriteLine("\n📌 Import into Postman:");
            Console.WriteLine("  1. Open Postman");
            Console.WriteLine("  2. Click 'Import' button");
            Console.WriteLine("  3. Drag and drop the JSON files");
            Console.WriteLine("  4. Select 'AtomicLMS Development' environment");
            Console.WriteLine("\n✅ Export complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Export failed: {ex.Message}");
            Environment.Exit(1);
        }
    }
}