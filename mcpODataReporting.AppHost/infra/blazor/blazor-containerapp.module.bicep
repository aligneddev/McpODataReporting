@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param appcontainerenv_outputs_azure_container_apps_environment_default_domain string

param appcontainerenv_outputs_azure_container_apps_environment_id string

param blazor_containerimage string

param blazor_containerport string

param azure_openai_endpoint_value string

param azure_openai_deployment_name_value string

param appcontainerenv_outputs_azure_container_registry_endpoint string

param appcontainerenv_outputs_azure_container_registry_managed_identity_id string

resource blazor 'Microsoft.App/containerApps@2025-02-02-preview' = {
  name: 'blazor'
  location: location
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: int(blazor_containerport)
        transport: 'http'
      }
      registries: [
        {
          server: appcontainerenv_outputs_azure_container_registry_endpoint
          identity: appcontainerenv_outputs_azure_container_registry_managed_identity_id
        }
      ]
      runtime: {
        dotnet: {
          autoConfigureDataProtection: true
        }
      }
    }
    environmentId: appcontainerenv_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: blazor_containerimage
          name: 'blazor'
          env: [
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EXCEPTION_LOG_ATTRIBUTES'
              value: 'true'
            }
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EVENT_LOG_ATTRIBUTES'
              value: 'true'
            }
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY'
              value: 'in_memory'
            }
            {
              name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
              value: 'true'
            }
            {
              name: 'HTTP_PORTS'
              value: blazor_containerport
            }
            {
              name: 'MCPODATAREPORTING_HTTP'
              value: 'http://mcpodatareporting.${appcontainerenv_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__mcpodatareporting__http__0'
              value: 'http://mcpodatareporting.${appcontainerenv_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'MCPODATAREPORTING_HTTPS'
              value: 'https://mcpodatareporting.${appcontainerenv_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__mcpodatareporting__https__0'
              value: 'https://mcpodatareporting.${appcontainerenv_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'AZURE_OPENAI_ENDPOINT'
              value: azure_openai_endpoint_value
            }
            {
              name: 'AZURE_OPENAI_DEPLOYMENT_NAME'
              value: azure_openai_deployment_name_value
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${appcontainerenv_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}