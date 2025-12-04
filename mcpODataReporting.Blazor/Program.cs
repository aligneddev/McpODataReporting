using mcpODataReporting.Blazor.Components;
using mcpODataReporting.Blazor.Services;
using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults from ServiceDefaults project
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add controllers for API endpoints
builder.Services.AddControllers();

// Configure Azure OpenAI Chat Client
// These values are injected by Aspire AppHost from Parameters configuration
var azureOpenAIEndpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"];
var azureOpenAIDeployment = builder.Configuration["AZURE_OPENAI_DEPLOYMENT_NAME"];

if (string.IsNullOrEmpty(azureOpenAIEndpoint) || string.IsNullOrEmpty(azureOpenAIDeployment))
{
    var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup");
    logger.LogWarning("Azure OpenAI configuration not found. AZURE_OPENAI_ENDPOINT: {Endpoint}, AZURE_OPENAI_DEPLOYMENT_NAME: {Deployment}. " +
        "Chat functionality will not be available. Please configure these values in AppHost appsettings.json or as azd parameters.",
        azureOpenAIEndpoint ?? "null", azureOpenAIDeployment ?? "null");
}
else
{
    // Register IChatClient with Azure OpenAI
    builder.Services.AddSingleton<IChatClient>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Creating Azure OpenAI Chat Client with endpoint: {Endpoint}, deployment: {Deployment}", 
            azureOpenAIEndpoint, azureOpenAIDeployment);

        var azureClient = new AzureOpenAIClient(
            new Uri(azureOpenAIEndpoint),
            new DefaultAzureCredential());

        // Build the chat client with function invocation for MCP tool integration
        // UseFunctionInvocation() automatically handles tool invocation when tools are provided in ChatOptions
        return new ChatClientBuilder(azureClient.GetChatClient(azureOpenAIDeployment).AsIChatClient())
            .UseFunctionInvocation()
            .Build();
    });
}

// Add HttpClient for MCP Functions communication
builder.Services.AddHttpClient("McpFunctions", client =>
{
    // Aspire service discovery will resolve mcpodatareporting hostname
    client.BaseAddress = new Uri("http://mcpodatareporting");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromMinutes(2);
});

// Register MCP Chat Service with both IChatClient and HttpClient
builder.Services.AddScoped<McpChatService>(sp =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("McpFunctions");
    var logger = sp.GetRequiredService<ILogger<McpChatService>>();
    
    var service = new McpChatService(chatClient, httpClient, logger);
    return service;
});

var app = builder.Build();

// Add default Aspire middleware
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map API controllers
app.MapControllers();

app.Run();
