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
    client.BaseAddress = new Uri("http://odataapi"); // Aspire Service discovery will resolve this hostname
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


// ASP.NET Core Program.cs / minimal API
app.MapGet("/mcp/.well-known/oauth-authorization-server", () =>
    Results.Json(new
    {
        authorization_endpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize",
        token_endpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token",
        issuer = $"https://login.microsoftonline.com/{tenantId}/v2.0",
        scopes_supported = new[] { "api://<your-app-id-uri>/.default" }
    }));


builder.Build().Run();

