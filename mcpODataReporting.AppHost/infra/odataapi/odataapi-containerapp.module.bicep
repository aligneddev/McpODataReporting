@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param appcontainerenv_outputs_azure_container_apps_environment_default_domain string

param appcontainerenv_outputs_azure_container_apps_environment_id string

param odataapi_containerimage string

param odataapi_containerport string

@secure()
param reportingdb_connectionstring string

param appcontainerenv_outputs_azure_container_registry_endpoint string

param appcontainerenv_outputs_azure_container_registry_managed_identity_id string

resource odataapi 'Microsoft.App/containerApps@2025-02-02-preview' = {
  name: 'odataapi'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'connectionstrings--reportingdb'
          value: reportingdb_connectionstring
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: int(odataapi_containerport)
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
          image: odataapi_containerimage
          name: 'odataapi'
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
              value: odataapi_containerport
            }
            {
              name: 'ConnectionStrings__ReportingDb'
              secretRef: 'connectionstrings--reportingdb'
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