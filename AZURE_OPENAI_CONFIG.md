# Azure OpenAI Configuration for Deployment

This document explains how to configure Azure OpenAI settings for the mcpODataReporting application.

## Configuration Overview

The Azure OpenAI configuration is managed through Aspire's parameter system and will be automatically picked up by `azd` during deployment.

## Required Parameters

- `AZURE_OPENAI_ENDPOINT`: The endpoint URL of your Azure OpenAI resource
- `AZURE_OPENAI_DEPLOYMENT_NAME`: The name of your deployed model

## Local Development

For local development, the parameters are set in `mcpODataReporting.AppHost\appsettings.json`:

```json
{
  "Parameters": {
    "azure-openai-endpoint": "https://kl-demo-hub-resource.cognitiveservices.azure.com/",
    "azure-openai-deployment-name": "gpt-4.1-mini"
  }
}
```

## Azure Deployment with azd

### Setting Parameters

You can set these parameters using `azd env set`:

```bash
# Navigate to the AppHost directory
cd mcpODataReporting.AppHost

# Set the Azure OpenAI endpoint
azd env set AZURE_OPENAI_ENDPOINT "https://your-resource.cognitiveservices.azure.com/"

# Set the deployment name
azd env set AZURE_OPENAI_DEPLOYMENT_NAME "gpt-5-mini"
```

### Environment File

Alternatively, you can edit the environment file directly at:
```
mcpODataReporting.AppHost\.azure\{environment-name}\.env
```

Add these lines:
```env
AZURE_OPENAI_ENDPOINT="https://your-resource.cognitiveservices.azure.com/"
AZURE_OPENAI_DEPLOYMENT_NAME="gpt-5-mini"
```

## GitHub Actions

The GitHub Actions workflow will automatically use the parameters from your `azd` environment. Make sure to run `azd pipeline config` to set up the deployment pipeline.

The parameters will be:
1. Read from the `.azure/{environment}/.env` file
2. Injected into the Bicep template via `main.parameters.json`
3. Passed to the Blazor container app as environment variables
4. Used by the `IChatClient` to connect to Azure OpenAI

## Finding Your Azure OpenAI Resources

To find your Azure OpenAI resources, you can use the Azure CLI:

```bash
# List all Azure AI Services resources
az cognitiveservices account list --output table

# Get details for a specific resource
az cognitiveservices account show --name kl-demo-hub-resource --resource-group klAI

# List deployments
az cognitiveservices account deployment list --name kl-demo-hub-resource --resource-group klAI
```

## Current Configuration

Based on the discovered resources:

- **Resource Name**: kl-demo-hub-resource
- **Resource Group**: klAI
- **Endpoint**: https://kl-demo-hub-resource.cognitiveservices.azure.com/
- **Available Deployments**:
  - gpt-5-mini
  - text-embedding-3-small

## Troubleshooting

### Missing Configuration

If the configuration is not set, the Blazor app will log a warning and the chat functionality will not be available. Check the application logs for:

```
Azure OpenAI configuration not found. Please configure these values in AppHost appsettings.json or as azd parameters.
```

### Authentication Issues

The application uses `DefaultAzureCredential` for authentication. Ensure that:
1. The managed identity of the container app has the "Cognitive Services OpenAI User" role on your Azure OpenAI resource
2. For local development, you're signed in to Azure CLI: `az login`

## Security Considerations

- **Never commit** the `.env` file with real Azure OpenAI endpoints to source control
- The `.azure` directory is already in `.gitignore`
- Use managed identities for authentication instead of API keys
- Rotate credentials regularly if using API keys
