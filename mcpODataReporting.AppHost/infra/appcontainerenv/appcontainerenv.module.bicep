@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param userPrincipalId string = ''

param tags object = { }

resource appcontainerenv_mi 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: take('appcontainerenv_mi-${uniqueString(resourceGroup().id)}', 128)
  location: location
  tags: tags
}

resource appcontainerenv_acr 'Microsoft.ContainerRegistry/registries@2025-04-01' = {
  name: take('appcontainerenvacr${uniqueString(resourceGroup().id)}', 50)
  location: location
  sku: {
    name: 'Basic'
  }
  tags: tags
}

resource appcontainerenv_acr_appcontainerenv_mi_AcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(appcontainerenv_acr.id, appcontainerenv_mi.id, subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d'))
  properties: {
    principalId: appcontainerenv_mi.properties.principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalType: 'ServicePrincipal'
  }
  scope: appcontainerenv_acr
}

resource appcontainerenv_law 'Microsoft.OperationalInsights/workspaces@2025-02-01' = {
  name: take('appcontainerenvlaw-${uniqueString(resourceGroup().id)}', 63)
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
  tags: tags
}

resource appcontainerenv 'Microsoft.App/managedEnvironments@2025-01-01' = {
  name: take('appcontainerenv${uniqueString(resourceGroup().id)}', 24)
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: appcontainerenv_law.properties.customerId
        sharedKey: appcontainerenv_law.listKeys().primarySharedKey
      }
    }
    workloadProfiles: [
      {
        name: 'consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
  tags: tags
}

resource aspireDashboard 'Microsoft.App/managedEnvironments/dotNetComponents@2024-10-02-preview' = {
  name: 'aspire-dashboard'
  properties: {
    componentType: 'AspireDashboard'
  }
  parent: appcontainerenv
}

output AZURE_LOG_ANALYTICS_WORKSPACE_NAME string = appcontainerenv_law.name

output AZURE_LOG_ANALYTICS_WORKSPACE_ID string = appcontainerenv_law.id

output AZURE_CONTAINER_REGISTRY_NAME string = appcontainerenv_acr.name

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = appcontainerenv_acr.properties.loginServer

output AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = appcontainerenv_mi.id

output AZURE_CONTAINER_APPS_ENVIRONMENT_NAME string = appcontainerenv.name

output AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = appcontainerenv.id

output AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = appcontainerenv.properties.defaultDomain