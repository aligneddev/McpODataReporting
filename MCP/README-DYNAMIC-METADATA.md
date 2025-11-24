# Dynamic OData MCP Tool Documentation

## Overview
The MCP tool now dynamically discovers the OData API schema and provides comprehensive metadata about available entities and query capabilities.

## Implementation

### 1. ODataMetadataService
**File**: `MCP\Services\ODataMetadataService.cs`

This service:
- Fetches the OData `$metadata` endpoint at `/odata/$metadata`
- Parses the EDM XML to extract entity information
- Caches the metadata for 30 minutes to avoid repeated calls
- Provides fallback description if metadata fetch fails

**Key Features**:
- Automatically discovers all EntitySets
- Extracts keys, properties, and navigation properties for each entity
- Groups entities into "Core Entities" (tables) and "Views"
- Includes comprehensive query capability documentation

### 2. Two MCP Tools

#### GetODataMetadata Tool
**Purpose**: Allows the LLM to discover the schema before querying

```csharp
[Function("GetODataMetadata")]
public async Task<string> GetODataMetadata(...)
```

**What it returns**:
- Complete list of all available entity sets
- Key fields for each entity
- Available properties (up to 10 main properties shown)
- Navigation properties for related entities
- OData query capabilities ($filter, $select, etc.)
- Usage examples

#### GetData Tool
**Purpose**: Executes OData queries

```csharp
[Function(nameof(GetData))]
public async Task<string> GetData(...)
```

**Parameters**:
- `query`: OData query string (e.g., `/Customers?$filter=City eq 'Seattle'`)

## Usage Flow

1. **LLM calls GetODataMetadata**: Discovers available entities, their properties, and relationships
2. **LLM formulates appropriate OData query**: Based on discovered schema
3. **LLM calls GetData with query**: Executes the query and gets results

## Example Workflow

### Step 1: Discover Schema
```
Tool: GetODataMetadata
Returns:
  - Customers (Key: CustomerId, Properties: FirstName, LastName, EmailAddress...)
  - Products (Key: ProductId, Properties: Name, ListPrice, Color...)
  - SalesOrderHeaders (Key: SalesOrderId, Navigation: Customer, SalesOrderDetails...)
```

### Step 2: Query Data
```
Tool: GetData
Parameter: "/Customers?$filter=EmailAddress ne null&$select=FirstName,LastName,EmailAddress&$top=10"
Returns: JSON array of customer data
```

## Benefits

? **Always Up-to-Date**: Automatically reflects schema changes in OData API
? **No Manual Maintenance**: No need to manually update entity documentation
? **Comprehensive**: Shows keys, properties, and relationships
? **Smart Caching**: 30-minute cache prevents excessive metadata calls
? **Error Handling**: Falls back to basic description if metadata unavailable
? **Discovery-First**: LLM can discover schema before querying

## Configuration

### Program.cs Setup
```csharp
// HttpClient for OData API
builder.Services.AddHttpClient("ODataApi", client =>
{
    var odataBaseUrl = builder.Configuration["ODataApi:BaseUrl"] ?? "https://localhost:7066";
    client.BaseAddress = new Uri(odataBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register metadata service
builder.Services.AddSingleton<ODataMetadataService>();
```

## Sample Metadata Output

```markdown
## Available Entity Sets

### Core Entities

**Customers**
   - Key: CustomerId
   - Properties: NameStyle, Title, FirstName, MiddleName, LastName, Suffix, CompanyName, SalesPerson, EmailAddress, Phone
   - Navigation: CustomerAddresses, SalesOrderHeaders

**Products**
   - Key: ProductId
   - Properties: Name, ProductNumber, Color, StandardCost, ListPrice, Size, Weight, SellStartDate, SellEndDate
   - Navigation: ProductCategory, ProductModel, SalesOrderDetails

### Views

**VProductAndDescriptions**
   - Key: ProductId, Culture
   - Properties: Name, ProductModel, Description

## OData Query Capabilities
- $filter - Filter results (e.g., Name eq 'John')
- $select - Choose specific properties
- $orderby - Sort results
...
```

## Notes

?? **Application Restart Required**: Changes to const fields require restarting the application (not hot-reload compatible)

?? **Port Configuration**: Update the OData base URL in configuration if using different port than 7066
