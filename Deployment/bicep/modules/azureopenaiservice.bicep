// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

param openAIServiceName string = 'openaiservice-esgdocanalysis'
param location string = resourceGroup().location
param prefix string
param obj_model_deployment_1 object = {
      name: 'gpt-4o'
      version: '2024-05-13'
      raiPolicyName: ''
      capacity: 1
      scaleType: 'Standard'
    }
param obj_model_deployment_2 object = {
      name: 'gpt-4o-mini'
      version: '2024-07-18'
      raiPolicyName: ''
      capacity: 1
      scaleType: 'Manual'
    }
param obj_model_deployment_3 object = {
      name: 'text-embedding-3-small'
      version: '1'
      raiPolicyName: ''
      capacity: 1
      scaleType: 'Standard'
    }


resource openAIService 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: 'openaiservice-esgdocanalysis${prefix}'
  location: location
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    // Add any specific properties if needed   }
  }
  resource model_deployment_1 'deployments@2024-10-01' = {
    name: '${obj_model_deployment_1.name}${prefix}'
    sku: {
      name: 'Standard'
      capacity: obj_model_deployment_1.capacity
    }
    properties: {
      model: {
        format: 'OpenAI'
        name: obj_model_deployment_1.name
        version: obj_model_deployment_1.version
      }
    }
  }
  // resource model_deployment_2 'deployments@2024-10-01' = {
  //   name: '${obj_model_deployment_2.name}${prefix}'
  //   sku: {
  //     name: 'Standard'
  //     capacity: obj_model_deployment_2.capacity
  //   }
  //   properties: {
  //     model: {
  //       format: 'OpenAI'
  //       name: obj_model_deployment_2.name
  //       version: obj_model_deployment_2.version
  //     }
  //   }
  // }
  // resource model_deployment_3 'deployments@2024-10-01' = {
  //   name: '${obj_model_deployment_3.name}${prefix}'
  //   sku: {
  //     name: 'Standard'
  //     capacity: obj_model_deployment_3.capacity
  //   }
  //   properties: {
  //     model: {
  //       format: 'OpenAI'
  //       name: obj_model_deployment_3.name
  //       version: obj_model_deployment_3.version
  //     }
  //   }
  // }
}

output openAIServiceId string = openAIService.id
output openAIServiceName string = openAIService.name
output openAIServiceEndpoint string = openAIService.properties.endpoint
output oopenAIServiceLocation string = openAIService.location

output gs_openaiservicemodels_deployment_1_model_name string = '${obj_model_deployment_1.name}${prefix}'
output gs_openaiservicemodels_deployment_1_model_id string = obj_model_deployment_1.name
output gs_openaiservicemodels_deployment_2_model_name string = '${obj_model_deployment_2.name}${prefix}'
output gs_openaiservicemodels_deployment_2_model_id string = obj_model_deployment_2.name
output gs_openaiservicemodels_deployment_3_model_name string = '${obj_model_deployment_3.name}${prefix}'
output gs_openaiservicemodels_deployment_3_model_id string = obj_model_deployment_3.name
