// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

// @description('Name of the resource group to deploy resources into')
// param resourceGroupName string


// @description('Name of the resource Prefix')
// param resourceSuffix string

targetScope = 'resourceGroup'

@minLength(3)
@maxLength(20)
@description('A unique prefix for all resources in this deployment. This should be 3-20 characters long:')
param environmentName string

@description('Azure data center region where resources will be deployed. This should be a valid Azure region, e.g., eastus, westus, etc.')
param location string

var uniqueId = toLower(uniqueString(subscription().id, environmentName, location))
var resourceSuffix = padLeft(take(uniqueId, 10), 10, '0')

// Create Teams Connection for Logic App
module gs_teamsconnection 'modules/teamsconnection.bicep' = {
  name: 'teams'
  scope: resourceGroup()
  params: {
    location: resourceGroup().location
  }
}

var docregistprocesswatcher = (loadTextContent('logicapp/documentprocesswatcher.json'))
var replacedDocregistprocesswatcher = json(replace( 
                                        replace(
                                          replace(docregistprocesswatcher, '{{ subscriptionId }}', subscription().subscriptionId),
                                            '{{ resourceGroup }}', resourceGroup().name),
                                              '{{ location }}', resourceGroup().location))

// Create Logic App - Document Registration Process Watcher
module gs_logicapp_docregistprocesswatcher 'modules/azurelogicapp.bicep' = {
  name: 'logicapp-docregistprocesswatcher${resourceSuffix}'
  scope: resourceGroup()
  
  params: {
    logicAppName: 'logicapp-docregistprocesswatcher${resourceSuffix}'
    location: resourceGroup().location
    logicAppSource: replacedDocregistprocesswatcher.definition
    logicAppParameter: replacedDocregistprocesswatcher.parameters
  }

  dependsOn: [
    gs_teamsconnection
  ]
}

var benchmarkprocesswatcher = (loadTextContent('logicapp/bechmarkprocesswatcher.json'))
var replacedbenchmarkprocesswatcher = json(replace( 
                                        replace(
                                          replace(benchmarkprocesswatcher, '{{ subscriptionId }}', subscription().subscriptionId),
                                            '{{ resourceGroup }}', resourceGroup().name),
                                              '{{ location }}', resourceGroup().location))


// Create Logic App - Benchmark Service Process Watcher
module gs_logicapp_benchmarkprocesswatcher 'modules/azurelogicapp.bicep' = {
  name: 'logicapp-benchmarkprocesswatcher${resourceSuffix}'
  scope: resourceGroup()
  params: {
    logicAppName: 'logicapp-benchmarkprocesswatcher${resourceSuffix}'
    location: resourceGroup().location
    logicAppSource: replacedbenchmarkprocesswatcher.definition
    logicAppParameter: replacedbenchmarkprocesswatcher.parameters
  }

  dependsOn: [
    gs_teamsconnection
  ]
}

var gapanalysisprocesswatcher = (loadTextContent('logicapp/gapanalysisprocesswatcher.json'))
var replacedgapanalysisprocesswatcher = json(replace( 
                                        replace(
                                          replace(gapanalysisprocesswatcher, '{{ subscriptionId }}', subscription().subscriptionId),
                                            '{{ resourceGroup }}', resourceGroup().name),
                                              '{{ location }}', resourceGroup().location))



// // Create Logic App - GapAnalysis Service Process Watcher
module gs_logicapp_ProcessWatcher 'modules/azurelogicapp.bicep' = {
  name: 'logicapp-gapanalysisprocesswatcher${resourceSuffix}'
  scope: resourceGroup()
  params: {
    logicAppName: 'logicapp-gapanalysisprocesswatcher${resourceSuffix}'
    location: resourceGroup().location
    logicAppSource: replacedgapanalysisprocesswatcher.definition
    logicAppParameter: replacedgapanalysisprocesswatcher.parameters
  }

  dependsOn: [
    gs_teamsconnection
  ]
}


// Create a storage account
module gs_storageaccount 'modules/azurestorageaccount.bicep' = {
  name: 'blobesgdocanalysis${resourceSuffix}'
  scope: resourceGroup()
  params: {
    storageAccountName: 'blob${resourceSuffix}'
    location: resourceGroup().location
  }
}

// Create a Azure Search Service
module gs_azsearch 'modules/azuresearch.bicep' = {
  name: 'search-esgdocanalysis${resourceSuffix}'
  scope: resourceGroup()
  params: {
    searchServiceName: 'search-${resourceSuffix}'
    location: resourceGroup().location
  }
}

// Create AKS Cluster
module gs_aks 'modules/azurekubernetesservice.bicep' = {
  name: 'aks-esgdocanalysis${resourceSuffix}'
  scope: resourceGroup()
  params: {
    aksName: 'aks-esgdocanalysis${resourceSuffix}'
    location: resourceGroup().location
  }
}

