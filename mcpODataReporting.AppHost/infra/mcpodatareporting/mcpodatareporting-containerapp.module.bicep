@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param appcontainerenv_outputs_azure_container_apps_environment_default_domain string

param appcontainerenv_outputs_azure_container_apps_environment_id string

param mcpodatareporting_containerimage string

param mcpodatareporting_identity_outputs_id string

param funcstorage987d1_outputs_blobendpoint string

param funcstorage987d1_outputs_queueendpoint string

param funcstorage987d1_outputs_tableendpoint string

param storage_outputs_blobendpoint string

param storage_outputs_queueendpoint string

param mcpodatareporting_identity_outputs_clientid string

param appcontainerenv_outputs_azure_container_registry_endpoint string

param appcontainerenv_outputs_azure_container_registry_managed_identity_id string

resource mcpodatareporting 'Microsoft.App/containerApps@2025-02-02-preview' = {
  name: 'mcpodatareporting'
  location: location
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
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
          image: mcpodatareporting_containerimage
          name: 'mcpodatareporting'
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
              name: 'FUNCTIONS_WORKER_RUNTIME'
              value: 'dotnet-isolated'
            }
            {
              name: 'AzureFunctionsJobHost__telemetryMode'
              value: 'OpenTelemetry'
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
            {
              name: 'AzureWebJobsStorage__blobServiceUri'
              value: funcstorage987d1_outputs_blobendpoint
            }
            {
              name: 'AzureWebJobsStorage__queueServiceUri'
              value: funcstorage987d1_outputs_queueendpoint
            }
            {
              name: 'AzureWebJobsStorage__tableServiceUri'
              value: funcstorage987d1_outputs_tableendpoint
            }
            {
              name: 'Aspire__Azure__Storage__Blobs__AzureWebJobsStorage__ServiceUri'
              value: funcstorage987d1_outputs_blobendpoint
            }
            {
              name: 'Aspire__Azure__Storage__Queues__AzureWebJobsStorage__ServiceUri'
              value: funcstorage987d1_outputs_queueendpoint
            }
            {
              name: 'Aspire__Azure__Data__Tables__AzureWebJobsStorage__ServiceUri'
              value: funcstorage987d1_outputs_tableendpoint
            }
            {
              name: 'ODATAAPI_HTTP'
              value: 'http://odataapi.internal.${appcontainerenv_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__odataapi__http__0'
              value: 'http://odataapi.internal.${appcontainerenv_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'ODATAAPI_HTTPS'
              value: 'https://odataapi.internal.${appcontainerenv_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__odataapi__https__0'
              value: 'https://odataapi.internal.${appcontainerenv_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'blobs__blobServiceUri'
              value: storage_outputs_blobendpoint
            }
            {
              name: 'blobs__queueServiceUri'
              value: storage_outputs_queueendpoint
            }
            {
              name: 'Aspire__Azure__Storage__Blobs__blobs__ServiceUri'
              value: storage_outputs_blobendpoint
            }
            {
              name: 'AZURE_TOKEN_CREDENTIALS'
              value: 'prod'
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: mcpodatareporting_identity_outputs_clientid
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
      '${mcpodatareporting_identity_outputs_id}': { }
      '${appcontainerenv_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
  kind: 'functionapp'
}