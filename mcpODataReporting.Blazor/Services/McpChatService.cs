using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using Microsoft.Extensions.AI;

namespace mcpODataReporting.Blazor.Services;

public class McpChatService
{
    private readonly IChatClient _chatClient;
    private readonly HttpClient _httpClient;
    private readonly ILogger<McpChatService> _logger;
    private readonly List<ChatMessage> _conversationHistory = [];
    private List<AITool>? _availableTools = [];

    private const string SystemPrompt = @"You are an intelligent OData reporting assistant. You have access to MCP (Model Context Protocol) tools that allow you to query and explore OData data sources.

When users ask questions about their data:
1. Use the GetData tool to execute OData queries against the reporting database
2. Always use GetODataMetadata first to understand available entity sets if needed
3. Format your queries using proper OData syntax with $filter, $select, $orderby, etc.
4. Present results in a clear, readable format
5. Explain what data you retrieved and what it means

Always try to be helpful and provide insights from the data. If a query fails, explain why and suggest alternatives.";

    public McpChatService(IChatClient chatClient, HttpClient httpClient, ILogger<McpChatService> logger)
    {
        _chatClient = chatClient;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task InitializeToolsAsync()
    {
        try
        {
            _logger.LogInformation("Initializing MCP tools");
            var toolsResponse = await GetAvailableToolsAsync();
            
            if (toolsResponse?.Tools != null)
            {
                _availableTools = ConvertToolsToAITools(toolsResponse.Tools);
                _logger.LogInformation("Initialized {ToolCount} tools", _availableTools.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing tools");
        }
    }

    public async Task<McpToolsResponse?> GetAvailableToolsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching available MCP tools");
            var response = await _httpClient.GetAsync("api/mcp/tools");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<McpToolsResponse>();
            }
            _logger.LogError("Failed to fetch tools: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching MCP tools");
            return null;
        }
    }

    private List<AITool> ConvertToolsToAITools(List<McpTool> tools)
    {
        var aiTools = new List<AITool>();

        foreach (var tool in tools)
        {
            try
            {
                // Create a wrapper function for the MCP tool
                var func = CreateFunctionFromMcpTool(tool);
                aiTools.Add(func);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to convert tool {ToolName} to AI function", tool.Name);
            }
        }

        return aiTools;
    }

    private AITool CreateFunctionFromMcpTool(McpTool tool)
    {
        // Create a Func that will be called when the AI model invokes this tool
        // The actual tool execution will be handled by the chat controller
        Func<string, string> toolDelegate = (input) =>
        {
            // This is a placeholder - actual execution happens server-side
            return $"Tool {tool.Name} called with input: {input}";
        };

        // Create AITool using the factory method
        var aiTool = AIFunctionFactory.Create(
            toolDelegate,
            tool.Name,
            tool.Description);

        return aiTool;
    }

    public async Task<ChatResponse?> SendMessageAsync(string userMessage)
    {
        try
        {
            _logger.LogInformation("Processing message with IChatClient: {Message}", userMessage);

            // Add system prompt at the beginning of conversation if needed
            if (_conversationHistory.Count == 0)
            {
                _conversationHistory.Add(new ChatMessage(ChatRole.System, SystemPrompt));
            }

            // Add user message to conversation history
            _conversationHistory.Add(new ChatMessage(ChatRole.User, userMessage));

            // Get response from IChatClient with tools
            var options = new ChatOptions
            {
                Tools = _availableTools?.Count > 0 ? _availableTools : null
            };

            var response = await _chatClient.GetResponseAsync(_conversationHistory, options);

            // Add assistant response to conversation history
            _conversationHistory.Add(new ChatMessage(ChatRole.Assistant, response.Text));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message with IChatClient");
            return null;
        }
    }

    public async IAsyncEnumerable<ChatResponseUpdate> StreamMessageAsync(string userMessage)
    {
        _logger.LogInformation("Streaming message with IChatClient: {Message}", userMessage);

        // Add system prompt at the beginning of conversation if needed
        if (_conversationHistory.Count == 0)
        {
            _conversationHistory.Add(new ChatMessage(ChatRole.System, SystemPrompt));
        }

        // Add user message to conversation history
        _conversationHistory.Add(new ChatMessage(ChatRole.User, userMessage));

        var responseText = "";
        
        var options = new ChatOptions
        {
            Tools = _availableTools?.Count > 0 ? _availableTools : null
        };

        await foreach (var update in _chatClient.GetStreamingResponseAsync(_conversationHistory, options))
        {
            responseText += update.Text;
            yield return update;
        }

        // Add complete assistant response to history
        _conversationHistory.Add(new ChatMessage(ChatRole.Assistant, responseText));
    }

    public void ClearConversationHistory()
    {
        _conversationHistory.Clear();
        _logger.LogInformation("Conversation history cleared");
    }

    public async Task<ToolExecutionResponse?> ExecuteToolAsync(string toolName, string toolInput)
    {
        try
        {
            _logger.LogInformation("Executing tool: {ToolName}", toolName);

            var request = new ToolExecutionRequest
            {
                ToolName = toolName,
                Input = toolInput
            };

            var response = await _httpClient.PostAsJsonAsync("api/chat/execute-tool", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ToolExecutionResponse>();
            }

            _logger.LogError("Failed to execute tool: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool");
            return null;
        }
    }
}

public class McpToolsResponse
{
    [JsonPropertyName("tools")]
    public List<McpTool> Tools { get; set; } = [];
}

public class McpTool
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("inputSchema")]
    public object InputSchema { get; set; } = new object();
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
