param logAnalyticsWorkspaceName string
param location string
resource log_analytics_workspace 'Microsoft.OperationalInsights/workspaces@2020-08-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  retentionInDays: 30
  }
}


output log_analytics_workspace_name string = log_analytics_workspace.name
