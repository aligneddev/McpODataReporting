using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using McpODataReporting.Services;
using System.Text.Json;

namespace McpODataReporting.Tools;

public class DataAccess(
    ILogger<DataAccess> logger,
    IHttpClientFactory httpClientFactory,
    ODataMetadataService metadataService)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("ODataApi");

    [Function(nameof(GetData))]
    public async Task<HttpResponseData> GetData(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp/tools/GetData")] 
        HttpRequestData req)
    {
        try
        {
            logger.LogInformation("GetData tool invoked");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var requestData = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);

            // Extract the query parameter from the request
            if (requestData != null && requestData.TryGetValue("query", out var queryValue))
            {
                var query = queryValue?.ToString() ?? string.Empty;
                logger.LogInformation("Executing OData query: {Query}", query);

                // Ensure query starts with /
                if (!query.StartsWith('/'))
                {
                    query = "/" + query;
                }

                // Execute the OData query
                var response = await _httpClient.GetAsync($"/odata{query}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    logger.LogInformation("Query executed successfully");
                    
                    var httpResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                    httpResponse.Headers.Add("Content-Type", "application/json");
                    await httpResponse.WriteStringAsync(content);
                    return httpResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    logger.LogWarning("Query failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                    
                    var httpResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    httpResponse.Headers.Add("Content-Type", "application/json");
                    await httpResponse.WriteStringAsync(JsonSerializer.Serialize(new
                    {
                        error = $"Query failed with status {response.StatusCode}",
                        details = errorContent
                    }));
                    return httpResponse;
                }
            }

            logger.LogWarning("Query parameter not provided");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            errorResponse.Headers.Add("Content-Type", "application/json");
            await errorResponse.WriteStringAsync(JsonSerializer.Serialize(new
            {
                error = "'query' parameter not provided",
                expectedFormat = "/EntitySet?$filter=...&$select=..."
            }));
            return errorResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetData tool");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            errorResponse.Headers.Add("Content-Type", "application/json");
            await errorResponse.WriteStringAsync(JsonSerializer.Serialize(new
            {
                error = ex.Message
            }));
            return errorResponse;
        }
    }

    [Function("GetODataMetadata")]
    public async Task<HttpResponseData> GetODataMetadata(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp/tools/GetODataMetadata")] 
        HttpRequestData req)
    {
        try
        {
            logger.LogInformation("GetODataMetadata tool invoked");
            var description = await metadataService.GetToolDescriptionAsync();
            
            var httpResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
            httpResponse.Headers.Add("Content-Type", "text/plain");
            await httpResponse.WriteStringAsync(description);
            return httpResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetODataMetadata tool");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            errorResponse.Headers.Add("Content-Type", "application/json");
            await errorResponse.WriteStringAsync(JsonSerializer.Serialize(new
            {
                error = ex.Message
            }));
            return errorResponse;
        }
    }
}
