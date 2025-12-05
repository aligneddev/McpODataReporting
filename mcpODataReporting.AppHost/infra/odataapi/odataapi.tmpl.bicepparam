using './odataapi-containerapp.module.bicep'

param appcontainerenv_outputs_azure_container_apps_environment_default_domain = '{{ .Env.APPCONTAINERENV_AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN }}'
param appcontainerenv_outputs_azure_container_apps_environment_id = '{{ .Env.APPCONTAINERENV_AZURE_CONTAINER_APPS_ENVIRONMENT_ID }}'
param appcontainerenv_outputs_azure_container_registry_endpoint = '{{ .Env.APPCONTAINERENV_AZURE_CONTAINER_REGISTRY_ENDPOINT }}'
param appcontainerenv_outputs_azure_container_registry_managed_identity_id = '{{ .Env.APPCONTAINERENV_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID }}'
param odataapi_containerimage = '{{ .Image }}'
param odataapi_containerport = '{{ targetPortOrDefault 8080 }}'
param reportingdb_connectionstring = '{{ securedParameter "ReportingDb" }}'
