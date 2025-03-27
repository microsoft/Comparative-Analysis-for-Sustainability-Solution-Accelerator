// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

param aksVersion string
param aksAgentVMSize string = 'Standard_D4ds_v5'
param aksAgentPoolCount int = 3
param aksAgentPoolCountMax int = 5

@description('Provide a globally unique name of your Azure kubernetes Cluster')
param aksName string = 'aks'

@description('Provide a location for aks.')
param location string = resourceGroup().location

resource aks 'Microsoft.ContainerService/managedClusters@2021-05-01' = {
  name: aksName
  location: location

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    dnsPrefix: aksName
    enableRBAC: true
    kubernetesVersion: aksVersion
    autoUpgradeProfile:{
        upgradeChannel: 'stable'
    }
    agentPoolProfiles: [
      {
        name: 'agentpool1'
        count: aksAgentPoolCount
        vmSize: aksAgentVMSize
        mode: 'System'
        enableAutoScaling:true
        minCount:aksAgentPoolCount
        maxCount:aksAgentPoolCountMax

      }
    ]
    addonProfiles: {
      httpApplicationRouting: {
        enabled: true
      }
    }
  }
}


output createdAksName string = aksName
output createdServicePrincipalId string = aks.identity.principalId
output createdAksId string = aks.id
