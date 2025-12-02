using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using McpODataReporting.Services;
using static McpODataReporting.ToolsInformation;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.ConfigureFunctionsWebApplication();

builder
    .Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Configure HttpClient for OData API with extended timeout
builder.Services.AddHttpClient("ODataApi", client =>
{
    var odataBaseUrl = builder.Configuration["ODataApi:BaseUrl"]; // don't default to localhost so service discovery can be used in container environments
    if (!string.IsNullOrWhiteSpace(odataBaseUrl))
    {
        client.BaseAddress = new Uri(odataBaseUrl);
    }

    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromMinutes(2); // increased timeout
});

// Register OData Metadata Service
builder.Services.AddSingleton<ODataMetadataService>();

// Register Tool Registry for MCP tool discovery
var toolRegistry = new ToolRegistry();
toolRegistry.RegisterTool(GetDataToolName, GetDataToolDescription, ToolDefinitions.GetDataInputSchema());
toolRegistry.RegisterTool(ToolDefinitions.GetODataMetadataToolName, ToolDefinitions.GetODataMetadataToolDescription, ToolDefinitions.GetODataMetadataInputSchema());
builder.Services.AddSingleton(toolRegistry);

builder.Build().Run();

