// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

targetScope = 'subscription'

var resourceprefix = padLeft(take(uniqueString(deployment().name), 5), 5, '0')
param appname string = 'esgdocanalysis'

param gpt4 object = {
      name: 'gpt-4o'
      version: '2024-05-13'
      raiPolicyName: ''
      capacity: 1
      scaleType: 'Standard'
    }
param gpt4_32k object = {
      name: 'gpt-4-32k'
      version: '0613'
      raiPolicyName: ''
      capacity: 1
      scaleType: 'Standard'
    }
param textembedding object = {
      name: 'text-embedding-3-large'
      version: '1'
      raiPolicyName: ''
      capacity: 1
      scaleType: 'Standard'
    }

// Create a resource group
resource gs_resourcegroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg${appname}${resourceprefix}'
  location: deployment().location
}

param aksVersion string = '1.30.7'
// /*
module gs_aks 'modules/azurekubernetesservice.bicep' = {
  name: 'aks${appname}${resourceprefix}'
  scope: gs_resourcegroup
  params: {
    aksName: 'aks${appname}${resourceprefix}'
    aksVersion:aksVersion
    aksAgentPoolCount:3
    location: deployment().location
  }
  dependsOn: [
    gs_appinsights
  ]
}

// Create Container Registry
module gs_containerregistry 'modules/azurecontainerregistry.bicep' = {
  name: 'acr${appname}${resourceprefix}'
  scope: gs_resourcegroup
  params: {
    acrName: '${appname}${resourceprefix}'
    location: deployment().location
  }
  dependsOn: [
    gs_aks
  ]
}
// */

// /*
// Create a storage account
module gs_storageaccount 'modules/azurestorageaccount.bicep' = {
  name: 'sa${appname}${resourceprefix}'
  scope: gs_resourcegroup
  params: {
    storageAccountName: '${appname}${resourceprefix}'
    location: deployment().location
  }
}
// */

// /*
// Create a Azure Search Service
module gs_azsearch 'modules/azuresearch.bicep' = {
  name: 'search${appname}${resourceprefix}'
  scope: gs_resourcegroup
  params: {
    searchServiceName: '${appname}${resourceprefix}'
    location: deployment().location
  }
}
// */

// /*
// Create Azure Cognitive Service
module gs_azcognitiveservice 'modules/azurecognitiveservice.bicep' = {
  name: 'cognitiveservice${appname}${resourceprefix}'
  scope: gs_resourcegroup
  params: {
    cognitiveServiceName: '${appname}${resourceprefix}'
    location: deployment().location
  }
}
// */

// /*
// Create Azure Open AI Service
module gs_openaiservice 'modules/azureopenaiservice.bicep' = {
  name: 'openaiservice${appname}${resourceprefix}'
  scope: gs_resourcegroup
  params: {
    prefix: resourceprefix
    // GPT-4-32K model & GPT-4o available Data center information.
    // https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models#gpt-4    
    location: deployment().location
    gpt4:gpt4
    gpt4_32k:gpt4_32k
    textembedding:textembedding
  }
}
// */

// /*
// Create Teams Connection for Logic App
module gs_teamsconnection 'modules/teamsconnection.bicep' = {
  name: 'teams'
  scope: gs_resourcegroup
  params: {
    location: deployment().location
  }
}
// */

// /*
var docregistprocesswatcher = (loadTextContent('logicapp/documentprocesswatcher.json'))
var replacedDocregistprocesswatcher = json(replace(
  replace(
    replace(docregistprocesswatcher, '{{ subscriptionId }}', subscription().subscriptionId),
    '{{ resourceGroup }}',
    gs_resourcegroup.name
  ),
  '{{ location }}',
  deployment().location
))

// Create Logic App - Document Registration Process Watcher
module gs_logicapp_docregistprocesswatcher 'modules/azurelogicapp.bicep' = {
  name: 'logicapp-docregistprocesswatcher${resourceprefix}'
  scope: gs_resourcegroup

  params: {
    logicAppName: 'docregistprocesswatcher${appname}${resourceprefix}'
    location: deployment().location
    logicAppSource: replacedDocregistprocesswatcher.definition
    logicAppParameter: replacedDocregistprocesswatcher.parameters
  }

  dependsOn: [
    gs_teamsconnection
  ]
}
// */

// /*
var benchmarkprocesswatcher = (loadTextContent('logicapp/bechmarkprocesswatcher.json'))
var replacedbenchmarkprocesswatcher = json(replace(
  replace(
    replace(benchmarkprocesswatcher, '{{ subscriptionId }}', subscription().subscriptionId),
    '{{ resourceGroup }}',
    gs_resourcegroup.name
  ),
  '{{ location }}',
  deployment().location
))

// Create Logic App - Benchmark Service Process Watcher
module gs_logicapp_benchmarkprocesswatcher 'modules/azurelogicapp.bicep' = {
  name: 'logicapp-benchmarkprocesswatcher${appname}${resourceprefix}'
  scope: gs_resourcegroup
  params: {
    logicAppName: 'benchmarkprocesswatcher${appname}${resourceprefix}'
    location: deployment().location
    logicAppSource: replacedbenchmarkprocesswatcher.definition
    logicAppParameter: replacedbenchmarkprocesswatcher.parameters
  }

  dependsOn: [
    gs_teamsconnection
  ]
}
// */

