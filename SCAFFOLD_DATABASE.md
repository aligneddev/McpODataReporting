# How to Scaffold Your Database Schema

Since you have an existing Azure SQL Database, use one of these approaches:

## Option 1: EF Core CLI (Recommended for .NET 10)

```bash
# Install EF Core CLI globally (if not already installed)
dotnet tool install --global dotnet-ef

# Navigate to your project directory
cd C:\git\ai\mcpODataReporting

# Scaffold the DbContext from your database
dotnet ef dbcontext scaffold `
  "Server=kl-mcp-reporting.database.windows.net;Database=kl-mcpReporting;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;Authentication=Active Directory Default;" `
  Microsoft.EntityFrameworkCore.SqlServer `
  --output-dir Data `
  --context ReportingDbContext `
  --force
```

This command will:
- Connect to your Azure SQL Database using Azure AD authentication
- Read all tables, views, and columns
- Generate C# model classes for each table
- Update ReportingDbContext with DbSet<T> properties

## Option 2: Visual Studio Power Tools

1. Install "EF Core Power Tools" extension
2. Right-click on your DbContext ? "EF Core Power Tools" ? "Reverse Engineer..."
3. Provide connection string
4. Select tables to include
5. Generate models and DbContext

## Option 3: Manual Approach

If you know your table schema, provide the following information:
- Table names
- Column names and data types
- Primary keys
- Foreign key relationships

Then I'll create the model classes manually.

## Connection String Note

Your connection string uses Azure AD authentication:
```
Server=kl-mcp-reporting.database.windows.net;
Database=kl-mcpReporting;
Encrypt=true;
TrustServerCertificate=false;
Connection Timeout=30;
Authentication=Active Directory Default;
```

This requires you to be logged in via `az login`.
