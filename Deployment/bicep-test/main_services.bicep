// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

targetScope = 'subscription'

param prefix string
var resourceprefix = length(prefix) < 5 ? padLeft(take(uniqueString(deployment().name), 5), 5, '0') : prefix

// Create a resource group
resource gs_resourcegroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-esgdocanalysis${resourceprefix}'
  location: deployment().location
}
output gs_resourcegroup_name string = 'rg-esgdocanalysis${resourceprefix}'
output resourceprefix string = resourceprefix
