# Aspire Sticky Ports Configuration

## Overview
This document describes the sticky port configuration for the mcpODataReporting Aspire project. Sticky ports ensure that services use consistent port numbers across local development runs.

## Port Assignments

| Service | HTTP Port | HTTPS Port | Description |
|---------|-----------|------------|-------------|
| **blazor** | 5000 | 5001 | Blazor Web App - Main UI for OData Reporting |
| **mcpodatareporting** | 7071 | N/A | Azure Functions - MCP Tools and OData Tool Implementation |
| **odataapi** | 8000 | 8001 | OData API - Backend data source |

## Configuration Details

### 1. AppHost.cs
Each service is configured with explicit HTTP/HTTPS endpoints using `WithHttpEndpoint()` and `WithHttpsEndpoint()` methods:

```csharp
.WithHttpEndpoint(port: 5000, name: "http")
.WithHttpsEndpoint(port: 5001, name: "https")
```

### 2. appsettings.Development.json
Sticky ports are enabled globally for the Aspire Hosting DCP (Distributed Cloud Platform):

```json
"Aspire": {
  "Hosting": {
    "DcpDefaults": {
      "StickyPorts": true
    }
  }
}
```

### 3. .aspirerc.json
This file documents the port mappings for reference and IDE support.

## Benefits

- **Consistency**: Services always run on the same ports across development sessions
- **Easier Debugging**: No need to update connection strings or URLs when ports change
- **Better Testing**: Integration tests can rely on fixed ports
- **Documentation**: Port assignments are explicit and documented

## Service Endpoints

### Local Development URLs

- **Blazor Web App**: 
  - HTTP: `http://localhost:5000`
  - HTTPS: `https://localhost:5001`

- **OData API**: 
  - HTTP: `http://localhost:8000`
  - HTTPS: `https://localhost:8001`

- **Azure Functions (MCP)**: 
  - HTTP: `http://localhost:7071`
  - API Base: `http://localhost:7071/api`

## Service Discovery

Services use Aspire's service discovery to communicate internally:
- `http://blazor` - resolves to Blazor service
- `http://odataapi` - resolves to OData API service
- `http://mcpodatareporting` - resolves to Azure Functions service

## Changing Port Numbers

To modify port assignments:

1. Update the port numbers in `AppHost.cs` using `WithHttpEndpoint()` and `WithHttpsEndpoint()`
2. Update this documentation with the new port numbers
3. Update `.aspirerc.json` if using IDE extensions that support it
4. Update any hardcoded localhost URLs in integration tests

Example:
```csharp
.WithHttpEndpoint(port: 3000, name: "http")
.WithHttpsEndpoint(port: 3001, name: "https")
```

## Troubleshooting

### Ports Already in Use
If you encounter "port already in use" errors:

1. Stop the Aspire orchestrator: Press Ctrl+C in the Aspire dashboard terminal
2. Wait 10-15 seconds for ports to be released
3. Restart the orchestrator

### Force Port Reset
To force a full port reset (use with caution):

1. Delete the Aspire DCP state: `~\.aspire\dcp` (on Windows) or `~/.aspire/dcp` (on macOS/Linux)
2. Restart the orchestrator

### Sticky Ports Not Working
If ports are not persisting:

1. Verify `StickyPorts` is set to `true` in `appsettings.Development.json`
2. Ensure you're running in Development environment
3. Check Aspire logs for any configuration errors

## References

- [Aspire Hosting Overview](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/app-host-overview)
- [Aspire Service Discovery](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-discovery)
