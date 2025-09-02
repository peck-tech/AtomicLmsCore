using AtomicLmsCore.WebApi.DTOs.LearningObjects;
using AtomicLmsCore.WebApi.DTOs.Tenants;
using AtomicLmsCore.WebApi.DTOs.Users;
// ReSharper disable RedundantArgumentDefaultValue

namespace AtomicLmsCore.IntegrationTests.PostmanExport;

/// <summary>
/// Generates Postman collections from integration test scenarios
/// </summary>
public class AtomicLmsPostmanExporter
{
    public static async Task ExportAllCollections(string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        await ExportTenantsCollection(Path.Combine(outputDirectory, "AtomicLMS-Tenants-API.postman_collection.json"));
        await ExportUsersCollection(Path.Combine(outputDirectory, "AtomicLMS-Users-API.postman_collection.json"));
        await ExportLearningObjectsCollection(Path.Combine(outputDirectory, "AtomicLMS-LearningObjects-API.postman_collection.json"));
        await ExportEnvironment(Path.Combine(outputDirectory, "AtomicLMS-Dev.postman_environment.json"));

        Console.WriteLine($"✅ Postman collections exported to: {outputDirectory}");
    }

    private static async Task ExportTenantsCollection(string filePath)
    {
        // ReSharper disable once RedundantArgumentDefaultValue
        var generator = new PostmanCollectionGenerator("AtomicLMS - Tenants API", "{{baseUrl}}");

        // Authentication Tests Folder
        var authFolder = generator.AddFolder("Authentication & Authorization",
            "Tests for role-based access control");

        PostmanCollectionGenerator.AddRequest(authFolder, PostmanCollectionGenerator.CreateRequest(
            "Get All Tenants - No Auth (Should Fail)",
            "GET",
            "{{baseUrl}}/api/v0.1/solution/tenants",
            headers: new Dictionary<string, string> { ["Authorization"] = "" },
            tests:
            [
                "pm.test('Status code is 401 Unauthorized', function() {",
                "    pm.response.to.have.status(401);",
                "});"
            ]
        ));

        PostmanCollectionGenerator.AddRequest(authFolder, PostmanCollectionGenerator.CreateRequest(
            "Get All Tenants - Wrong Role (Should Fail)",
            "GET",
            "{{baseUrl}}/api/v0.1/solution/tenants",
            headers: new Dictionary<string, string> { ["X-Test-Role"] = "user" },
            tests:
            [
                "pm.test('Status code is 403 Forbidden', function() {",
                "    pm.response.to.have.status(403);",
                "});"
            ]
        ));

        // CRUD Operations Folder
        var crudFolder = generator.AddFolder("CRUD Operations",
            "Create, Read, Update, Delete operations for tenants");

        // Create Tenant
        PostmanCollectionGenerator.AddRequest(crudFolder, PostmanCollectionGenerator.CreateRequest(
            "Create Tenant",
            "POST",
            "{{baseUrl}}/api/v0.1/solution/tenants",
            body: new CreateTenantRequestDto(
                "Test Tenant",
                "test-tenant",
                "TestDatabase",
                true,
                new Dictionary<string, string> { ["environment"] = "test" }
            ),
            headers: new Dictionary<string, string> { ["X-Test-Role"] = "superadmin" },
            tests:
            [
                "pm.test('Status code is 201 Created', function() {",
                "    pm.response.to.have.status(201);",
                "});",
                "",
                "pm.test('Response contains tenant ID', function() {",
                "    var tenantId = pm.response.json();",
                "    pm.expect(tenantId).to.be.a('string');",
                "    pm.environment.set('createdTenantId', tenantId);",
                "});",
                "",
                "pm.test('Has Correlation ID header', function() {",
                "    pm.response.to.have.header('X-Correlation-ID');",
                "    pm.environment.set('correlationId', pm.response.headers.get('X-Correlation-ID'));",
                "});"
            ]
        ));

        // Get All Tenants
        PostmanCollectionGenerator.AddRequest(crudFolder, PostmanCollectionGenerator.CreateRequest(
            "Get All Tenants",
            "GET",
            "{{baseUrl}}/api/v0.1/solution/tenants",
            headers: new Dictionary<string, string> { ["X-Test-Role"] = "superadmin" },
            tests:
            [
                "pm.test('Status code is 200', function() {",
                "    pm.response.to.have.status(200);",
                "});",
                "",
                "pm.test('Response is an array', function() {",
                "    var tenants = pm.response.json();",
                "    pm.expect(tenants).to.be.an('array');",
                "});",
                "",
                "pm.test('Each tenant has required properties', function() {",
                "    var tenants = pm.response.json();",
                "    tenants.forEach(function(tenant) {",
                "        pm.expect(tenant).to.have.property('id');",
                "        pm.expect(tenant).to.have.property('name');",
                "        pm.expect(tenant).to.have.property('slug');",
                "        pm.expect(tenant).to.have.property('isActive');",
                "    });",
                "});"
            ]
        ));

        // Get Tenant by ID
        PostmanCollectionGenerator.AddRequest(crudFolder, PostmanCollectionGenerator.CreateRequest(
            "Get Tenant by ID",
            "GET",
            "{{baseUrl}}/api/v0.1/solution/tenants/{{createdTenantId}}",
            headers: new Dictionary<string, string> { ["X-Test-Role"] = "superadmin" },
            tests:
            [
                "pm.test('Status code is 200', function() {",
                "    pm.response.to.have.status(200);",
                "});",
                "",
                "pm.test('Tenant has correct structure', function() {",
                "    var tenant = pm.response.json();",
                "    pm.expect(tenant.id).to.equal(pm.environment.get('createdTenantId'));",
                "    pm.expect(tenant).to.have.property('name');",
                "    pm.expect(tenant).to.have.property('databaseName');",
                "    pm.expect(tenant).to.have.property('metadata');",
                "});"
            ]
        ));

        // Update Tenant
        PostmanCollectionGenerator.AddRequest(crudFolder, PostmanCollectionGenerator.CreateRequest(
            "Update Tenant",
            "PUT",
            "{{baseUrl}}/api/v0.1/solution/tenants/{{createdTenantId}}",
            body: new UpdateTenantRequestDto(
                "Updated Tenant Name",
                "updated-tenant",
                false,
                new Dictionary<string, string> { ["updated"] = "true" }
            ),
            headers: new Dictionary<string, string> { ["X-Test-Role"] = "superadmin" },
            tests:
            [
                "pm.test('Status code is 204 No Content', function() {",
                "    pm.response.to.have.status(204);",
                "});"
            ]
        ));

        // Delete Tenant
        PostmanCollectionGenerator.AddRequest(crudFolder, PostmanCollectionGenerator.CreateRequest(
            "Delete Tenant",
            "DELETE",
            "{{baseUrl}}/api/v0.1/solution/tenants/{{createdTenantId}}",
            headers: new Dictionary<string, string> { ["X-Test-Role"] = "superadmin" },
            tests:
            [
                "pm.test('Status code is 204 No Content', function() {",
                "    pm.response.to.have.status(204);",
                "});"
            ]
        ));

        // Validation Tests Folder
        var validationFolder = generator.AddFolder("Validation Tests",
            "Input validation and error handling");

        PostmanCollectionGenerator.AddRequest(validationFolder, PostmanCollectionGenerator.CreateRequest(
            "Create Tenant - Invalid Data",
            "POST",
            "{{baseUrl}}/api/v0.1/solution/tenants",
            body: new { name = "", slug = "test", databaseName = "db", isActive = true },
            headers: new Dictionary<string, string> { ["X-Test-Role"] = "superadmin" },
            tests:
            [
                "pm.test('Status code is 400 Bad Request', function() {",
                "    pm.response.to.have.status(400);",
                "});",
                "",
                "pm.test('Error response has expected structure', function() {",
                "    var error = pm.response.json();",
                "    pm.expect(error).to.have.property('code');",
                "    pm.expect(error).to.have.property('message');",
                "    pm.expect(error).to.have.property('correlationId');",
                "});"
            ]
        ));

        await generator.SaveToFileAsync(filePath);
    }

