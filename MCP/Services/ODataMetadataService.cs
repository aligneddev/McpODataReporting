using System.Collections.Generic;
using System.Net.Mail;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Client;
using Microsoft.SqlServer.Server;
using static Azure.Core.HttpHeader;
using static Grpc.Core.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace McpODataReporting.Services;

public class ODataMetadataService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ODataMetadataService> _logger;
    private string? _cachedDescription;
    private DateTime _lastFetchTime = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public ODataMetadataService(IHttpClientFactory httpClientFactory, ILogger<ODataMetadataService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> GetToolDescriptionAsync()
    {
        // Return cached description if still valid
        if (_cachedDescription != null && DateTime.UtcNow - _lastFetchTime < _cacheExpiration)
        {
            return _cachedDescription;
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient("ODataApi");
            var response = await httpClient.GetAsync("/odata/$metadata");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch OData metadata: {StatusCode}", response.StatusCode);
                return GetFallbackDescription();
            }

            var metadataXml = await response.Content.ReadAsStringAsync();
            var description = ParseMetadataToDescription(metadataXml);

            _cachedDescription = description;
            _lastFetchTime = DateTime.UtcNow;

            return description;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching OData metadata");
            return GetFallbackDescription();
        }
    }

    private string ParseMetadataToDescription(string metadataXml)
    {
        try
        {
            var doc = XDocument.Parse(metadataXml);
            XNamespace edmx = "http://docs.oasis-open.org/odata/ns/edmx";
            XNamespace edm = "http://docs.oasis-open.org/odata/ns/edm";

            var sb = new StringBuilder();
            sb.AppendLine("An MCP tool that retrieves data from the database using OData queries.");
            sb.AppendLine();
            sb.AppendLine("## Available Entity Sets");
            sb.AppendLine();

            // Get the schema
            var schema = doc.Descendants(edm + "Schema").FirstOrDefault();
            if (schema == null)
            {
                return GetFallbackDescription();
            }

            // Get entity container with entity sets
            var entityContainer = schema.Descendants(edm + "EntityContainer").FirstOrDefault();
            var entitySets = entityContainer?.Descendants(edm + "EntitySet").ToList() ?? new List<XElement>();

            // Group entity sets by type (tables vs views)
            var tableEntitySets = new List<XElement>();
            var viewEntitySets = new List<XElement>();

            foreach (var entitySet in entitySets)
            {
                var name = entitySet.Attribute("Name")?.Value ?? "";
                if (name.StartsWith("V"))
                {
                    viewEntitySets.Add(entitySet);
                }
                else
                {
                    tableEntitySets.Add(entitySet);
                }
            }

            // Process core entities (tables)
            if (tableEntitySets.Any())
            {
                sb.AppendLine("### Core Entities");
                sb.AppendLine();

                foreach (var entitySet in tableEntitySets)
                {
                    AppendEntitySetDescription(sb, entitySet, schema, edm);
                }
            }

            // Process views
            if (viewEntitySets.Any())
            {
                sb.AppendLine("### Views");
                sb.AppendLine();

                foreach (var entitySet in viewEntitySets)
                {
                    AppendEntitySetDescription(sb, entitySet, schema, edm);
                }
            }

            // Add query capabilities section
            sb.AppendLine();
            sb.AppendLine(GetQueryCapabilitiesDescription());

            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing OData metadata");
            return GetFallbackDescription();
        }
    }

    private void AppendEntitySetDescription(StringBuilder sb, XElement entitySet, XElement schema, XNamespace edm)
    {
        var entitySetName = entitySet.Attribute("Name")?.Value;
        var entityTypeName = entitySet.Attribute("EntityType")?.Value?.Split('.').Last();

        if (string.IsNullOrEmpty(entitySetName) || string.IsNullOrEmpty(entityTypeName))
        {
            return;
        }

        sb.AppendLine($"**{entitySetName}**");

        // Find the entity type definition
        var entityType = schema.Descendants(edm + "EntityType")
            .FirstOrDefault(e => e.Attribute("Name")?.Value == entityTypeName);

        if (entityType != null)
        {
            // Get keys
            var keys = entityType.Descendants(edm + "Key")
                .SelectMany(k => k.Descendants(edm + "PropertyRef"))
                .Select(pr => pr.Attribute("Name")?.Value)
                .Where(n => n != null)
                .ToList();

            if (keys.Any())
            {
                sb.AppendLine($"   - Key: {string.Join(", ", keys)}");
            }

            // Get properties (limit to important ones, exclude keys)
            var properties = entityType.Descendants(edm + "Property")
                .Where(p => !keys.Contains(p.Attribute("Name")?.Value))
                .Take(10)
                .Select(p => new
                {
                    Name = p.Attribute("Name")?.Value,
                    Type = p.Attribute("Type")?.Value?.Replace("Edm.", "")
                })
                .Where(p => p.Name != null)
                .ToList();

            if (properties.Any())
            {
                var propertyNames = properties.Select(p => p.Name).ToList();
                sb.AppendLine($"   - Properties: {string.Join(", ", propertyNames)}");
            }

            // Get navigation properties
            var navProperties = entityType.Descendants(edm + "NavigationProperty")
                .Select(np => np.Attribute("Name")?.Value)
                .Where(n => n != null)
                .ToList();

            if (navProperties.Any())
            {
                sb.AppendLine($"   - Navigation: {string.Join(", ", navProperties)}");
            }
        }

        sb.AppendLine();
    }

    private string GetQueryCapabilitiesDescription()
    {
        return @"## OData Query Capabilities

The API supports the following OData query options (max top: 100):

- **$filter** - Filter results (e.g., Name eq 'John')
- **$select** - Choose specific properties (e.g., FirstName,LastName)
- **$orderby** - Sort results (e.g., ModifiedDate desc)
- **$expand** - Include related entities (e.g., Customer,Product)
- **$top** - Limit results (max 100)
- **$skip** - Skip results for pagination
- **$count** - Get total count of results

## Query Format

The query parameter should be in the format: /EntitySetName?$queryOptions

Examples:
- /Customers?$filter=City eq 'Seattle'&$select=FirstName,LastName,EmailAddress
- /Products?$filter=ListPrice gt 100&$orderby=ListPrice desc&$top=10
- /SalesOrderHeaders?$expand=Customer,SalesOrderDetails&$filter=OrderDate gt 2024-01-01

## Common Filter Operators

- eq (equal), ne (not equal)
- gt (greater than), ge (greater than or equal)
- lt (less than), le (less than or equal)
- and, or, not
- contains(field, 'value'), startswith(field, 'value'), endswith(field, 'value')";
    }

    private string GetFallbackDescription()
    {
        return @"An MCP tool that retrieves data from the database using OData queries.

## Available Entity Sets

The OData API exposes the following entity sets that can be queried:

### Core Entities

1. **Addresses** - Physical addresses
   - Key: AddressId (int)
   - Properties: AddressLine1, AddressLine2, City, StateProvince, CountryRegion, PostalCode

2. **Customers** - Customer information
   - Key: CustomerId (int)
   - Properties: FirstName, LastName, MiddleName, Title, Suffix, CompanyName, SalesPerson, EmailAddress, Phone
   - Navigation: CustomerAddresses, SalesOrderHeaders

3. **CustomerAddresses** - Links customers to addresses
   - Key: CustomerId, AddressId (composite)
   - Properties: AddressType
   - Navigation: Customer, Address

4. **Products** - Product catalog
   - Key: ProductId (int)
   - Properties: Name, ProductNumber, Color, StandardCost, ListPrice, Size, Weight, SellStartDate, SellEndDate
   - Navigation: ProductCategory, ProductModel, SalesOrderDetails

5. **ProductCategories** - Product category hierarchy
   - Key: ProductCategoryId (int)
   - Properties: Name, ParentProductCategoryId
   - Navigation: Products, ParentCategory, InverseParentCategory

6. **ProductModels** - Product model information
   - Key: ProductModelId (int)
   - Properties: Name, CatalogDescription, ModifiedDate
   - Navigation: Products, ProductModelProductDescriptions

7. **ProductDescriptions** - Product descriptions
   - Key: ProductDescriptionId (int)
   - Properties: Description, ModifiedDate
   - Navigation: ProductModelProductDescriptions

8. **ProductModelProductDescriptions** - Links product models to descriptions
   - Key: ProductModelId, ProductDescriptionId, Culture (composite)
   - Navigation: ProductModel, ProductDescription

9. **SalesOrderHeaders** - Sales order master records
   - Key: SalesOrderId (int)
   - Properties: OrderDate, DueDate, ShipDate, Status, SubTotal, TaxAmt, Freight, TotalDue, CustomerID, ShipToAddressID, BillToAddressID
   - Navigation: Customer, SalesOrderDetails, BillToAddress, ShipToAddress

10. **SalesOrderDetails** - Sales order line items
    - Key: SalesOrderId, SalesOrderDetailId (composite)
    - Properties: OrderQty, ProductID, UnitPrice, UnitPriceDiscount, LineTotal
    - Navigation: SalesOrderHeader, Product

### Views

11. **VGetAllCategories** - Flattened view of all product categories
    - Key: ProductCategoryId (int)
    - Properties: ParentProductCategoryName, ProductCategoryName

12. **VProductAndDescriptions** - Products with descriptions
    - Key: ProductId, Culture (composite)
    - Properties: Name, ProductModel, Description

13. **VProductModelCatalogDescriptions** - Product model catalog descriptions
    - Key: ProductModelId (int)
    - Properties: Name, Summary, Manufacturer, Copyright, ProductURL, WarrantyPeriod, WarrantyDescription

### System Entities

14. **BuildVersions** - Database version information
    - Key: SystemInformationId (int)
    - Properties: Database_Version, VersionDate, ModifiedDate

15. **ErrorLogs** - System error logs
    - Key: ErrorLogId (int)
    - Properties: ErrorTime, UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage

## OData Query Capabilities
    public const string GetDataToolDescription = Executes OData queries against the reporting database.

The API supports the following OData query options(max top: 100):

-**$filter * *-Filter results(e.g., Name eq 'John')
- **$select * *-Choose specific properties(e.g., FirstName, LastName)
-**$orderby * *-Sort results(e.g., ModifiedDate desc)
- **$expand * *-Include related entities(e.g., Customer, Product)
-**$top * *-Limit results(max 100)
- **$skip * *-Skip results for pagination
- **$count * *-Get total count of results

## Query Format

The query parameter should be in the format: / EntitySetName ?$queryOptions

Examples:
- / Customers ?$filter = City eq 'Seattle' &$select = FirstName, LastName, EmailAddress
## OData Query Options
- $filter - Filter results(e.g., Name eq 'John')
- $select - Choose specific properties(e.g., FirstName, LastName)
- $orderby - Sort results(e.g., ModifiedDate desc)
- $expand - Include related entities(e.g., Customer, Product)
- $top - Limit results(max 100)
- $skip - Skip results for pagination
- $count - Get total count of results

## Examples
- / Customers ?$filter = City eq 'Seattle' &$select = FirstName, LastName
- / Products ?$filter = ListPrice gt 100 &$orderby = ListPrice desc &$top = 10
- / SalesOrderHeaders ?$expand = Customer, SalesOrderDetails &$filter = OrderDate gt 2024 - 01 - 01
- / VProductAndDescriptions ?$filter = Culture eq 'en' &$select = Name, Description

## Common Filter Operators
- / SalesOrderHeaders ?$expand = Customer, SalesOrderDetails

- eq(equal), ne(not equal)
- gt(greater than), ge(greater than or equal)
- lt(less than), le(less than or equal)
- and, or, not
- contains(field, 'value'), startswith(field, 'value'), endswith(field, 'value')
## Important
Use the GetODataMetadata tool first to discover available entity sets and their properties before querying

For detailed entity information, please check the OData metadata endpoint: /odata/$metadata";
    }
}
