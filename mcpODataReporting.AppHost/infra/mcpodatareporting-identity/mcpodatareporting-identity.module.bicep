@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource mcpodatareporting_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: take('mcpodatareporting_identity-${uniqueString(resourceGroup().id)}', 128)
  location: location
}

output id string = mcpodatareporting_identity.id

output clientId string = mcpodatareporting_identity.properties.clientId

output principalId string = mcpodatareporting_identity.properties.principalId

output principalName string = mcpodatareporting_identity.name

output name string = mcpodatareporting_identity.name