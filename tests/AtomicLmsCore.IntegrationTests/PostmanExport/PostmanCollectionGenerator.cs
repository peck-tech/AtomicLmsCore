using System.Text.Json;
using System.Text.Json.Serialization;

namespace AtomicLmsCore.IntegrationTests.PostmanExport;

public class PostmanCollectionGenerator(string collectionName, string baseUrl = "{{baseUrl}}")
{
    private readonly PostmanCollection _collection = new()
    {
        Info = new PostmanInfo
        {
            Name = collectionName,
            Schema = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
        },
        Variable =
        [
            new()
            {
                Key = "baseUrl", Value = baseUrl, Type = "string"
            },
            new()
            {
                Key = "tenantId", Value = "{{$guid}}", Type = "string"
            },
            new()
            {
                Key = "userId", Value = "{{$guid}}", Type = "string"
            },
            new()
            {
                Key = "learningObjectId", Value = "{{$guid}}", Type = "string"
            },
            new()
            {
                Key = "correlationId", Value = "", Type = "string"
            }
        ]
    };

    public PostmanFolder AddFolder(string name, string description = "")
    {
        var folder = new PostmanFolder
        {
            Name = name,
            Description = description,
            Item = []
        };
        _collection.Item.Add(folder);
        return folder;
    }

    public static void AddRequest(PostmanFolder folder, PostmanRequest request)
        => folder.Item.Add(request);

    public static PostmanRequest CreateRequest(
        string name,
        string method,
        string url,
        object? body = null,
        Dictionary<string, string>? headers = null,
        List<string>? tests = null,
        string? preRequestScript = null)
    {
        var request = new PostmanRequest
        {
            Name = name,
            Request = new PostmanRequestDetails
            {
                Method = method,
                Header = new List<PostmanHeader>(),
                Url = new PostmanUrl { Raw = url }
            }
        };

        // Add default headers
        request.Request.Header.Add(new PostmanHeader { Key = "Content-Type", Value = "application/json" });
        request.Request.Header.Add(new PostmanHeader { Key = "Authorization", Value = "Bearer {{token}}" });

        // Add custom headers
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Request.Header.Add(new PostmanHeader { Key = header.Key, Value = header.Value });
            }
        }

        // Add body
        if (body != null)
        {
            request.Request.Body = new PostmanBody
            {
                Mode = "raw",
                Raw = JsonSerializer.Serialize(body, new JsonSerializerOptions { WriteIndented = true }),
                Options = new PostmanBodyOptions
                {
                    Raw = new PostmanRawOptions { Language = "json" }
                }
            };
        }

        // Add tests
        if (tests != null && tests.Count > 0)
        {
            request.Event =
            [
                new()
                {
                    Listen = "test",
                    Script = new PostmanScript
                    {
                        Type = "text/javascript", Exec = tests
                    }
                }
            ];
        }

        // Add pre-request script
        if (string.IsNullOrEmpty(preRequestScript))
        {
            return request;
        }
        request.Event ??= new List<PostmanEvent>();
        request.Event.Add(new PostmanEvent
        {
            Listen = "prerequest",
            Script = new PostmanScript
            {
                Type = "text/javascript",
                Exec = new List<string> { preRequestScript }
            }
        });

        return request;
    }

    private string GenerateJson()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        return JsonSerializer.Serialize(_collection, options);
    }

    public async Task SaveToFileAsync(string filePath)
    {
        var json = GenerateJson();
        await File.WriteAllTextAsync(filePath, json);
    }
}

// Postman Collection Schema Classes
public class PostmanCollection
{
    public PostmanInfo Info { get; set; } = new();
    public List<object> Item { get; set; } = new();
    public List<PostmanVariable> Variable { get; set; } = new();
}

public class PostmanInfo
{
    public string Name { get; set; } = "";
    public string Schema { get; set; } = "";
    public string? Description { get; set; }
}

public class PostmanFolder
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public List<object> Item { get; set; } = new();
}

public class PostmanRequest
{
    public string Name { get; set; } = "";
    public PostmanRequestDetails Request { get; set; } = new();
    public List<PostmanEvent>? Event { get; set; }
}

public class PostmanRequestDetails
{
    public string Method { get; set; } = "";
    public List<PostmanHeader> Header { get; set; } = new();
    public PostmanUrl Url { get; set; } = new();
    public PostmanBody? Body { get; set; }
}

public class PostmanHeader
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}

public class PostmanUrl
{
    public string Raw { get; set; } = "";
}

public class PostmanBody
{
    public string Mode { get; set; } = "";
    public string Raw { get; set; } = "";
    public PostmanBodyOptions? Options { get; set; }
}

public class PostmanBodyOptions
{
    public PostmanRawOptions? Raw { get; set; }
}

public class PostmanRawOptions
{
    public string Language { get; set; } = "";
}

public class PostmanEvent
{
    public string Listen { get; set; } = "";
    public PostmanScript Script { get; set; } = new();
}

public class PostmanScript
{
    public string Type { get; set; } = "";
    public List<string> Exec { get; set; } = new();
}

public class PostmanVariable
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public string Type { get; set; } = "";
}
