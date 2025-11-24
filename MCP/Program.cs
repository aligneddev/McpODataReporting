using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using McpODataReporting.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.ConfigureFunctionsWebApplication();

builder
    .Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Configure HttpClient for OData API with extended timeout
builder.Services.AddHttpClient("ODataApi", client =>
{
    var odataBaseUrl = builder.Configuration["ODataApi:BaseUrl"] ?? "https://localhost:7066";
    client.BaseAddress = new Uri(odataBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromMinutes(2); // increased timeout
});

// Register OData Metadata Service
builder.Services.AddSingleton<ODataMetadataService>();

builder.Build().Run();

