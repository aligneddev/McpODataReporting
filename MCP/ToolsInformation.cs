namespace McpODataReporting;

internal sealed class ToolsInformation
{
    public const string GetDataPropertyName = "query";
    public const string PropertyType = "string";
    public const string GetDataToolName = "GetData";
    public const string GetDataToolDescription = @"Executes OData queries against the reporting database.

    ## Query Format
    The query parameter should be in the format: /EntitySetName?$queryOptions

    ## OData Query Options
    - $filter - Filter results (e.g., Name eq 'John')
    - $select - Choose specific properties (e.g., FirstName,LastName)
    - $orderby - Sort results (e.g., ModifiedDate desc)
    - $expand - Include related entities (e.g., Customer,Product)
    - $top - Limit results (max 100)
    - $skip - Skip results for pagination
    - $count - Get total count of results

    ## Examples
    - /Customers?$filter=City eq 'Seattle'&$select=FirstName,LastName
    - /Products?$filter=ListPrice gt 100&$orderby=ListPrice desc&$top=10
    - /SalesOrderHeaders?$expand=Customer,SalesOrderDetails

    ## Important
    Use the GetODataMetadata tool first to discover available entity sets and their properties before querying.";
}
