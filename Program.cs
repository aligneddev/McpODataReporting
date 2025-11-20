using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using McpODataReporting.Data;
using McpODataReporting.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Configure DbContext with Azure SQL Database
var connectionString = builder.Configuration["ConnectionStrings:ReportingDb"]
    ?? throw new Exception("Connection string 'ReportingDb' not found.");

builder.Services
    .AddDbContext<ReportingDbContext>(options =>
        options.UseSqlServer(connectionString,
            sqlOptions => sqlOptions.CommandTimeout(300)))
    .AddScoped<IODataQueryService, ODataQueryService>();

builder
    .Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
