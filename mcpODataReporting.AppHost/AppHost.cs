using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddAzureContainerAppEnvironment("appcontainerenv");

var reportingDb = builder.AddConnectionString("ReportingDb");

var odataApi = builder
    .AddProject<Projects.ODataApi>("odataapi")
    .WithReference(reportingDb)
    .PublishAsAzureContainerApp(
        (infra, app) =>
        {
            app.Template.Scale.MinReplicas = 0;
        }
    );

// Add Azure Storage for Functions runtime
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator()
    .AddBlobs("blobs");

// When using the AddAzureFunctionsProject method in .NET Aspire, the deployment is currently configured to publish the Azure Functions project as a container, either to Azure Container Apps (ACA) or as a containerized application in Azure App Service, rather than as a native serverless function app.
//  This behavior is consistent with the current documentation, which states that Aspire supports deploying Functions projects as containers and does not yet provide a first-class option for direct deployment to the native Azure Functions serverless runtime.
// https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-aspire-integration
builder
    .AddAzureFunctionsProject<Projects.McpODataReporting>("mcpodatareporting")
    .WithExternalHttpEndpoints()
    .WithReference(odataApi)
    .WithReference(storage)
    .WaitFor(odataApi)
    .WaitFor(storage)
    .PublishAsAzureContainerApp(
        (infra, app) =>
        {
            app.Template.Scale.MinReplicas = 0;
        }
    );

builder.Build().Run();
