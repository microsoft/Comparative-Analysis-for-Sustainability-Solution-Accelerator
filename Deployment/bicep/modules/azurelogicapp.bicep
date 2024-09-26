// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

@description('Name of the Logic App')
param logicAppName string

@description('Location for the Logic App')
param location string = resourceGroup().location

@description('The definition of the Logic App workflow')
param logicAppSource object

@description('The parameter of the Logic App workflow')
param logicAppParameter object

resource logicApp 'Microsoft.Logic/workflows@2019-05-01' = {
  name: logicAppName
  location: location
  properties: {
    definition: logicAppSource
    parameters: logicAppParameter
  }
}

output logicAppName string = logicApp.name
output logicAppId string = logicApp.id
output logicAppEndpoint string = logicApp.properties.accessEndpoint

