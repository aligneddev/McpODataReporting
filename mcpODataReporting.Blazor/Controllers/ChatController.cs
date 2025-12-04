using Microsoft.AspNetCore.Mvc;
using mcpODataReporting.Blazor.Services;
using System.Text.Json.Serialization;

namespace mcpODataReporting.Blazor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(McpChatService mcpChatService, ILogger<ChatController> logger) : ControllerBase
{
    [HttpGet("tools")]
    public async Task<IActionResult> GetTools()
    {
        try
        {
            logger.LogInformation("API: Fetching available MCP tools");
            var tools = await mcpChatService.GetAvailableToolsAsync();
            if (tools == null)
            {
                return StatusCode(500, new { error = "Failed to fetch tools from MCP" });
            }
            return Ok(tools);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching tools");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessMessage([FromBody] ChatMessageRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Message cannot be empty" });
            }

            logger.LogInformation("API: Processing message: {Message}", request.Message);

            // Analyze the message to determine if any tools should be suggested
            var suggestedTools = AnalyzeSuggestedTools(request.Message);

            var response = new ChatMessageResponse
            {
                Response = GenerateAiResponse(request.Message, suggestedTools),
                SuggestedTools = suggestedTools
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("execute-tool")]
    public async Task<IActionResult> ExecuteTool([FromBody] ToolExecutionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ToolName))
            {
                return BadRequest(new { error = "Tool name is required" });
            }

            logger.LogInformation("API: Executing tool: {ToolName}", request.ToolName);

            var toolResponse = await mcpChatService.ExecuteToolAsync(request.ToolName, request.Input);

            if (toolResponse == null)
            {
                return StatusCode(500, new ToolExecutionResponse
                {
                    Success = false,
                    Error = "Failed to execute tool"
                });
            }

            return Ok(toolResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing tool");
            return StatusCode(500, new ToolExecutionResponse
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    private List<string> AnalyzeSuggestedTools(string message)
    {
        var suggestedTools = new List<string>();
        var lowerMessage = message.ToLower();

        // Suggest GetODataMetadata if user asks about available data
        if (lowerMessage.Contains("metadata") || 
            lowerMessage.Contains("what data") || 
            lowerMessage.Contains("available") ||
            lowerMessage.Contains("entities") ||
            lowerMessage.Contains("schema"))
        {
            suggestedTools.Add("GetODataMetadata");
        }

        // Suggest GetData if user asks for data
        if (lowerMessage.Contains("show") || 
            lowerMessage.Contains("get") || 
            lowerMessage.Contains("fetch") ||
            lowerMessage.Contains("query") ||
            lowerMessage.Contains("data") ||
            lowerMessage.Contains("list"))
        {
            suggestedTools.Add("GetData");
        }

        return suggestedTools.Distinct().ToList();
    }

    private string GenerateAiResponse(string userMessage, List<string> suggestedTools)
    {
        var response = "I'll help you with your OData query. ";

        if (suggestedTools.Contains("GetODataMetadata"))
        {
            response += "I'm retrieving the available metadata to show you what data entities are available. ";
        }

        if (suggestedTools.Contains("GetData"))
        {
            response += "I'm executing a query to fetch the data you're looking for. ";
        }

        if (!suggestedTools.Any())
        {
            response += "Please ask me about the available data, or request specific information from the OData API.";
        }
        else
        {
            response += "Processing your request...";
        }

        return response;
    }
}

public class ChatMessageRequest
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class ChatMessageResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;

    [JsonPropertyName("suggestedTools")]
    public List<string> SuggestedTools { get; set; } = [];
}

public class ToolExecutionRequest
{
    [JsonPropertyName("toolName")]
    public string ToolName { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;
}

public class ToolExecutionResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("result")]
    public string Result { get; set; } = string.Empty;

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