    private static async Task ExportUsersCollection(string filePath)
    {
        var generator = new PostmanCollectionGenerator("AtomicLMS - Users API", "{{baseUrl}}");

        // Tenant Context Folder
        var tenantFolder = generator.AddFolder("Tenant Context",
            "Tests for multi-tenant user management");

        PostmanCollectionGenerator.AddRequest(tenantFolder, PostmanCollectionGenerator.CreateRequest(
            "Create User in Tenant",
            "POST",
            "{{baseUrl}}/api/v0.1/learners/users",
            body: new CreateUserRequestDto
            {
                ExternalUserId = "ext-user-{{$randomInt}}",
                Email = "user{{$randomInt}}@test.com",
                FirstName = "Test",
                LastName = "User",
                DisplayName = "Test User",
                IsActive = true,
                Metadata = new Dictionary<string, string> { ["role"] = "student" }
            },
            headers: new Dictionary<string, string>
            {
                ["X-Test-Tenant"] = "{{tenantId}}",
                ["X-Tenant-Id"] = "{{tenantId}}"
            },
            tests:
            [
                "pm.test('Status code is 201 Created', function() {",
                "    pm.response.to.have.status(201);",
                "});",
                "",
                "pm.test('Returns user ID', function() {",
                "    var userId = pm.response.json();",
                "    pm.expect(userId).to.be.a('string');",
                "    pm.environment.set('createdUserId', userId);",
                "});"
            ],
            preRequestScript: "// Set tenant context\npm.environment.set('tenantId', pm.variables.replaceIn('{{$guid}}'));"
        ));

        PostmanCollectionGenerator.AddRequest(tenantFolder, PostmanCollectionGenerator.CreateRequest(
            "Get All Users in Tenant",
            "GET",
            "{{baseUrl}}/api/v0.1/learners/users",
            headers: new Dictionary<string, string>
            {
                ["X-Test-Tenant"] = "{{tenantId}}",
                ["X-Tenant-Id"] = "{{tenantId}}"
            },
            tests:
            [
                "pm.test('Status code is 200', function() {",
                "    pm.response.to.have.status(200);",
                "});",
                "",
                "pm.test('Returns array of users', function() {",
                "    var users = pm.response.json();",
                "    pm.expect(users).to.be.an('array');",
                "});",
                "",
                "pm.test('Users have correct structure', function() {",
                "    var users = pm.response.json();",
                "    if (users.length > 0) {",
                "        var user = users[0];",
                "        pm.expect(user).to.have.property('id');",
                "        pm.expect(user).to.have.property('email');",
                "        pm.expect(user).to.have.property('firstName');",
                "        pm.expect(user).to.have.property('lastName');",
                "    }",
                "});"
            ]
        ));

        await generator.SaveToFileAsync(filePath);
    }

