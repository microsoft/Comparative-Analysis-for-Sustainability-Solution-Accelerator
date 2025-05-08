// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

param location string = resourceGroup().location
param appInsightsName string = 'myAppInsights'
param logAnalyticsWorkspaceName string

// Define the Application Insights resource
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: log_analytic_sworkspace.id
  }
}

// Output the instrumentation key
output instrumentationKey string = appInsights.properties.InstrumentationKey
// Output the Application Insights resource name
output appInsightsName string = appInsights.name

resource log_analytic_sworkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' existing = {
  name: logAnalyticsWorkspaceName
  scope: resourceGroup()
}

