# OData Reporting MCP Function - Tool Metadata

## MCP Server Configuration

**Server Name**: `odata-reporting-mcp-function`  
**Type**: HTTP  
**URL**: `http://localhost:60165/runtime/webhooks/mcp/sse`  
**Package**: `io.github.alignedDev/mcpODataReporting`  
**Version**: `0.1.0-beta`

## Available Tools

### 1. GetODataMetadata

**Purpose**: Discovers the OData API schema and available entity sets

**Description**: 
Retrieves the OData API schema and available entity sets with their properties. Use this to discover what data is available before querying. Returns information about all entity sets including their keys, properties, and navigation relationships.

**Input Schema**:
```json
{
  "type": "object",
  "properties": {},
  "required": []
}
```

**Parameters**: None

**Returns**: 
A markdown-formatted string containing:
- List of all available entity sets (tables and views)
- Key fields for each entity
- Important properties for each entity
- Navigation properties (relationships)
- OData query capabilities documentation
- Query examples

**Example Usage**:
```json
{
  "tool": "GetODataMetadata",
  "arguments": {}
}
```

**Sample Response**:
```markdown
## Available Entity Sets

### Core Entities

**Customers**
   - Key: CustomerId
   - Properties: FirstName, LastName, EmailAddress, Phone, City...
   - Navigation: CustomerAddresses, SalesOrderHeaders

**Products**
   - Key: ProductId
   - Properties: Name, ProductNumber, ListPrice, Color...
   - Navigation: ProductCategory, ProductModel, SalesOrderDetails

### Views

**VProductAndDescriptions**
   - Key: ProductId, Culture
   - Properties: Name, ProductModel, Description

## OData Query Capabilities
...
```

---

### 2. GetData

**Purpose**: Executes OData queries against the reporting database

**Description**:
Executes OData queries against the reporting database.

**Query Format**: The query parameter should be in the format: `/EntitySetName?$queryOptions`

**OData Query Options**:
- `$filter` - Filter results (e.g., `Name eq 'John'`)
- `$select` - Choose specific properties (e.g., `FirstName,LastName`)
- `$orderby` - Sort results (e.g., `ModifiedDate desc`)
- `$expand` - Include related entities (e.g., `Customer,Product`)
- `$top` - Limit results (max 100)
- `$skip` - Skip results for pagination
- `$count` - Get total count of results

**Input Schema**:
```json
{
  "type": "object",
  "properties": {
    "query": {
      "type": "string",
      "description": "OData query string to execute against the reporting data source. Format: /EntitySetName?$queryOptions"
    }
  },
  "required": ["query"]
}
```

**Parameters**:
- `query` (string, required): OData query string
  - Format: `/EntitySetName?$queryOptions`
  - Example: `/Products?$filter=ListPrice gt 100&$top=10`

**Returns**: JSON response from the OData API containing the queried data

**Example Queries**:

1. **Get top 10 customers with email addresses**:
```json
{
  "tool": "GetData",
  "arguments": {
    "query": "/Customers?$filter=EmailAddress ne null&$select=FirstName,LastName,EmailAddress&$top=10"
  }
}
```

2. **Get expensive products**:
```json
{
  "tool": "GetData",
  "arguments": {
    "query": "/Products?$filter=ListPrice gt 100&$orderby=ListPrice desc&$top=10"
  }
}
```

3. **Get sales orders with customer details**:
```json
{
  "tool": "GetData",
  "arguments": {
    "query": "/SalesOrderHeaders?$expand=Customer&$filter=OrderDate gt 2024-01-01&$top=20"
  }
}
```

4. **Get top selling products** (by order quantity):
```json
{
  "tool": "GetData",
  "arguments": {
    "query": "/SalesOrderDetails?$apply=groupby((ProductID), aggregate(OrderQty with sum as TotalQuantity))&$orderby=TotalQuantity desc&$top=10"
  }
}
```
Note: This uses OData aggregation if enabled in the API.

Alternatively without aggregation:
```json
{
  "tool": "GetData",
  "arguments": {
    "query": "/SalesOrderDetails?$expand=Product&$select=ProductID,OrderQty,Product&$orderby=OrderQty desc&$top=50"
  }
}
```

5. **Count total customers**:
```json
{
  "tool": "GetData",
  "arguments": {
    "query": "/Customers/$count"
  }
}
```

## Common Filter Operators

- `eq` - Equal to
- `ne` - Not equal to
- `gt` - Greater than
- `ge` - Greater than or equal to
- `lt` - Less than
- `le` - Less than or equal to
- `and` - Logical AND
- `or` - Logical OR
- `not` - Logical NOT
- `contains(field, 'value')` - String contains
- `startswith(field, 'value')` - String starts with
- `endswith(field, 'value')` - String ends with

## Best Practices

1. **Discovery First**: Always call `GetODataMetadata` first to understand available entities and their properties
2. **Use $select**: Limit the properties returned to only what you need
3. **Use $top**: Limit result sets to avoid large payloads (max 100)
4. **Use $expand wisely**: Only expand navigation properties you need
5. **Filter effectively**: Use specific filters to reduce data transfer

## Workflow Example

### Step 1: Discover Schema
```json
{
  "tool": "GetODataMetadata",
  "arguments": {}
}
```

### Step 2: Query Based on Schema
```json
{
  "tool": "GetData",
  "arguments": {
    "query": "/Products?$filter=ProductCategoryId eq 18&$select=Name,ListPrice&$orderby=ListPrice desc"
  }
}
```

## Implementation Details

- **Cache Duration**: Metadata is cached for 30 minutes
- **Max Results**: Limited to 100 items per query via `$top`
- **Fallback**: If metadata service is unavailable, provides basic query instructions
- **Error Handling**: Returns detailed error messages from OData API
