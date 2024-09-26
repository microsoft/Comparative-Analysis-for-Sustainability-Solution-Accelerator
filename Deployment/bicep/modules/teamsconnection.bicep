// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

param location string = resourceGroup().location

resource teamsConnection 'Microsoft.Web/connections@2016-06-01' = {
  name: 'teams'
  location: location
  properties: {
    api: {
      id: subscriptionResourceId('Microsoft.Web/locations/managedApis', location, 'teams')
    }
    displayName: 'Teams connection'
    parameterValues: {
      token: ''
    }
  }
}

output teamsConnectionId string = teamsConnection.id
