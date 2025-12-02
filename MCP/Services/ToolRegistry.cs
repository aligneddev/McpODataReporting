using System.Text.Json;

namespace McpODataReporting.Services;

public class ToolDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public JsonElement InputSchema { get; set; }
}

public class ToolRegistry
{
    private readonly List<ToolDefinition> _tools = [];

    public void RegisterTool(string name, string description, JsonElement inputSchema)
    {
        _tools.Add(new ToolDefinition
        {
            Name = name,
            Description = description,
            InputSchema = inputSchema
        });
    }

    public IReadOnlyList<ToolDefinition> GetAllTools() => _tools.AsReadOnly();

    public ToolDefinition? GetTool(string name) => _tools.FirstOrDefault(t => t.Name == name);
}
