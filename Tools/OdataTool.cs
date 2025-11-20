using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;
using McpODataReporting.Services;
using static McpODataReporting.ToolsInformation;

namespace McpODataReporting.Tools;

public class ODataAccess
{
    private readonly IODataQueryService _oDataService;
    private readonly ILogger<ODataAccess> _logger;

    public ODataAccess(IODataQueryService oDataService, ILogger<ODataAccess> logger)
    {
        _oDataService = oDataService;
        _logger = logger;
    }

    [Function(nameof(GetData))]
    public async Task<string> GetData(
        [McpToolTrigger(GetDataToolName, GetDataToolDescription)] ToolInvocationContext context
    )
    {
        try
        {
            _logger.LogInformation("GetData tool invoked");

            // Extract the query parameter from the tool invocation context
            if (context.Arguments is Dictionary<string, object> args && args.TryGetValue("query", out var queryValue))
            {
                var query = queryValue?.ToString() ?? string.Empty;
                _logger.LogInformation("Executing OData query: {Query}", query);

                var result = await _oDataService.ExecuteQueryAsync(query);
                return result;
            }

            return "Error: 'query' parameter not provided. Expected format: /EntitySet?$filter=...&$select=...";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetData tool");
            return $"Error: {ex.Message}";
        }
    }
}
