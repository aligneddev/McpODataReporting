# MCP OData Reporting Example

A Model Context Protocol (MCP) server that provides AI assistants with the ability to query reporting databases through OData APIs. This solution demonstrates how to expose database reporting capabilities to AI tools using the MCP standard.

## Overview

This solution consists of three main components:

1. **ODataApi** - An ASP.NET Core Web API that exposes database entities through OData endpoints
2. **McpODataReporting** - An Azure Functions-based MCP server that provides tools for querying the OData API
3. **mcpODataReporting.AppHost** - An Aspire application host for orchestrating the distributed application

## Architecture

```
┌─────────────────┐         ┌──────────────────┐         ┌─────────────────┐
│   AI Assistant  │ ◄─MCP─► │ McpODataReporting│ ◄─HTTP─►│   ODataApi      │
│   (e.g. Claude) │         │  (MCP Server)    │         │  (OData Service)│
└─────────────────┘         └──────────────────┘         └─────────────────┘
                                                                    │
                                                                    ▼
                                                           ┌─────────────────┐
                                                           │   SQL Server    │
                                                           │   (ReportingDb) │
                                                           └─────────────────┘
```

## Features

### OData API
- Full OData v4 support with query capabilities (`$filter`, `$select`, `$orderby`, `$expand`, `$top`, `$skip`, `$count`)
- Entity Framework Core with SQL Server
- Supports multiple entities including:
  - Customers, Products, Orders
  - Product Categories and Models
  - Sales Order Headers and Details
  - Addresses and more
- Database views support for complex queries
- OpenAPI/Swagger documentation

### MCP Server
- Two main tools for AI assistants:
  - **GetODataMetadata** - Discovers available entity sets and their properties
  - **GetData** - Executes OData queries against the reporting database
- Built on Azure Functions with MCP SDK
- Configurable HTTP client with extended timeout support
- Application Insights integration for monitoring

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or full version)
- [Azure Functions Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local) (for local development)
- An MCP-compatible AI client (e.g., Claude Desktop)

## Getting Started

### 1. Database Setup

Set up your SQL Server database with the AdventureWorksLT sample schema. Update the connection string in your configuration:

```json
{
  "ConnectionStrings": {
    "ReportingDb": "Server=(localdb)\\mssqllocaldb;Database=AdventureWorksLT;Trusted_Connection=True;"
  }
}
```

I created a new Azure SQL Database and used the sample data from Microsoft of the AdventureWorksLT database.

### 2. Running with .NET Aspire

The easiest way to run the entire solution is using the Aspire App Host:

```bash
cd mcpODataReporting.AppHost
dotnet run
```

This will start both the OData API and the MCP server with proper service discovery and orchestration.

### 3. Running Components Individually

**OData API:**
```bash
cd ODataApi
dotnet run
```

The API will be available at `https://localhost:7066` (or as configured).

**MCP Server:**
```bash
cd MCP
dotnet run
```

## Using the MCP Server

### Configuration

Configure your AI client (e.g., Claude Desktop) to connect to the MCP server. Add the following to your MCP settings:

```json
{
  "mcpServers": {
    "odata-reporting": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/MCP/McpODataReporting.csproj"]
    }
  }
}
```

Or register the MCP server endpoint from runing locally:
```json

    "odata-reporting-mcp-function": {
      "type": "http",
      "url": "http://localhost:63930/runtime/webhooks/mcp/sse"
    },
```
or pointing to the Azure Function
```json

    "odata-reporting-mcp-function": {
      "type": "http",
      "url": "https://mcpodatareporting.internal.{more}.eastus.azurecontainerapps.io/runtime/webhooks/mcp/sse"
    },
```
then VS prompts with "Authentication Required" 
### Available MCP Tools

#### GetODataMetadata
Retrieves the schema of available entities and their properties.

**Example Usage:**
```
Please show me what data is available in the reporting database.
```

#### GetData
Executes OData queries to retrieve data.

**Example Usage:**
```
Get all customers from Seattle
Get the top 10 most expensive products
Show me sales orders with customer details
```

**Query Format:**
```
/EntitySetName?$filter=Field eq 'Value'&$select=Field1,Field2&$top=10
```

**Examples:**
- `/Customers?$filter=City eq 'Seattle'&$select=FirstName,LastName,EmailAddress`
- `/Products?$filter=ListPrice gt 100&$orderby=ListPrice desc&$top=10`
- `/SalesOrderHeaders?$expand=Customer,SalesOrderDetails&$top=5`

## OData Query Options

