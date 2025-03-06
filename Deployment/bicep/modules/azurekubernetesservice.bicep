// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

param aksVersion string
param aksAgentVMSize string = 'Standard_D4ds_v5'
param aksAgentPoolCount int = 2
param aksAppVMSize string = 'Standard_D4ds_v5'
param aksAppPoolCount int = 2
@description('Provide a globally unique name of your Azure kubernetes Cluster')
param aksName string = 'aks-'

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
        minCount:2
        maxCount:5

      }
      // {
      //   name: 'apppool1'
      //   count: aksAppPoolCount
      //   vmSize: aksAppVMSize
      //   mode: 'User'
      //   enableAutoScaling:true
      //   minCount:2
      //   maxCount:5
      // }
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
