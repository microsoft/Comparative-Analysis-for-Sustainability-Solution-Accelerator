// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

param location string = resourceGroup().location
param prefix string

param gpt4 object
param gpt4_32k object
param textembedding object


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
}
output openAIServiceId string = openAIService.id
output openAIServiceName string = openAIService.name
output openAIServiceEndpoint string = openAIService.properties.endpoint
output oopenAIServiceLocation string = openAIService.location

resource model_deployment_1 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: openAIService
  name: '${gpt4.name}${prefix}'
  sku: {
    name: 'Standard'
    capacity: gpt4.capacity
    tier:gpt4.scaleType
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: gpt4.name
      version: gpt4.version
    }
  }
}
resource model_deployment_2 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: openAIService
  name: '${gpt4_32k.name}${prefix}'
  sku: {
    name: 'Standard'
    capacity: gpt4_32k.capacity
    tier:gpt4_32k.scaleType
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: gpt4_32k.name
      version: gpt4_32k.version
    }
  }
  dependsOn:[
    model_deployment_1
  ]
}
resource model_deployment_3 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: openAIService
  name: '${textembedding.name}${prefix}'
  sku: {
    name: 'Standard'
    capacity: textembedding.capacity
    tier:textembedding.scaleType
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: textembedding.name
      version: textembedding.version
    }
  }
  dependsOn:[
    model_deployment_2
  ]
}

output gs_openaiservicemodels_deployment_1_model_name string = gpt4.name
output gs_openaiservicemodels_deployment_1_model_id string = gpt4.name
output gs_openaiservicemodels_deployment_2_model_name string = gpt4_32k.name
output gs_openaiservicemodels_deployment_2_model_id string = gpt4_32k.name
output gs_openaiservicemodels_deployment_3_model_name string = textembedding.name
output gs_openaiservicemodels_deployment_3_model_id string = textembedding.name