| Option | Description | Example |
|--------|-------------|---------|
| `$filter` | Filter results | `City eq 'Seattle'` |
| `$select` | Choose specific properties | `FirstName,LastName` |
| `$orderby` | Sort results | `ModifiedDate desc` |
| `$expand` | Include related entities | `Customer,Product` |
| `$top` | Limit results (max 100) | `10` |
| `$skip` | Skip results for pagination | `20` |
| `$count` | Get total count | `true` |

## Available Entities

### Core Tables
- **Customers** - Customer information
- **Products** - Product catalog
- **ProductCategories** - Product category hierarchy
- **ProductModels** - Product model information
- **SalesOrderHeaders** - Sales order master records
- **SalesOrderDetails** - Sales order line items
- **Addresses** - Address information
- **CustomerAddresses** - Customer-Address relationships

### Views
- **VGetAllCategories** - Flattened product category view
- **VProductAndDescriptions** - Products with descriptions
- **VProductModelCatalogDescriptions** - Detailed product model catalog

## Project Structure

```
mcpODataReporting/
├── ODataApi/                      # OData Web API
│   ├── Data/
│   │   ├── ReportingDbContext.cs  # EF Core DbContext
│   │   └── [Entity Models]        # Database entity classes
│   ├── Controllers/               # OData controllers
│   └── Program.cs                 # API startup
├── MCP/                           # MCP Server (Azure Functions)
│   ├── Tools/
│   │   └── OdataTool.cs          # MCP tool implementations
│   ├── Services/
│   │   └── ODataMetadataService.cs # Metadata discovery service
│   └── Program.cs                 # Function app startup
├── mcpODataReporting.AppHost/     # Aspire App Host
│   └── AppHost.cs                 # Orchestration configuration
└── mcpODataReporting.ServiceDefaults/ # Shared service defaults
```

## Development

### Building the Solution

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Adding New Entities

1. Add the entity class to `ODataApi/Data/`
2. Add the `DbSet<T>` to `ReportingDbContext`
3. Register the entity set in `Program.cs`:
   ```csharp
   odataBuilder.EntitySet<YourEntity>("YourEntities");
   ```

## Deployment

deploy to Azure following: https://learn.microsoft.com/en-us/dotnet/aspire/deployment/azd/aca-deployment-github-actions?tabs=windows&pivots=github-actions

Since this is using Aspire, you must be in the \mcpODataReporting.AppHost directory or you will have problems and it won't create a infr/main.bicep file and azd up will fail.
the .azure folder will be in this folder as well.

`azd init` to initialize the project

`azd pipeline config` setup GitHub Actions for CI/CD  (run this again after adding new or updating existing environment variables). It will push the values into the GitHub Secrets/Variables for you.

`azd infra gen` will create the bicep file in the `infra` directory 
`azd up` to create the infrastructure and provision, but can be separated to provision and deploy
  `azd provision` it will prompt you for your Azure SQL connection string and create a new Resource Group
  `azd deploy` to deploy the application


### Publishing the MCP Server

The MCP server is configured to be packaged as a self-contained, single-file executable:

```bash
cd MCP
dotnet publish -c Release
```

The published package can be distributed as a NuGet package with package type `McpServer`.

### Deploying the OData API

The OData API can be deployed to any .NET hosting environment:

- Azure App Service
- Azure Container Apps
- Docker containers
- On-premises IIS

## Configuration

### App Settings

**ODataApi/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "ReportingDb": "Server=...;Database=...;"
  }
}
```

**MCP Configuration:**
Set the OData API base URL via configuration or environment variable:
```json
{
  "ODataApi": {
    "BaseUrl": "https://localhost:7066"
  }
}
```

## Monitoring

The solution includes Application Insights integration for monitoring:

- Request/response logging
- Performance metrics
- Error tracking
- Custom telemetry

## Security Considerations

- **Authentication**: Currently configured for development. Implement proper authentication (Azure AD, API keys, etc.) for production
- **Authorization**: Add authorization policies to control access to sensitive data
- **Connection Strings**: Use Azure Key Vault or secure configuration providers for production
- **HTTPS**: Always use HTTPS in production environments
- **Rate Limiting**: Consider implementing rate limiting on the OData API

## License

This project is provided as-is for demonstration and educational purposes.

## Resources

- [Model Context Protocol Documentation](https://modelcontextprotocol.io/)
- [OData v4 Documentation](https://www.odata.org/documentation/)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Azure Functions Documentation](https://learn.microsoft.com/azure/azure-functions/)
- [Entity Framework Core Documentation](https://learn.microsoft.com/ef/core/)
