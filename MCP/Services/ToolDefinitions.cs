using System.Text.Json;

namespace McpODataReporting.Services;

public static class ToolDefinitions
{
    public const string GetODataMetadataToolName = "GetODataMetadata";
    public const string GetODataMetadataToolDescription = "Retrieves the OData API schema and available entity sets with their properties. Use this to discover what data is available before querying.";

    public static JsonElement GetDataInputSchema()
    {
        var schema = new
        {
            type = "object",
            properties = new
            {
                query = new
                {
                    type = "string",
                    description = "OData query string to execute against the reporting data source. Format: /EntitySetName?$queryOptions"
                }
            },
            required = new[] { "query" }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    public static JsonElement GetODataMetadataInputSchema()
    {
        var schema = new
        {
            type = "object",
            properties = new { },
            required = new string[] { }
        };

        return JsonSerializer.SerializeToElement(schema);
    }
}
