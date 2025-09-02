#!/bin/bash

# Export Postman collections from integration tests
echo "🚀 Exporting Postman collections from integration tests..."

# Navigate to the integration tests project
cd tests/AtomicLmsCore.IntegrationTests

# Run the export test (will create PostmanCollections folder)
dotnet test --filter "FullyQualifiedName=AtomicLmsCore.IntegrationTests.PostmanExport.ExportPostmanCollections.ExportAllPostmanCollections" --no-build

# Check if export was successful
if [ -d "PostmanCollections" ]; then
    echo ""
    echo "✅ Export complete! Files created in:"
    echo "   tests/AtomicLmsCore.IntegrationTests/PostmanCollections/"
    echo ""
    echo "📁 Generated files:"
    ls -la PostmanCollections/*.json
    echo ""
    echo "📌 To import into Postman:"
    echo "   1. Open Postman"
    echo "   2. Click 'Import' button"
    echo "   3. Drag and drop all .json files from PostmanCollections folder"
    echo "   4. Select the AtomicLMS-Dev environment after import"
else
    echo "❌ Export failed. Please check the logs above."
fi