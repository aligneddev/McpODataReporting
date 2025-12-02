targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention, the name of the resource group for your application will use this name, prefixed with rg-')
param environmentName string

@minLength(1)
@description('The location used for all deployed resources')
param location string

@description('Id of the user or app to assign application roles')
param principalId string = ''

@secure()
param ReportingDb string

var tags = {
  'azd-env-name': environmentName
}

resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module appcontainerenv 'appcontainerenv/appcontainerenv.module.bicep' = {
  name: 'appcontainerenv'
  scope: rg
  params: {
    location: location
    userPrincipalId: principalId
  }
}
module funcstorage987d1 'funcstorage987d1/funcstorage987d1.module.bicep' = {
  name: 'funcstorage987d1'
  scope: rg
  params: {
    location: location
  }
}
module mcpodatareporting_identity 'mcpodatareporting-identity/mcpodatareporting-identity.module.bicep' = {
  name: 'mcpodatareporting-identity'
  scope: rg
  params: {
    location: location
  }
}
module mcpodatareporting_roles_funcstorage987d1 'mcpodatareporting-roles-funcstorage987d1/mcpodatareporting-roles-funcstorage987d1.module.bicep' = {
  name: 'mcpodatareporting-roles-funcstorage987d1'
  scope: rg
  params: {
    funcstorage987d1_outputs_name: funcstorage987d1.outputs.name
    location: location
    principalId: mcpodatareporting_identity.outputs.principalId
  }
}
output APPCONTAINERENV_AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = appcontainerenv.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
output APPCONTAINERENV_AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = appcontainerenv.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID
output APPCONTAINERENV_AZURE_CONTAINER_REGISTRY_ENDPOINT string = appcontainerenv.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
output APPCONTAINERENV_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = appcontainerenv.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID
output AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = appcontainerenv.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = appcontainerenv.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
output FUNCSTORAGE987D1_BLOBENDPOINT string = funcstorage987d1.outputs.blobEndpoint
output FUNCSTORAGE987D1_QUEUEENDPOINT string = funcstorage987d1.outputs.queueEndpoint
output FUNCSTORAGE987D1_TABLEENDPOINT string = funcstorage987d1.outputs.tableEndpoint
output MCPODATAREPORTING_IDENTITY_CLIENTID string = mcpodatareporting_identity.outputs.clientId
output MCPODATAREPORTING_IDENTITY_ID string = mcpodatareporting_identity.outputs.id
