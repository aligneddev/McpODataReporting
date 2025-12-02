using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using McpODataReporting.Services;
using System.Text.Json;

namespace McpODataReporting.Tools;

public class ToolDiscoveryEndpoint
{
    private readonly ToolRegistry _toolRegistry;
    private readonly ILogger<ToolDiscoveryEndpoint> _logger;

    public ToolDiscoveryEndpoint(ToolRegistry toolRegistry, ILogger<ToolDiscoveryEndpoint> logger)
    {
        _toolRegistry = toolRegistry;
        _logger = logger;
    }

    [Function("GetToolsMetadata")]
    public HttpResponseData GetToolsMetadata(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "mcp/tools")] 
        HttpRequestData req)
    {
        _logger.LogInformation("Tool metadata discovery endpoint called");

        try
        {
            var tools = _toolRegistry.GetAllTools();
            var toolMetadata = tools.Select(tool => new
            {
                name = tool.Name,
                description = tool.Description,
                inputSchema = tool.InputSchema
            });

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            
            using (var writer = new StreamWriter(response.Body))
            {
                var json = JsonSerializer.Serialize(new { tools = toolMetadata });
                writer.Write(json);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in tool metadata discovery endpoint");
            var response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            response.Headers.Add("Content-Type", "application/json");
            
            using (var writer = new StreamWriter(response.Body))
            {
                var json = JsonSerializer.Serialize(new { error = ex.Message });
                writer.Write(json);
            }

            return response;
        }
    }
}
