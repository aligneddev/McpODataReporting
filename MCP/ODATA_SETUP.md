# OData Reporting MCP Tool - EF Core Integration

This MCP tool provides OData query capabilities for Azure SQL Database (`kl-mcp-reporting.database.windows.net`) using Entity Framework Core 9.0.

## Setup

### 1. Database Connection
The tool uses Azure Active Directory authentication. Update `local.settings.json`:

```json
{
  "ConnectionStrings": {
    "ReportingDb": "Server=kl-mcp-reporting.database.windows.net;Database=ReportingDb;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;Authentication=Active Directory Default;"
  }
}
```

### 2. Azure Authentication
Ensure you're logged in locally:
```bash
az login
```

### 3. Define Your Data Model
In `Data/ReportingDbContext.cs`, add DbSet properties for your tables:

```csharp
public DbSet<Product> Products { get; set; }
public DbSet<Order> Orders { get; set; }
```

### 4. Usage

Query format:
```
/EntitySet?$filter=...&$select=...&$top=...
```

Example OData queries:
- `/Products?$top=10` - Get top 10 products
- `/Orders?$filter=Status eq 'Completed'` - Filter orders
- `/Products?$select=Name,Price&$top=5` - Select specific columns

### 5. Extend OData Support

The `ODataQueryService` in `Services/ODataQueryService.cs` can be extended to:
- Parse OData $filter expressions
- Execute EF Core queries
- Handle $select, $top, $skip, $orderby
- Support complex queries

Current implementation provides query parsing and can be expanded with actual execution logic.

## Architecture

- **Program.cs**: DI configuration for DbContext and OData service
- **Data/ReportingDbContext.cs**: EF Core DbContext
- **Services/ODataQueryService.cs**: OData query parsing and execution
- **Tools/OdataTool.cs**: MCP tool function entry point
- **local.settings.json**: Local configuration with connection string

## Next Steps

1. Connect to your actual database and add table mappings
2. Implement OData query execution in `ODataQueryService`
3. Add authentication/authorization as needed
4. Deploy to Azure Functions