// Create Container Registry
module gs_containerregistry 'modules/azurecontainerregistry.bicep' = {
  name: 'acresgdocanalysis${resourceSuffix}'
  scope: resourceGroup()
  params: {
    acrName: 'acresgdocanalysis${resourceSuffix}'
    location: resourceGroup().location
  }
  dependsOn: [
    gs_aks
  ]
}

// Create Azure Cognitive Service
module gs_azcognitiveservice 'modules/azurecognitiveservice.bicep' = {
  name: 'cognitiveservice-esgdocanalysis${resourceSuffix}'
  scope: resourceGroup()
  params: {
    cognitiveServiceName: 'cognitiveservice-esgdocanalysis${resourceSuffix}'
    location: 'eastus'
  }
}

// Create Azure Open AI Service
module gs_openaiservice 'modules/azureopenaiservice.bicep' = {
  name: 'openaiservice-esgdocanalysis${resourceSuffix}'
  scope: resourceGroup()
  params: {
    openAIServiceName: 'openaiservice-esgdocanalysis${resourceSuffix}'
    // GPT-4-32K model & GPT-4o available Data center information.
    // https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models#gpt-4    
    location: 'swedencentral'
  }
}

// TBD - Create Azure App Insights.
// Create Azure App Insights
// module gs_appinsights 'modules/azureappingisht.bicep' = {
//   name: 'appinsights-esgdocanalysis${resourceSuffix}'
//   scope: resourceGroup()
//   params: {
//     appInsightsName: 'appinsights-esgdocanalysis${resourceSuffix}'
//     location: deployment().location
//   }
// }

// Due to limited Quota, not easy to control per each model deployment.
// Set the minimum capacity of each model
// Based on customer's Model capacity, it needs to be updated in Azure Portal.
module gs_openaiservicemodels_gpt4o 'modules/azureopenaiservicemodel.bicep' = {
  scope: resourceGroup()
  name: 'gpt-4o${resourceSuffix}'
  params: {
    parentResourceName: gs_openaiservice.outputs.openAIServiceName
    name:'gpt-4o${resourceSuffix}'
    model: {
        name: 'gpt-4o'
        version: '2024-11-20'
        raiPolicyName: ''
        capacity: 1
        scaleType: 'Standard'
      }
  }
}

module gs_openaiservicemodels_text_embedding 'modules/azureopenaiservicemodel.bicep' = {
  scope: resourceGroup()
  name: 'textembedding${resourceSuffix}'
  params: {
    parentResourceName: gs_openaiservice.outputs.openAIServiceName
    name:'textembedding${resourceSuffix}'
    model: {
        name: 'text-embedding-3-large'
        version: '1'
        raiPolicyName: ''
        capacity: 1
        scaleType: 'Standard'
    }
  }
  dependsOn: [gs_openaiservicemodels_gpt4o]
}

// Create Azure Cosmos DB Mongo
module gs_cosmosdb 'modules/azurecosmosdb.bicep' = {
  name: 'cosmosdb-esgdocanalysis${resourceSuffix}'
  scope: resourceGroup()
  params: {
    cosmosDbAccountName: 'cosmosdb-esgdocanalysis${resourceSuffix}'
    location: resourceGroup().location
  }
}

// return all resource names as a output
output gs_resourcegroup_name string =  resourceGroup().name //'rg-esgdocanalysis${resourceSuffix}'
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
output gs_openaiservicemodels_gpt4o_model_name string = gs_openaiservicemodels_gpt4o.outputs.deployedModelName
output gs_openaiservicemodels_gpt4o_model_id string = gs_openaiservicemodels_gpt4o.outputs.deployedModelId
// output gs_openaiservicemodels_gpt4_32k_model_name string = gs_openaiservicemodels_gpt4_32k.outputs.deployedModelName
// output gs_openaiservicemodels_gpt4_32k_model_id string = gs_openaiservicemodels_gpt4_32k.outputs.deployedModelId
output gs_openaiservicemodels_text_embedding_model_name string = gs_openaiservicemodels_text_embedding.outputs.deployedModelName
output gs_openaiservicemodels_text_embedding_model_id string = gs_openaiservicemodels_text_embedding.outputs.deployedModelId
output gs_cosmosdb_name string = gs_cosmosdb.outputs.cosmosDbAccountName
// output gs_appinsights_name string = gs_appinsights.outputs.appInsightsName
// output gs_appinsights_instrumentationkey string = gs_appinsights.outputs.instrumentationKey

// return all Azure logic apps service endpoints
output gs_logicapp_docregistprocesswatcher_endpoint string = gs_logicapp_docregistprocesswatcher.outputs.logicAppEndpoint
output gs_logicapp_benchmarkprocesswatcher_endpoint string = gs_logicapp_benchmarkprocesswatcher.outputs.logicAppEndpoint
output gs_logicapp_processwatcher_endpoint string = gs_logicapp_ProcessWatcher.outputs.logicAppEndpoint

// return acr url
output gs_containerregistry_endpoint string = gs_containerregistry.outputs.acrEndpoint


