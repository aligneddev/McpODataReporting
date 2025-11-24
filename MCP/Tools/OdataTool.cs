using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;
using static McpODataReporting.ToolsInformation;

namespace McpODataReporting.Tools;

public class DataAccess(ILogger<DataAccess> logger)
{
    [Function(nameof(GetData))]
    public async Task<string> GetData(
        [McpToolTrigger(GetDataToolName, GetDataToolDescription)] ToolInvocationContext context
    )
    {
        try
        {
            logger.LogInformation("GetData tool invoked");

            //// Extract the query parameter from the tool invocation context
            //if (context.Arguments is Dictionary<string, object> args && args.TryGetValue("query", out var queryValue))
            //{
            //    var query = queryValue?.ToString() ?? string.Empty;
            //    logger.LogInformation("Executing OData query: {Query}", query);

            //    var result = await _oDataService.ExecuteQueryAsync(query);
            //    return result;
            //}

            return "Error: 'query' parameter not provided. Expected format: /EntitySet?$filter=...&$select=...";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetData tool");
            return $"Error: {ex.Message}";
        }
    }
}
