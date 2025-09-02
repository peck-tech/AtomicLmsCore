# API Testing with Postman

## Using the Pre-Generated Collections

The project includes Postman collections and environment files in the `/PostmanCollections` directory:
- `AtomicLMS-Tenants-API.postman_collection.json` - Tenant management endpoints
- `AtomicLMS-Users-API.postman_collection.json` - User management endpoints  
- `AtomicLMS-LearningObjects-API.postman_collection.json` - Learning object endpoints
- `AtomicLMS-Dev.postman_environment.json` - Environment variables

### Importing into Postman

1. Open Postman application
2. Click the **Import** button in the top-left
3. Drag and drop all JSON files from `/PostmanCollections` folder
4. Click **Import** to confirm
5. In the top-right environment dropdown, select "AtomicLMS Development"

### Running the Collections

1. Open any imported collection (e.g., "AtomicLMS - Tenants API")
2. Expand folders to see individual requests
3. Click on a request to view details
4. Click **Send** to execute the request
5. View test results in the **Tests** tab

**Note:** Some requests require authentication tokens. Update the `token` variable in the environment with a valid JWT token.

## Regenerating Postman Collections

The Postman collections are programmatically generated from integration tests to ensure they stay in sync with the actual API.

### Quick Export

```bash
# Run from project root
dotnet test --filter "ExportAllPostmanCollections"
```

This will regenerate all collections in `/PostmanCollections`.

### Manual Export Steps

1. Navigate to the integration tests project:
   ```bash
   cd tests/AtomicLmsCore.IntegrationTests
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run the export test:
   ```bash
   dotnet test --filter "ExportAllPostmanCollections" --no-build
   ```

4. Find the generated files:
   - Collections are created in `bin/Debug/net9.0/PostmanCollections/`
   - Copy them to the project root: `cp -r bin/Debug/net9.0/PostmanCollections ../../`

### Adding New Endpoints to Postman Export

When adding new API endpoints:

1. Create integration tests for the endpoints
2. Update `AtomicLmsPostmanExporter.cs` to include the new endpoints
3. Run the export process to regenerate collections
4. Commit the updated JSON files

## Using with Newman (CLI)

Newman allows running Postman collections from the command line, useful for CI/CD pipelines:

```bash
# Install Newman globally
npm install -g newman

# Run a collection
newman run PostmanCollections/AtomicLMS-Tenants-API.postman_collection.json \
  --environment PostmanCollections/AtomicLMS-Dev.postman_environment.json

# Run with custom variables
newman run PostmanCollections/AtomicLMS-Users-API.postman_collection.json \
  --environment PostmanCollections/AtomicLMS-Dev.postman_environment.json \
  --global-var "baseUrl=https://api.staging.atomiclms.com" \
  --global-var "token=$API_TOKEN"

# Generate HTML report
newman run PostmanCollections/AtomicLMS-Tenants-API.postman_collection.json \
  --environment PostmanCollections/AtomicLMS-Dev.postman_environment.json \
  --reporters cli,html \
  --reporter-html-export test-results.html
```