    private static async Task ExportLearningObjectsCollection(string filePath)
    {
        var generator = new PostmanCollectionGenerator("AtomicLMS - Learning Objects API", "{{baseUrl}}");

        var crudFolder = generator.AddFolder("Learning Object Management",
            "CRUD operations for learning objects");

        PostmanCollectionGenerator.AddRequest(crudFolder, PostmanCollectionGenerator.CreateRequest(
            "Create Learning Object",
            "POST",
            "{{baseUrl}}/api/v0.1/learning/learningobjects",
            body: new CreateLearningObjectRequestDto(
                "Introduction to Programming",
                new Dictionary<string, string>
                {
                    ["type"] = "video",
                    ["duration"] = "45 minutes",
                    ["difficulty"] = "beginner"
                }
            ),
            headers: new Dictionary<string, string>
            {
                ["X-Test-Tenant"] = "{{tenantId}}",
                ["X-Tenant-Id"] = "{{tenantId}}"
            },
            tests:
            [
                "pm.test('Status code is 201 Created', function() {",
                "    pm.response.to.have.status(201);",
                "});",
                "",
                "pm.test('Returns learning object ID', function() {",
                "    var id = pm.response.json();",
                "    pm.expect(id).to.be.a('string');",
                "    pm.environment.set('createdLearningObjectId', id);",
                "});"
            ]
        ));

        await generator.SaveToFileAsync(filePath);
    }

    private static async Task ExportEnvironment(string filePath)
    {
        var environment = new
        {
            id = Guid.NewGuid().ToString(),
            name = "AtomicLMS Development",
            values = new[]
            {
                new { key = "baseUrl", value = "https://localhost:7001", enabled = true },
                new { key = "token", value = "", enabled = true },
                new { key = "tenantId", value = "", enabled = true },
                new { key = "createdTenantId", value = "", enabled = true },
                new { key = "createdUserId", value = "", enabled = true },
                new { key = "createdLearningObjectId", value = "", enabled = true },
                new { key = "correlationId", value = "", enabled = true }
            },
            _postman_variable_scope = "environment",
            _postman_exported_at = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            _postman_exported_using = "AtomicLMS Integration Test Exporter"
        };

        var json = System.Text.Json.JsonSerializer.Serialize(environment, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(filePath, json);
    }
}