// /*
var gapanalysisprocesswatcher = (loadTextContent('logicapp/gapanalysisprocesswatcher.json'))
var replacedgapanalysisprocesswatcher = json(replace(
  replace(
    replace(gapanalysisprocesswatcher, '{{ subscriptionId }}', subscription().subscriptionId),
    '{{ resourceGroup }}',
    gs_resourcegroup.name
  ),
  '{{ location }}',
  deployment().location
))

// // Create Logic App - GapAnalysis Service Process Watcher
module gs_logicapp_ProcessWatcher 'modules/azurelogicapp.bicep' = {
  name: 'logicapp-gapanalysisprocesswatcher${appname}${resourceprefix}'
  scope: gs_resourcegroup
  params: {
    logicAppName: 'gapanalysisprocesswatcher${appname}${resourceprefix}'
    location: deployment().location
    logicAppSource: replacedgapanalysisprocesswatcher.definition
    logicAppParameter: replacedgapanalysisprocesswatcher.parameters
  }

  dependsOn: [
    gs_teamsconnection
  ]
}
// */

// /*
// Create Azure Cosmos DB Mongo
module gs_cosmosdb 'modules/azurecosmosdb.bicep' = {
  name: 'cosmosdb${appname}${resourceprefix}'
  scope: gs_resourcegroup
  params: {
    cosmosDbAccountName: 'cosmosdb${appname}${resourceprefix}'
    location: deployment().location
  }
}
// */

// /*
// Create Azure App Insights
module gs_appinsights 'modules/azureappingisht.bicep' = {
  name: 'appinsights${appname}${resourceprefix}'
  scope: gs_resourcegroup
  params: {
    appInsightsName: 'appinsights${appname}${resourceprefix}'
    location: deployment().location
    logAnalyticsWorkspaceName:'la${appname}${resourceprefix}'
  }

  dependsOn: [
    gs_log_analytics_workspace
  ]
}
// */

// /*
// Create Azure Log Analytics
module gs_log_analytics_workspace 'modules/azureloganalytics.bicep' = {
  name: 'la${appname}${resourceprefix}'
  scope: gs_resourcegroup
  params: {
    logAnalyticsWorkspaceName: 'la${appname}${resourceprefix}'
    location: deployment().location
  }
}
// */

// ORIGINAL  ORDERING
// return all resource names as a output
output gs_resourcegroup_name string = gs_resourcegroup.name
output gs_storageaccount_name string = gs_storageaccount.outputs.storageAccountName
output gs_azsearch_name string = gs_azsearch.outputs.searchServiceName
output gs_logicapp_docregistprocesswatcher_name string = gs_logicapp_docregistprocesswatcher.outputs.logicAppName
output gs_logicapp_benchmarkprocesswatcher_name string = gs_logicapp_benchmarkprocesswatcher.outputs.logicAppName
output gs_logicapp_ProcessWatcher_name string = gs_logicapp_ProcessWatcher.outputs.logicAppName
output gs_aks_name string = gs_aks.outputs.createdAksName
output gs_containerregistry_name string = gs_containerregistry.outputs.createdAcrName
output gs_azcognitiveservice_name string = gs_azcognitiveservice.outputs.cognitiveServiceName
output gs_azcognitiveservice_endpoint string = gs_azcognitiveservice.outputs.cognitiveServiceEndpoint
output gs_openaiservice_name string = gs_openaiservice.outputs.openAIServiceName
output gs_openaiservice_location string = gs_openaiservice.outputs.oopenAIServiceLocation
output gs_openaiservice_endpoint string = gs_openaiservice.outputs.openAIServiceEndpoint
output gs_openaiservicemodels_gpt4o_model_name string = gs_openaiservice.outputs.gs_openaiservicemodels_deployment_1_model_name
output gs_openaiservicemodels_gpt4o_model_id string = gs_openaiservice.outputs.gs_openaiservicemodels_deployment_1_model_id
output gs_openaiservicemodels_gpt4_32k_model_name string = gs_openaiservice.outputs.gs_openaiservicemodels_deployment_2_model_name
output gs_openaiservicemodels_gpt4_32k_model_id string = gs_openaiservice.outputs.gs_openaiservicemodels_deployment_2_model_id
output gs_openaiservicemodels_text_embedding_model_name string = gs_openaiservice.outputs.gs_openaiservicemodels_deployment_3_model_name
output gs_openaiservicemodels_text_embedding_model_id string = gs_openaiservice.outputs.gs_openaiservicemodels_deployment_3_model_id
output gs_cosmosdb_name string = gs_cosmosdb.outputs.cosmosDbAccountName
output gs_appinsights_name string = gs_appinsights.outputs.appInsightsName
output gs_appinsights_instrumentationkey string = gs_appinsights.outputs.instrumentationKey
output gs_log_analytics_workspace_log_analytics_workspace_name string = gs_log_analytics_workspace.outputs.log_analytics_workspace_name

// return all Azure logic apps service endpoints
output gs_logicapp_docregistprocesswatcher_endpoint string = gs_logicapp_docregistprocesswatcher.outputs.logicAppEndpoint
output gs_logicapp_benchmarkprocesswatcher_endpoint string = gs_logicapp_benchmarkprocesswatcher.outputs.logicAppEndpoint
output gs_logicapp_processwatcher_endpoint string = gs_logicapp_ProcessWatcher.outputs.logicAppEndpoint

// return acr url
output gs_containerregistry_endpoint string = gs_containerregistry.outputs.acrEndpoint

// return meta info
output resourceprefix string = resourceprefix
output appname string = appname
