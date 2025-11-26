using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddAzureContainerAppEnvironment("appcontainerenv");

var reportingDb = builder.AddConnectionString("ReportingDb");

var odataApi = builder.AddProject<Projects.ODataApi>("odataapi")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(reportingDb)
    .PublishAsAzureContainerApp((infra, app) =>
    {
        app.Template.Scale.MinReplicas = 0;
    });

builder.AddAzureFunctionsProject<Projects.McpODataReporting>("mcpodatareporting")
    .WithReference(odataApi)
    .WaitFor(odataApi);
    
builder.Build().Run();
