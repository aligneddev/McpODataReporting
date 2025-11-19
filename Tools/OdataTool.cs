using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;
using static McpODataReporting.ToolsInformation;

namespace McpODataReporting.Tools;

public class ODataAccess(ILogger<ODataAccess> logger)
{
    [Function(nameof(GetData))]
    public string GetData(
        [McpToolTrigger(GetDataToolName, GetDataToolDescription)] ToolInvocationContext context
    )
    {
        logger.LogInformation("Saying hello");
        return "Hello I am MCP Tool!";
    }
}
