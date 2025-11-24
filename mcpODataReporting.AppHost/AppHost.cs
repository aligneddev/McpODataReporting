var builder = DistributedApplication.CreateBuilder(args);

var odataApi = builder.AddProject<Projects.ODataApi>("odataapi");

builder.AddAzureFunctionsProject<Projects.McpODataReporting>("mcpodatareporting")
.WithReference(odataApi)
.WaitFor(odataApi);
    

builder.Build().Run();
