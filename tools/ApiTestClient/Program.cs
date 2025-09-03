using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;

namespace ApiTestClient;

class Program
{
    private static readonly HttpClient _httpClient = new();
    private static readonly string _baseUrl = "https://localhost:7001";

    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 AtomicLMS API Test Client");
        Console.WriteLine("============================");
        
        // Configure HTTP client
        _httpClient.DefaultRequestHeaders.Add("X-Test-Role", "superadmin");
        _httpClient.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        try
        {
            await RunTenantTests();
            Console.WriteLine("\n✅ All tests completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Test failed: {ex.Message}");
            Environment.Exit(1);
        }
    }

    static async Task RunTenantTests()
    {
        Console.WriteLine("\n📂 Testing Tenant API...");
        
        // Create tenant
        Console.WriteLine("  Creating tenant...");
        var createRequest = new
        {
            name = "Test Tenant via Client",
            slug = "test-tenant-client",
            databaseName = "TestClientDb",
            isActive = true,
            metadata = new { environment = "test", client = "console" }
        };

        var createResponse = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/v0.1/solution/tenants", createRequest);
        createResponse.EnsureSuccessStatusCode();
        
        var tenantIdJson = await createResponse.Content.ReadAsStringAsync();
        var tenantId = JsonConvert.DeserializeObject<Guid>(tenantIdJson);
        Console.WriteLine($"  ✓ Tenant created: {tenantId}");

        // Get tenant
        Console.WriteLine("  Retrieving tenant...");
        var getResponse = await _httpClient.GetAsync($"{_baseUrl}/api/v0.1/solution/tenants/{tenantId}");
        getResponse.EnsureSuccessStatusCode();
        
        var tenant = await getResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"  ✓ Tenant retrieved");

        // Update tenant  
        Console.WriteLine("  Updating tenant...");
        var updateRequest = new
        {
            name = "Updated Test Tenant",
            slug = "updated-test-tenant-client",
            isActive = false,
            metadata = new { updated = "true" }
        };

        var updateResponse = await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/v0.1/solution/tenants/{tenantId}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        Console.WriteLine($"  ✓ Tenant updated");

        // Delete tenant
        Console.WriteLine("  Deleting tenant...");
        var deleteResponse = await _httpClient.DeleteAsync($"{_baseUrl}/api/v0.1/solution/tenants/{tenantId}");
        deleteResponse.EnsureSuccessStatusCode();
        Console.WriteLine($"  ✓ Tenant deleted");
    }
}