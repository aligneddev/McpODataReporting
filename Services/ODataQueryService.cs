using System.Dynamic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using McpODataReporting.Data;

namespace McpODataReporting.Services;

public interface IODataQueryService
{
    Task<string> ExecuteQueryAsync(string query);
}

public class ODataQueryService : IODataQueryService
{
    private readonly ReportingDbContext _dbContext;
    private readonly ILogger<ODataQueryService> _logger;

    public ODataQueryService(ReportingDbContext dbContext, ILogger<ODataQueryService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<string> ExecuteQueryAsync(string query)
    {
        try
        {
            _logger.LogInformation("Executing OData query: {Query}", query);

            // Parse basic OData query syntax
            var result = await ParseAndExecuteODataQuery(query);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing OData query: {Query}", query);
            return $"Error: {ex.Message}";
        }
    }

    private async Task<string> ParseAndExecuteODataQuery(string query)
    {
        // Parse OData query format: /EntitySet?$filter=...&$select=...&$top=...
        // Example: /Products?$filter=Price gt 100&$select=Name,Price&$top=10
        
        var parts = query.Split('?');
        if (parts.Length != 2)
        {
            return "Invalid OData query format. Expected: /EntitySet?$filter=...";
        }

        var entitySet = parts[0].Trim('/');
        var queryParams = parts[1];

        // For now, return a structured response with query info
        // This can be expanded to execute actual EF Core queries against your database
        var response = new
        {
            entitySet,
            queryParams,
            message = $"OData query would execute against '{entitySet}' with parameters: {queryParams}",
            timestamp = DateTime.UtcNow,
            note = "Connect your database schema and DbSet properties to execute actual queries"
        };

        return System.Text.Json.JsonSerializer.Serialize(response, 
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }
}
