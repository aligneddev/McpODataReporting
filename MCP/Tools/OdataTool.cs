using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;
using McpODataReporting.Services;
using static McpODataReporting.ToolsInformation;

namespace McpODataReporting.Tools;

public class DataAccess(
    ILogger<DataAccess> logger,
    IHttpClientFactory httpClientFactory,
    ODataMetadataService metadataService)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("ODataApi");

    [Function(nameof(GetData))]
    public async Task<string> GetData(
        [McpToolTrigger(GetDataToolName, GetDataToolDescription)] 
        ToolInvocationContext context
    )
    {
        try
        {
            logger.LogInformation("GetData tool invoked");

            // Extract the query parameter from the tool invocation context
            if (context.Arguments is Dictionary<string, object> args && args.TryGetValue("query", out var queryValue))
            {
                var query = queryValue?.ToString() ?? string.Empty;
                logger.LogInformation("Executing OData query: {Query}", query);

                // Ensure query starts with /
                if (!query.StartsWith('/'))
                {
                    query = "/" + query;
                }

                // Execute the OData query
                var response = await _httpClient.GetAsync($"/odata{query}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    logger.LogInformation("Query executed successfully");
                    return content;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    logger.LogWarning("Query failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                    return $"Error: Query failed with status {response.StatusCode}. Details: {errorContent}";
                }
            }

            logger.LogWarning("Query parameter not provided");
            return "Error: 'query' parameter not provided. Expected format: /EntitySet?$filter=...&$select=...";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetData tool");
            return $"Error: {ex.Message}";
        }
    }

    [Function("GetODataMetadata")]
    public async Task<string> GetODataMetadata(
        [McpToolTrigger("GetODataMetadata", "Retrieves the OData API schema and available entity sets with their properties. Use this to discover what data is available before querying.")] 
        ToolInvocationContext context
    )
    {
        try
        {
            logger.LogInformation("GetODataMetadata tool invoked");
            var description = await metadataService.GetToolDescriptionAsync();
            return description;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetODataMetadata tool");
            return $"Error: {ex.Message}";
        }
    }
}

