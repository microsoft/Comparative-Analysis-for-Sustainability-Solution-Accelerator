# Copyright (c) Microsoft Corporation.
# Licensed under the MIT license.
param(
    [Parameter(Mandatory=$false,
      HelpMessage='Enter the Azure subscription ID to deploy your resources')]
    [string]$subscriptionID = $env:AZSUB,

    [Parameter(Mandatory=$false,
      HelpMessage='Enter the Azure Data Center Region to deploy your resources')]
    [ValidateSet(
        'EastUS', 'EastUS2', 'WestUS', 'WestUS2', 'WestUS3', 'CentralUS', 'NorthCentralUS', 'SouthCentralUS', 
        'WestEurope', 'NorthEurope', 'SoutheastAsia', 'EastAsia', 'JapanEast', 'JapanWest', 
        'AustraliaEast', 'AustraliaSoutheast', 'CentralIndia', 'SouthIndia', 'CanadaCentral', 
        'CanadaEast', 'UKSouth', 'UKWest', 'FranceCentral', 'FranceSouth', 'KoreaCentral', 
        'KoreaSouth', 'GermanyWestCentral', 'GermanyNorth', 'NorwayWest', 'NorwayEast', 
        'SwitzerlandNorth', 'SwitzerlandWest', 'UAENorth', 'UAECentral', 'SouthAfricaNorth', 
        'SouthAfricaWest', 'BrazilSouth', 'BrazilSoutheast', 'QatarCentral', 'ChinaNorth', 
        'ChinaEast', 'ChinaNorth2', 'ChinaEast2', 'SwedenCentral'
    )]
    [string]$location = $env:AZLOC,

    [Parameter(Mandatory=$false,
      HelpMessage='Enter Your Email address for the certificate management')]
    [string]$email = $env:ADMIN_EMAIL,

    [Parameter(Mandatory=$false,
      HelpMessage='Enter an IP range as comma separated list of CIDRs to allow access to the services')]
    [string]$ipRange = $env:ipRange,

    [Parameter(Mandatory=$false,
      HelpMessage='Enter the prefix')]
    [string]$prefix,

    [Parameter(Mandatory=$false,
      HelpMessage='Enter the appname')]
    [string]$appname
)
function Get-CurrentLine() {
    $s = $STAMP
    $n = $MyInvocation.ScriptLineNumber
    $f = Split-Path -Path $MyInvocation.ScriptName -Leaf
    return "${s}:${f}(${n})"
}
function LoginAzure([string]$subscriptionID) {
    try {
        # Write-Host "*** TESTING DEPLOYMENT *** AUTO AZURE LOGIN " -ForegroundColor DarkRed
        Write-Host "Log in to Azure.....`r`n" -ForegroundColor Yellow
        az login
        az account set --subscription $subscriptionID
        Write-Host "Switched subscription to '$subscriptionID' `r`n" -ForegroundColor Yellow
        return
    } catch {
        Write-Host "$($MyInvocation.MyCommand.Name): no login" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host $_.InvocationInfo.PositionMessage -ForegroundColor Red
        Write-Host $_.ScriptStackTrace -ForegroundColor Red
        exit 1
    }
}
function DeployAzureResources([string]$location) {
    try {
        $subscriptionDeployment = "esg-$appname-$prefix-$STAMP"
        # Perform a what-if deployment to preview changes
        Write-Host "Evaluating Deployment resource availabilities to preview changes..." -ForegroundColor Yellow
# Write-Host "*** TESTING DEPLOYMENT *** NO WHAT-IF" -ForegroundColor DarkRed
        $whatIfResult = az deployment sub what-if --parameters "@../$iac_dir/main_services.parameters.json" --template-file ../$iac_dir/main_services.bicep -l $location -n "$subscriptionDeployment"
        if ($LASTEXITCODE -ne 0) {
            Write-Host "There might be something wrong with your deployment." -ForegroundColor Red
            Write-Host $whatIfResult -ForegroundColor Red
            exit 1            
        }
Write-Host ($whatIfResult|Format-List|Out-String)
# Write-Host "*** TESTING DEPLOYMENT *** throwing What If Exception:" -ForegroundColor DarkRed
# throw [System.Exception]"What If Exception"
        Write-Host "Deployment resource availabilities have been evaluated successfully." -ForegroundColor Green
        $deployment_output = az deployment sub create --parameters "@../$iac_dir/main_services.parameters.json" --template-file ../$iac_dir/main_services.bicep -l $location -n "$subscriptionDeployment"

        $joinedString = $deployment_output -join ""
        $jsonString = ConvertFrom-Json $joinedString
        # Map the deployment result to DeploymentResult object
        $deploymentResult.MapResult($jsonString)
        return $jsonString
    } catch {
        Write-Host "$($MyInvocation.MyCommand.Name):An error occurred during the deployment process:" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host $_.InvocationInfo.PositionMessage -ForegroundColor Red
        Write-Host $_.ScriptStackTrace -ForegroundColor Red
        exit 1
    }
}

function DisplayResult([pscustomobject]$jsonString) {
    $logicAppDocumentProcessWatcherName = $jsonString.properties.outputs.gs_logicapp_docregistprocesswatcher_name.value
    $logicAppBenchmarkProcessWatcherName = $jsonString.properties.outputs.gs_logicapp_benchmarkprocesswatcher_name.value
    $logicAppGapAnalysisProcessWatcherName = $jsonString.properties.outputs.gs_logicapp_ProcessWatcher_name.value
    
    $azLogicAppDocumentRegistProcessWatcherUrl = $jsonString.properties.outputs.gs_logicapp_docregistprocesswatcher_endpoint.value
    $azLogicAppBenchmarkProcessWatcherUrl = $jsonString.properties.outputs.gs_logicapp_benchmarkprocesswatcher_endpoint.value
    $azLogicAppGapAnalysisProcessWatcherUrl = $jsonString.properties.outputs.gs_logicapp_docregistprocesswatcher_endpoint.value
    
    Write-Host "--------------------------------------------`r`n" -ForegroundColor White
    Write-Host "Deployment output: `r`n" -ForegroundColor White
    Write-Host "Subscription Id: $subscriptionID `r`n" -ForegroundColor Yellow
    Write-Host "appname is $($jsonString.properties.outputs.appname.value) " -ForegroundColor Yellow
    Write-Host "resource prefix is $($jsonString.properties.outputs.resourceprefix.value) " -ForegroundColor Yellow
    Write-Host "ESG AI Document Analysis resource group: $($jsonString.properties.outputs.gs_resourcegroup_name.value) `r`n" -ForegroundColor Yellow
    Write-Host "Azure Kubernetes Account $($jsonString.properties.outputs.gs_aks_name.value) has been created" -ForegroundColor Yellow
    Write-Host "Azure Container Registry $($jsonString.properties.outputs.gs_containerregistry_name.value) has been created" -ForegroundColor Yellow
    Write-Host "Azure Search Service $($jsonString.properties.outputs.gs_azsearch_name.value) has been created" -ForegroundColor Yellow
    Write-Host "Azure Open AI Service $($jsonString.properties.outputs.gs_openaiservice_name.value) has been created" -ForegroundColor Yellow
    Write-Host "Azure Cognitive Service $($jsonString.properties.outputs.gs_azcognitiveservice_name.value) has been created" -ForegroundColor Yellow
    Write-Host "Azure Stroage Account $($jsonString.properties.outputs.gs_storageaccount_name.value) has been created" -ForegroundColor Yellow 
    Write-Host "Azure Cosmos DB $($jsonString.properties.outputs.gs_cosmosdb_name.value) has been created " -ForegroundColor Yellow
    Write-Host "Document Registration Process Watcher Logic App $logicAppDocumentProcessWatcherName has been deployed " -ForegroundColor Yellow
    Write-Host "($azLogicAppDocumentRegistProcessWatcherUrl) " -ForegroundColor Yellow
    Write-Host "Benchmark Process Watcher $logicAppBenchmarkProcessWatcherName has been deployed" -ForegroundColor Yellow
    Write-Host "($azLogicAppBenchmarkProcessWatcherUrl) " -ForegroundColor Yellow
    Write-Host "GapAnalysis Process Watcher $logicAppGapAnalysisProcessWatcherName has been deployed" -ForegroundColor Yellow
    Write-Host "($azLogicAppGapAnalysisProcessWatcherUrl) " -ForegroundColor Yellow
    Write-Host "Application Insights: $($jsonString.properties.outputs.gs_appinsights_name.value) `r`n" -ForegroundColor Yellow
    
    Write-Host "--------------------------------------------`r`n" -ForegroundColor White
}

class DeploymentResult {
    [string]$ResourceGroupName
    [string]$StorageAccountName
    [string]$StorageAccountConnectionString
    [string]$AzSearchServiceName
    [string]$AzSearchServicEndpoint
    [string]$AzSearchAdminKey
    [string]$LogicAppDocumentProcessWatcherName
    [string]$LogicAppBenchmarkProcessWatcherName
    [string]$LogicAppGapAnalysisProcessWatcherName
    [string]$AzLogicAppDocumentRegistProcessWatcherUrl
    [string]$AzLogicAppBenchmarkProcessWatcherUrl
    [string]$AzLogicAppGapAnalysisProcessWatcherUrl
    [string]$AksName
    [string]$ContainerRegistryName
    [string]$AzCognitiveServiceName
    [string]$AzCognitiveServiceKey
    [string]$AzCognitiveServiceEndpoint
    [string]$AzOpenAiServiceName
    [string]$AzGPT4oModelName
    [string]$AzGPT4oModelId
    [string]$AzGPT4_32KModelName
    [string]$AzGPT4_32KModelId
    [string]$AzGPTEmbeddingModelName
    [string]$AzGPTEmbeddingModelId
    [string]$AzOpenAiServiceEndpoint
    [string]$AzOpenAiServiceKey
    [string]$AzCosmosDBName
    [string]$AzCosmosDBConnectionString
    [string]$AzAppInsightsName
    [string]$AzAppInsightsInstrumentationKey
    [string]$appname
    [string]$resourceprefix

    DeploymentResult() {
        # Resource Group
        $this.ResourceGroupName = ""
        # Storage Account
        $this.StorageAccountName = ""
        $this.StorageAccountConnectionString = ""
        # Azure Search
        $this.AzSearchServiceName = ""
        $this.AzSearchServicEndpoint = ""
        $this.AzSearchAdminKey = ""
        # Logic Apps
        $this.LogicAppDocumentProcessWatcherName = ""
        $this.LogicAppBenchmarkProcessWatcherName = ""
        $this.LogicAppGapAnalysisProcessWatcherName = ""
        $this.AzLogicAppDocumentRegistProcessWatcherUrl = ""
        $this.AzLogicAppBenchmarkProcessWatcherUrl = ""
        $this.AzLogicAppGapAnalysisProcessWatcherUrl = ""
        # AKS
        $this.AksName = ""
        # Container Registry
        $this.ContainerRegistryName = ""
        # Cognitive Service - Azure AI Intelligence Document Service
        $this.AzCognitiveServiceName = ""
        $this.AzCognitiveServiceEndpoint = ""
        $this.AzCognitiveServiceKey = ""
        # Open AI Service
        $this.AzOpenAiServiceName = ""
        $this.AzOpenAiServiceEndpoint = ""
        $this.AzOpenAiServiceKey = ""
        # Model - GPT4o
        $this.AzGPT4oModelName = ""
        $this.AzGPT4oModelId = ""
        # Model - GPT4_32K
        $this.AzGPT4_32KModelName = ""
        $this.AzGPT4_32KModelId = ""
        # Model - Embedding
        $this.AzGPTEmbeddingModelName = ""
        $this.AzGPTEmbeddingModelId = ""
        # Cosmos DB
        $this.AzCosmosDBName = ""
        $this.AzCosmosDBConnectionString = ""
        # Insights
        $this.AzAppInsightsName = ""
        $this.AzAppInsightsInstrumentationKey = ""
        # meta info
        $this.appname = ""
        $this.resourceprefix = ""
    }

    [void]MapResult([pscustomobject]$jsonString) {
        # Add your code here
        $this.ResourceGroupName = $jsonString.properties.outputs.gs_resourcegroup_name.value
        $this.StorageAccountName = $jsonString.properties.outputs.gs_storageaccount_name.value
        $this.AzSearchServiceName = $jsonString.properties.outputs.gs_azsearch_name.value
        $this.AzSearchServicEndpoint =  "https://$($this.AzSearchServiceName).search.windows.net"
        $this.LogicAppDocumentProcessWatcherName = $jsonString.properties.outputs.gs_logicapp_docregistprocesswatcher_name.value
        $this.LogicAppBenchmarkProcessWatcherName = $jsonString.properties.outputs.gs_logicapp_benchmarkprocesswatcher_name.value
        $this.LogicAppGapAnalysisProcessWatcherName = $jsonString.properties.outputs.gs_logicapp_ProcessWatcher_name.value
        $this.AksName = $jsonString.properties.outputs.gs_aks_name.value
        $this.ContainerRegistryName = $jsonString.properties.outputs.gs_containerregistry_name.value
        $this.AzCognitiveServiceName = $jsonString.properties.outputs.gs_azcognitiveservice_name.value
        $this.AzCognitiveServiceEndpoint = $jsonString.properties.outputs.gs_azcognitiveservice_endpoint.value
        $this.AzOpenAiServiceName = $jsonString.properties.outputs.gs_openaiservice_name.value
        $this.AzOpenAiServiceEndpoint = $jsonString.properties.outputs.gs_openaiservice_endpoint.value
        $this.AzCosmosDBName = $jsonString.properties.outputs.gs_cosmosdb_name.value
        $this.AzLogicAppDocumentRegistProcessWatcherUrl = $jsonString.properties.outputs.gs_logicapp_docregistprocesswatcher_endpoint.value
        $this.AzLogicAppBenchmarkProcessWatcherUrl = $jsonString.properties.outputs.gs_logicapp_benchmarkprocesswatcher_endpoint.value
        $this.AzLogicAppGapAnalysisProcessWatcherUrl = $jsonString.properties.outputs.gs_logicapp_docregistprocesswatcher_endpoint.value
        $this.AzGPT4oModelName = $jsonString.properties.outputs.gs_openaiservicemodels_gpt4o_model_name.value
        $this.AzGPT4oModelId = $jsonString.properties.outputs.gs_openaiservicemodels_gpt4o_model_id.value
        $this.AzGPT4_32KModelName = $jsonString.properties.outputs.gs_openaiservicemodels_gpt4_32k_model_name.value
        $this.AzGPT4_32KModelId = $jsonString.properties.outputs.gs_openaiservicemodels_gpt4_32k_model_id.value
        $this.AzGPTEmbeddingModelName = $jsonString.properties.outputs.gs_openaiservicemodels_text_embedding_model_name.value
        $this.AzGPTEmbeddingModelId = $jsonString.properties.outputs.gs_openaiservicemodels_text_embedding_model_id.value
        $this.AzAppInsightsName = $jsonString.properties.outputs.gs_appinsights_name.value
        $this.AzAppInsightsInstrumentationKey = $jsonString.properties.outputs.gs_appinsights_instrumentationkey.value
        $this.appname = $jsonString.properties.outputs.appname.value
        $this.resourceprefix = $jsonString.properties.outputs.resourceprefix.value
    }
}

function Get-LogicAppTriggerUrl {
    param (
        [string]$subscriptionId,
        [string]$resourceGroupName,
        [string]$logicAppName,
        [string]$triggerName
    )
    $url = "https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Logic/workflows/$logicAppName/triggers/$triggerName/listCallbackUrl?api-version=2016-06-01"
    $callbackUrl = az rest --method POST --uri $url --query "value" -o tsv
    return $callbackUrl
}

# Function to replace placeholders in a template with actual values
function Invoke-PlaceholdersReplacement($template, $placeholders) {
    foreach ($key in $placeholders.Keys) {
        $template = $template -replace $key, $placeholders[$key]
    }
    return $template
}
# Function to get the external IP address of a service
function Get-ExternalIP {
    param (
        [string]$serviceName,
        [string]$namespace
    )
    $externalIP = kubectl get svc $serviceName -n $namespace -o jsonpath='{.status.loadBalancer.ingress[0].ip}'
    return $externalIP
}

function deploy_main_services() {
    ###############################################################
    # Step 1 : Deploy Azure resources
    Write-Host "Step 1 : Deploy Azure resources" -ForegroundColor Yellow
    ###############################################################
    # Deploy Azure Resources
    Write-Host "Deploying Azure resources in $location region.....`r`n" -ForegroundColor Yellow
    $resultJson = DeployAzureResources($location)
    # Display the deployment result
    DisplayResult($resultJson)
}
function get_service_info() {
    ###############################################################
    # Step 2 : Get Secrets from Azure resources
    Write-Host "Step 2 : Get Secrets from Azure resources" -ForegroundColor Yellow
    ###############################################################
    # Get the storage account key
    $storageAccountKey = az storage account keys list --account-name $deploymentResult.StorageAccountName --resource-group $deploymentResult.ResourceGroupName --query "[0].value" -o tsv
    ## Construct the connection string manually
    $storageAccountConnectionString = "DefaultEndpointsProtocol=https;AccountName=$($deploymentResult.StorageAccountName);AccountKey=$storageAccountKey;EndpointSuffix=core.windows.net"
    ## Assign the connection string to the deployment result object
    $deploymentResult.StorageAccountConnectionString = $storageAccountConnectionString    
    # Get the Azure Logic App workflow URLs
    $triggerName = "When_a_HTTP_request_is_received" # Logic App trigger name for all logic apps HTTP trigger
    ## Get Logic Workflow URL for Document Registration Process Watcher
    $deploymentResult.AzLogicAppDocumentRegistProcessWatcherUrl = Get-LogicAppTriggerUrl -subscriptionId $subscriptionID -resourceGroupName $deploymentResult.ResourceGroupName -logicAppName $deploymentResult.LogicAppDocumentProcessWatcherName -triggerName $triggerName
    ## Get Logic Workflow URL for Benchmark Process Watcher
    $deploymentResult.AzLogicAppBenchmarkProcessWatcherUrl = Get-LogicAppTriggerUrl -subscriptionId $subscriptionID -resourceGroupName $deploymentResult.ResourceGroupName -logicAppName $deploymentResult.LogicAppBenchmarkProcessWatcherName -triggerName $triggerName
    ## Get Logic Workflow URL for GapAnalysis Process Watcher
    $deploymentResult.AzLogicAppGapAnalysisProcessWatcherUrl = Get-LogicAppTriggerUrl -subscriptionId $subscriptionID -resourceGroupName $deploymentResult.ResourceGroupName -logicAppName $deploymentResult.LogicAppGapAnalysisProcessWatcherName -triggerName $triggerName
    # Get MongoDB connection string
    $deploymentResult.AzCosmosDBConnectionString = az cosmosdb keys list --name $deploymentResult.AzCosmosDBName --resource-group $deploymentResult.ResourceGroupName --type connection-strings --query "connectionStrings[0].connectionString" -o tsv
    # Get Azure Cognitive Service API Key
    $deploymentResult.AzCognitiveServiceKey = az cognitiveservices account keys list --name $deploymentResult.AzCognitiveServiceName --resource-group $deploymentResult.ResourceGroupName --query "key1" -o tsv
    # Get Azure Search Service Admin Key
    $deploymentResult.AzSearchAdminKey = az search admin-key show --service-name $deploymentResult.AzSearchServiceName --resource-group $deploymentResult.ResourceGroupName --query "primaryKey" -o tsv
    # Get Azure Open AI Service API Key
    $deploymentResult.AzOpenAiServiceKey = az cognitiveservices account keys list --name $deploymentResult.AzOpenAiServiceName --resource-group $deploymentResult.ResourceGroupName --query "key1" -o tsv
    
}
function update_app_configs() {
    ######################################################################################################################
    # Step 3 : Update App Configuration files with Secrets and information for AI Service and Kernel Memory Service.
    write-host "Step 3 : Update App Configuration files with Secrets and information for AI Service and Kernel Memory Service." -ForegroundColor Yellow
    ######################################################################################################################

    # Step 3-1 Loading aiservice's configuration file template then replace the placeholder with the actual values
    # Define the placeholders and their corresponding values for AI service configuration
    
    $aiServicePlaceholders = @{
        '{{ gpt4-32Kendpoint }}' = $deploymentResult.AzOpenAiServiceEndpoint
        '{{ gpt4-32Kapikey }}' = $deploymentResult.AzOpenAiServiceKey
        '{{ gpt4-32Kmodelname }}' = $deploymentResult.AzGPT4_32KModelName
        '{{ gpt4-32Kmodelid }}' = $deploymentResult.AzGPT4_32KModelId
        '{{ gpt4-32Kembeddingmodelname }}' = $deploymentResult.AzGPTEmbeddingModelName
        '{{ mongodbconnectionstring }}' = $deploymentResult.AzCosmosDBConnectionString
        '{{ blobstorageconnectionstring }}' = $deploymentResult.StorageAccountConnectionString
        '{{ documentpreprocessingprocesswatcherurl }}' = $deploymentResult.AzLogicAppDocumentRegistProcessWatcherUrl
        '{{ benchmarkprocesswatcherurl }}' = $deploymentResult.AzLogicAppBenchmarkProcessWatcherUrl
        '{{ gapanalysisprocesswatcherurl }}' = $deploymentResult.AzLogicAppGapAnalysisProcessWatcherUrl
    }

    # Load and update the AI service configuration template
    $aiServiceConfigTemplate = Get-Content -Path ../../Services/appconfig/aiservice/appsettings.dev.json.template -Raw
    $aiServiceConfigTemplate = Invoke-PlaceholdersReplacement $aiServiceConfigTemplate $aiServicePlaceholders

    # Save the updated AI service configuration file
    $aiServiceConfigPath = "../../Services/appconfig/aiservice/appsettings.dev.json"
    $aiServiceConfigTemplate | Set-Content -Path $aiServiceConfigPath -Force
    Write-Host "ESG AI Document Service Application Configuration file have been updated successfully." -ForegroundColor Green

    # Step 3-2 Loading kernel memory service's configuration file template then replace the placeholder with the actual values
    # Define the placeholders and their corresponding values for kernel memory service configuration

    $kernelMemoryServicePlaceholders = @{
        '{{ blobstorageName }}' = $deploymentResult.StorageAccountName
        '{{ blobstorageconnectionstring }}' = $deploymentResult.StorageAccountConnectionString
        '{{ azuresearchendpoint }}' = $deploymentResult.AzSearchServicEndpoint
        '{{ azuresearchadminkey }}' = $deploymentResult.AzSearchAdminKey
        '{{ azureopenaiendpoint }}' = $deploymentResult.AzOpenAiServiceEndpoint
        '{{ azureopenaiapikey }}' = $deploymentResult.AzOpenAiServiceKey
        '{{ azuregpt4omodelname }}' = $deploymentResult.AzGPT4oModelName
        '{{ embeddingmodelname }}' = $deploymentResult.AzGPTEmbeddingModelName
        '{{ azurecognitiveservicapikey }}' = $deploymentResult.AzCognitiveServiceKey
        '{{ azurecognitiveserviceendpoint }}' = $deploymentResult.AzCognitiveServiceEndpoint
    }

    # Load and update the kernel memory service configuration template
    $kernelMemoryServiceConfigTemplate = Get-Content -Path ../../Services/appconfig/kernelmemory/appsettings.development.json.template -Raw
    $kernelMemoryServiceConfigTemplate = Invoke-PlaceholdersReplacement $kernelMemoryServiceConfigTemplate $kernelMemoryServicePlaceholders

    # Save the updated kernel memory service configuration file
    $kernelMemoryServiceConfigPath = "../../Services/appconfig/kernelmemory/appsettings.development.json"
    $kernelMemoryServiceConfigTemplate | Set-Content -Path $kernelMemoryServiceConfigPath -Force
    Write-Host "Kernel Memory Service Application Configuration file have been updated successfully." -ForegroundColor Green
        
    # Step 3-3 Copy the configuration files to the source folders
    # Copy two configuration files to each source folder

    Write-Host "Copying the configuration files to the source folders" -ForegroundColor Green
    Copy-Item -Path $aiServiceConfigPath -Destination "../../Services/src/esg-ai-doc-analysis/CFS.SK.Sustainability.AI.Host/appsettings.dev.json" -Force
    Copy-Item -Path $kernelMemoryServiceConfigPath -Destination "../../Services/src/kernel-memory/service/Service/appsettings.Development.json" -Force


}
function build_push_container_images() {
    ######################################################################################################################
    # Step 4 : docker build and push container images to Azure Container Registry
    Write-Host "Step 4 : docker build and push container images to Azure Container Registry" -ForegroundColor Yellow
    ######################################################################################################################
    
    # 1. Login to Azure Container Registry
    az acr login --name $deploymentResult.ContainerRegistryName
    $acrNamespace = "esgdocanalysis"

    # 2. Build and push the images to Azure Container Registry
    #  2-1. Build and push the AI Service container image to  Azure Container Registry
    $acrAIServiceTag = "$($deploymentResult.ContainerRegistryName).azurecr.io/$acrNamespace/aiservice"
# Write-Host "*** TESTING DEPLOYMENT *** NO DOCKER BUILD -t $acrAIServiceTag" -ForegroundColor DarkRed
    docker build ../../Services/src/esg-ai-doc-analysis/. --no-cache -t $acrAIServiceTag
# Write-Host "*** TESTING DEPLOYMENT *** NO docker push $acrAIServiceTag" -ForegroundColor DarkRed
    docker push $acrAIServiceTag

    #  2-2. Build and push the Kernel Memory Service container image to Azure Container Registry
    $acrKernelMemoryTag = "$($deploymentResult.ContainerRegistryName).azurecr.io/$acrNamespace/kernelmemory"
# Write-Host "*** TESTING DEPLOYMENT *** NO DOCKER BUILD -t $acrKernelMemoryTag" -ForegroundColor DarkRed
    docker build ../../Services/src/kernel-memory/. --no-cache -t $acrKernelMemoryTag
# Write-Host "*** TESTING DEPLOYMENT *** NO docker push $acrKernelMemoryTag" -ForegroundColor DarkRed
    docker push $acrKernelMemoryTag
}
function enable_app_routing() {
    # 4.approuting enable and enable addons for http_application_routing
    try {
        Write-Host "Enabling application routing addon for AKS..." -ForegroundColor Cyan
        Import-Module ../kubernetes/enable_approuting.psm1
        Enable-AppRouting -ResourceGroupName $deploymentResult.ResourceGroupName -ClusterName $deploymentResult.AksName
        Write-Host "Application routing addon enabled." -ForegroundColor Green
    }
    catch {
        Write-Host "$($MyInvocation.MyCommand.Name): Failed to enable application routing addon." -ForegroundColor Red
        Write-Host "Error details:" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host $_.Exception.StackTrace -ForegroundColor Red
        exit 1
    }
}
function configure_k8s() {
    ######################################################################################################################
    # Step 5 : Configure Kubernetes Infrastructure
    Write-Host "Step 5 : Configure Kubernetes Infrastructure" -ForegroundColor Yellow
    ######################################################################################################################
    # 0. Attach Container Registry to AKS
    # Write-Host "Attach Container Registry to AKS" -ForegroundColor Green
    # az aks update --name $deploymentResult.AksName --resource-group $deploymentResult.ResourceGroupName --attach-acr $deploymentResult.ContainerRegistryName

    Write-Host "Attach Container Registry to AKS" -ForegroundColor Green
    
    $maxRetries = 10
    $retryCount = 0
    $delay = 30 # Delay in seconds

    while ($retryCount -lt $maxRetries) {
        try {
            # Attempt to update the AKS cluster
            az aks update --name $deploymentResult.AksName --resource-group $deploymentResult.ResourceGroupName --attach-acr $deploymentResult.ContainerRegistryName
            Write-Host "AKS cluster updated successfully."
            break
        } catch {
            $errorMessage = $_.Exception.Message
            if ($errorMessage -match "OperationNotAllowed" -and $errorMessage -match "Another operation \(Updating\) is in progress") {
                Write-Host "Operation not allowed: Another operation is in progress. Retrying in $delay seconds..."
                Start-Sleep -Seconds $delay
                $retryCount++
            } else {
                Write-Host "An unexpected error occurred: $errorMessage" -ForegroundColor Red
                throw $_
            }
        }
    }

    if ($retryCount -eq $maxRetries) {
        Write-Host "Max retries reached. Failed to update the AKS cluster." -ForegroundColor Red
        exit 1
    }

    $kubenamepsace = "esgdocanalysis"

    # 1. Get the Kubernetes resource group
    $aksResourceGroupName = $(az aks show --resource-group $deploymentResult.ResourceGroupName --name $deploymentResult.AksName --query nodeResourceGroup --output tsv)

    try {
        Write-Host "Getting the Kubernetes resource group..." -ForegroundColor Cyan
        $aksResourceGroupName = $(az aks show --resource-group $deploymentResult.ResourceGroupName --name $deploymentResult.AksName --query nodeResourceGroup --output tsv)
        Write-Host "Kubernetes resource group: $aksResourceGroupName" -ForegroundColor Green
    }
    catch {
        Write-Host "Failed to get the Kubernetes resource group." -ForegroundColor Red
        Write-Host "Error details:" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host $_.Exception.StackTrace -ForegroundColor Red
        exit 1
    }


    # 2.Connect to AKS cluster
    #az aks get-credentials --resource-group $deploymentResult.ResourceGroupName --name $deploymentResult.AksName --overwrite-existing
    try {
        Write-Host "Connecting to AKS cluster..." -ForegroundColor Cyan
        az aks get-credentials --resource-group $deploymentResult.ResourceGroupName --name $deploymentResult.AksName --overwrite-existing
        Write-Host "Connected to AKS cluster." -ForegroundColor Green
    }
    catch {
        Write-Host "Failed to connect to AKS cluster." -ForegroundColor Red
        Write-Host "Error details:" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host $_.Exception.StackTrace -ForegroundColor Red
        exit 1
    }
    

    ###################################################################
    # 3. Create System Assigned Managed Identity for AKS
    ###################################################################
# Write-Host "*** TESTING DEPLOYMENT *** SKIPPING ROLE ASSIGNMENTS & IDENTITY FOR AKS" -ForegroundColor DarkRed
    # Get vmss Resource group Name
    $vmssResourceGroupName = $(az aks show --resource-group $deploymentResult.ResourceGroupName --name $deploymentResult.AksName --query nodeResourceGroup --output tsv)
    # Get vmss Name
    $vmssName = $(az vmss list --resource-group $vmssResourceGroupName --query "[0].name" --output tsv)
    # Create System Assigned Managed Identity for AKS
    $systemAssignedIdentity = $(az vmss identity assign --resource-group $vmssResourceGroupName --name $vmssName --query systemAssignedIdentity --output tsv)

    # Assign the role for aks system assigned managed identity to Azure blob Storage Data contributor role with the scope of the storage account
    az role assignment create --role "Storage Blob Data Contributor" --assignee $systemAssignedIdentity --scope "/subscriptions/$subscriptionID/resourceGroups/$($deploymentResult.ResourceGroupName)/providers/Microsoft.Storage/storageAccounts/$($deploymentResult.StorageAccountName)"

    # Assigne the role for aks system assigned managed identity to Azure queue data contributor role with the scope of the storage account
    az role assignment create --role "Storage Queue Data Contributor" --assignee $systemAssignedIdentity --scope "/subscriptions/$subscriptionID/resourceGroups/$($deploymentResult.ResourceGroupName)/providers/Microsoft.Storage/storageAccounts/$($deploymentResult.StorageAccountName)"

    # 3.Create namespace for AI Service
# Write-Host "*** TESTING DEPLOYMENT *** SKIPPING NEW KUBE NAMESPACE" -ForegroundColor DarkRed
    kubectl create namespace $kubenamepsace
# Write-Host "*** TESTING DEPLOYMENT *** SKIPPING APP ROUTING" -ForegroundColor DarkRed
    enable_app_routing
    
    
    # 5. Deploy nginx ingress public controller for dedicated public IP address
    # https://learn.microsoft.com/en-us/azure/aks/app-routing-nginx-configuration
    Write-Host "Deploy nginx ingress public controller for dedicated public IP address" -ForegroundColor Green
    
    Set-Location -Path $CWD
Write-Host ($(Get-CurrentLine)) "Current Path is $(Get-Location)"

    # 6.0 Update deploy.nginx-public-controller.yaml.template file and save as deploy.nginx-public-controller.yaml
$msg = "6.0. Update deploy.nginx-public-controller.yaml.template file and save as deploy.nginx-public-controller.yaml"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    $nginx_public_controllerTemplate = Get-Content -Path ../kubernetes/deploy.nginx-public-controller.yaml.template -Raw
    $nginx_public_controllerTemplate = $nginx_public_controllerTemplate -replace '{{ namespace }}', $kubenamepsace 
    $nginx_public_controllerPath = "../kubernetes/deploy.nginx-public-controller.yaml"
    $nginx_public_controllerTemplate | Set-Content -Path $nginx_public_controllerPath -Force
    
    kubectl apply -f ../kubernetes/deploy.nginx-public-controller.yaml
    # Get the public IP address for the public ingress controller
    $appRoutingNamespace = "app-routing-system"
    $externalIP
    while ($true) {
        $externalIP = Get-ExternalIP -serviceName 'nginx-public-0' -namespace $appRoutingNamespace
        if ($externalIP -and $externalIP -ne "<none>") {
            Write-Host "EXTERNAL-IP for nginx-public-0 in $appRoutingNamespace namespace is: $externalIP"
            break
        } else {
            Write-Host "Waiting for EXTERNAL-IP to be assigned..."
            Start-Sleep -Seconds 10
        }
    }
    # 6. Assign DNS Name to the public IP address
    #########################################################################################################################################
    # Step 6 : Update Kubernetes configuration files with the FQDN, Container Image Path and Email address for the certificate management
    $msg = "Assign DNS name to the public IP address"
    Write-Host "$('#' * 5) Step6 $($MyInvocation.MyCommand.Name) $('#' * 10) $(Get-CurrentLine) $msg" -ForegroundColor Yellow
    #########################################################################################################################################

Write-Host "$(Get-CurrentLine) got aksResourceGroupName: $aksResourceGroupName"
    #  6-1. Get Az Network resource Name with the public IP address`
$msg = "6.1. Get Az Network resource Name with the public IP address"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    Write-Host "Assign DNS Name to the public IP address" -ForegroundColor Green
    $publicIpName=$(az network public-ip list --query "[?ipAddress=='$externalIP'].name" --output tsv)
Write-Host "$(Get-CurrentLine) got publicIpName: $publicIpName"

    #  6-2. Generate Unique ESG API fqdn Name - 3 digit random number with padding 0
$msg = "6.2. Generate Unique ESG API fqdn Name - $kubenamepsace-<5-digit random number with padding 0>"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    $dnsName = "$kubenamepsace-$($(Get-Random -Minimum 0 -Maximum 999).ToString("D5"))"
Write-Host "$(Get-CurrentLine) got dnsName: $dnsName"

    #  6-3. Assign DNS Name to the public IP address
$msg = "6.3. Assign DNS Name to the public IP address"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    az network public-ip update --resource-group $aksResourceGroupName --name $publicIpName --dns-name $dnsName

    #  6-4. Get FQDN for the public IP address    
$msg = "6.4. Get FQDN for the public IP address"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    $fqdn = az network public-ip show --resource-group $aksResourceGroupName --name $publicIpName --query "dnsSettings.fqdn" --output tsv
    Write-Host "FQDN for the public IP address is: $fqdn" -ForegroundColor Green

    # 6.5 Update deploy.certclusterissuer.yaml.template file and save as deploy.certclusterissuer.yaml
$msg = "6.5. Update deploy.certclusterissuer.yaml.template file and save as deploy.certclusterissuer.yaml"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    $certManagerTemplate = Get-Content -Path ../kubernetes/deploy.certclusterissuer.yaml.template -Raw
    $certManagerTemplate = $certManagerTemplate -replace '{{ your-email }}', $email
    $certManagerTemplate = $certManagerTemplate -replace '{{ namespace }}', $kubenamepsace
    $certManagerPath = "../kubernetes/deploy.certclusterissuer.yaml"
    $certManagerTemplate | Set-Content -Path $certManagerPath -Force

    # 6.6 Update deploy.ingress.yaml.template file and save as deploy.ingress.yaml
$msg = "6.6. Update deploy.ingress.yaml.template file and save as deploy.ingress.yaml"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    $ingressTemplate = Get-Content -Path ../kubernetes/deploy.ingress.yaml.template -Raw
    $ingressTemplate = $ingressTemplate -replace '{{ fqdn }}', $fqdn
    $ingressTemplate = $ingressTemplate -replace '{{ ip_range }}', $ipRange
    $ingressTemplate = $ingressTemplate -replace '{{ namespace }}', $kubenamepsace
    $ingressPath = "../kubernetes/deploy.ingress.yaml"
    $ingressTemplate | Set-Content -Path $ingressPath -Force

    # 6.7 Update deploy.deployment.yaml.template file and save as deploy.deployment.yaml
$msg = "6.7. Update deploy.deployment.yaml.template file and save as deploy.deployment.yaml"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    $deploymentTemplate = Get-Content -Path ../kubernetes/deploy.deployment.yaml.template -Raw
    $deploymentTemplate = $deploymentTemplate -replace '{{ aiservice-imagepath }}', "$($deploymentResult.ContainerRegistryName).azurecr.io/$kubenamepsace/aiservice"
    $deploymentTemplate = $deploymentTemplate -replace '{{ kernelmemory-imagepath }}', "$($deploymentResult.ContainerRegistryName).azurecr.io/$kubenamepsace/kernelmemory"
    $deploymentTemplate = $deploymentTemplate -replace '{{ namespace }}', $kubenamepsace 
    $deploymentTemplate = $deploymentTemplate -replace '{{ instrumentationkey }}', "$($deploymentResult.AzAppInsightsInstrumentationKey)"
    $deploymentPath = "../kubernetes/deploy.deployment.yaml"
    $deploymentTemplate | Set-Content -Path $deploymentPath -Force


    # 6.8 Update deploy.service.yaml.template file and save as deploy.service.yaml
$msg = "6.8. Update deploy.service.yaml.template file and save as deploy.service.yaml"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    $serviceTemplate = Get-Content -Path ../kubernetes/deploy.service.yaml.template -Raw
    $serviceTemplate = $serviceTemplate -replace '{{ namespace }}', $kubenamepsace 
    $servicePath = "../kubernetes/deploy.service.yaml"
    $serviceTemplate | Set-Content -Path $servicePath -Force
$msg = "k8s configured"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue    
}
function Wait-ForCertManager {
    Write-Host "Waiting for Cert-Manager to be ready..." -ForegroundColor Cyan
    while ($true) {
        $certManagerPods = kubectl get pods -n cert-manager -l app.kubernetes.io/instance=cert-manager -o jsonpath='{.items[*].status.phase}'
        if ($certManagerPods -eq "Running Running Running") {
            Write-Host "Cert-Manager is running." -ForegroundColor Green
            break
        } else {
            Write-Host "Cert-Manager is not ready yet. Waiting..." -ForegroundColor Yellow
            Start-Sleep -Seconds 10
        }
    }
}

function upgrade_k8s() {
    # Update aks nodepools to updated new role
    try {
        Write-Host "Upgrading node pools..." -ForegroundColor Cyan
        $nextver = $(az aks get-upgrades --resource-group $deploymentResult.ResourceGroupName --name $deploymentResult.AksName --query "controlPlaneProfile.upgrades[0].kubernetesVersion" -o tsv)
        az aks upgrade --resource-group $deploymentResult.ResourceGroupName --name $deploymentResult.AksName -y -k $nextver
    }
    catch {
        Write-Host "$($MyInvocation.MyCommand.Name): Failed to upgrade node pools." -ForegroundColor Red
        Write-Host "Error details:" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host $_.Exception.StackTrace -ForegroundColor Red
        exit 1
    }
}

function depoy_k8s_certclusterissuer() {
    $attempt = 0
    $maxAttempts = 5
    $interval = 30
    Write-Host "7.2. Deploy ClusterIssuer in Kubernetes for SSL/TLS certificate" -ForegroundColor Cyan
    while( -not ($(kubectl apply -f ../kubernetes/deploy.certclusterissuer.yaml) -or ($attempt -eq $maxAttempts))) {
        ++$attempt
        Write-Host "kubectl apply certclusterissuer, attempt $attempt of $maxAttempts. Waiting..." -ForegroundColor Yellow
        Start-Sleep -Seconds $interval
    }
}
function deploy_k8s_deployment() {
    $attempt = 0
    $maxAttempts = 5
    $interval = 30
    Write-Host "7.3. Deploy Deployment in Kubernetes" -ForegroundColor Cyan
    while( -not ($(kubectl apply -f ../kubernetes/deploy.deployment.yaml) -or ($attempt -eq $maxAttempts))) {
        Write-Host "kubectl apply deployment, attempt $(++$attempt) of $maxAttempts. Waiting..." -ForegroundColor Yellow
        Start-Sleep -Seconds $interval
    }
}
function deploy_k8s_service() {
    $attempt = 0
    $maxAttempts = 5
    $interval = 30
    Write-Host "7.4. Deploy Services in Kubernetes" -ForegroundColor Cyan
    while( -not ($(kubectl apply -f ../kubernetes/deploy.service.yaml) -or ($attempt -eq $maxAttempts))) {
        Write-Host "kubectl apply service, attempt $(++$attempt) of $maxAttempts. Waiting..." -ForegroundColor Yellow
        Start-Sleep -Seconds $interval
    }
}
function deploy_k8s_ingress() {
    $attempt = 0
    $maxAttempts = 5
    $interval = 30
    Write-Host "7.5. Deploy Ingress Controller in Kubernetes for external access" -ForegroundColor Cyan
    while( -not ($(kubectl apply -f ../kubernetes/deploy.ingress.yaml) -or ($attempt -eq $maxAttempts))) {
        Write-Host "kubectl apply certclusterissuer, attempt $(++$attempt) of $maxAttempts. Waiting..." -ForegroundColor Yellow
        Start-Sleep -Seconds $interval
    }
}
function configure_aks() {
    ########################################################################################################################################################
    # Step 7 : Configure AKS (deploy Cert Manager, Ingress Controller) and Deploy Images on the kubernetes cluster
    Write-Host "Step 7 : Configure AKS (deploy Cert Manager, Ingress Controller) and Deploy Images on the kubernetes cluster" -ForegroundColor Yellow
    ########################################################################################################################################################

    Write-Host "Deploying Cert Manager and Ingress Controller in Kubernetes cluster" -ForegroundColor Green
    # 7.1. Install Cert Manager and nginx ingress controller in Kubernetes for SSL/TLS certificate
    # Install Cert-Manager
    Write-Host "7.1. Install Cert Manager and nginx ingress controller in Kubernetes for SSL/TLS certificate" -ForegroundColor Cyan
    helm repo add jetstack https://charts.jetstack.io --force-update
    kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.15.3/cert-manager.yaml
    
    # Wait for Cert-Manager to be ready
    Wait-ForCertManager

    # 7.2. Deploy ClusterIssuer in Kubernetes for SSL/TLS certificate
    depoy_k8s_certclusterissuer

    # 7.3. Deploy Deployment in Kubernetes
    deploy_k8s_deployment

    # 7.4. Deploy Services in Kubernetes
    deploy_k8s_service

    # 7.5. Deploy Ingress Controller in Kubernetes for external access
    deploy_k8s_ingress

}
function get_fqdn {
    param(
        [Parameter(Mandatory=$true,
        HelpMessage='Enter the public node name')]
        [string]$node
    )
    $node_resource_group=$(az aks show --resource-group $deploymentResult.ResourceGroupName --name $deploymentResult.AksName --query nodeResourceGroup --output tsv)
    $node_ip=$(kubectl get svc -n app-routing-system -o jsonpath="{.items[?(@.metadata.name=='$($node)')].status.loadBalancer.ingress[*].ip}")
    $node_ip_name=$(az network public-ip list --query "[?ipAddress=='$node_ip'].name" --output tsv)
    $domain_name=$(az network public-ip show --resource-group $node_resource_group --name $node_ip_name --query "dnsSettings.fqdn" --output tsv)
    return $domain_name
}

function closing_remarks() {
    #####################################################################
    # Step 8 : Display the deployment result and following instructions
    #####################################################################
    $FQDN = get_fqdn -Node "nginx-public-0"
    
    $messageString = "Deployment completed. Please find the deployment details below: `r`n" +
        "1. Check your Logic Apps Teams Channel connection `n`r" +
        "`t- Document Registration Process Watcher: $($deploymentResult.LogicAppDocumentProcessWatcherName) `n`r" +
        "`t- Benchmark Process Watcher: $($deploymentResult.LogicAppBenchmarkProcessWatcherName) `n`r" +
        "`t- GapAnalysis Process Watcher: $($deploymentResult.LogicAppGapAnalysisProcessWatcherName) `n`r" +
        "2. AKS Public Node 0 Ingress Load Balancer URL - https://$($FQDN) `n`r" +
        "3. Check GPT Model's TPM rate - Set each values high as much as you can set`n`r" +
        "`t- GPT4o Model - $($deploymentResult.AzGPT4oModelName) `n`r" +
        "`t- GPT4 32K Model - $($deploymentResult.AzGPT4_32KModelName) `n`r" +
        "`t- GPT Embedding Model - $($deploymentResult.AzGPTEmbeddingModelName) `n`r`n`r" +
        "`You may control the TPM rate in Azure Open AI Studio Deployments section."
    Write-Host $messageString -ForegroundColor Yellow
}

function validate_parms() {
    if ( -not ($subscriptionID -and $location -and $email -and $ipRange)) {
        Write-Error "Need subscriptionID, location, email, ipRange"
        exit 1
    }
    Write-Host "parameters found!" -ForegroundColor Green
}
###########################################################
# main()
###########################################################
$STAMP = $(Get-Date -Format "yyyyMMdd_T_HHmmss")
$CWD = $(Get-Location)
#####
$deploymentResult = [DeploymentResult]::new()
$is_testing = $false
$iac_dir = 'bicep'
if($is_testing) {
    $iac_dir += '-test'
}

try {
    Write-Host "Script start $(Get-Date -Format 'yyyyMMdd_T_HHmmss')"
    # Step1
    validate_parms
    LoginAzure($subscriptionID)
    $json = $(az account show --query "{Subscription:name,SubscriptionID:id,Type:user.type,User:user.name,Tenant:tenantId}" -o json)
    if ([string]::IsNullOrEmpty($json)) {
        throw [System.Exception]"NOLOGIN"
    }
    Write-Host "$($json | ConvertFrom-Json | ConvertTo-Json)" -ForegroundColor Green
    deploy_main_services

    # Step2
    get_service_info

    # Step3
    update_app_configs

    # Step4
    build_push_container_images

    # Step5
    configure_k8s
    upgrade_k8s

    # Step6
    ### REMOVED

    # Step7
    configure_aks

    # Step8
    closing_remarks

} catch {
    Write-Host "$($MyInvocation.MyCommand.Name): Error in main" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host $_.InvocationInfo.PositionMessage -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
} finally {
    Remove-Variable LOG -ErrorAction SilentlyContinue
    Remove-Variable LOGDIR -ErrorAction SilentlyContinue
    Remove-Variable RESULTS_OUT -ErrorAction SilentlyContinue
    Remove-Variable CWD -ErrorAction SilentlyContinue
    Remove-Variable deploymentResult -ErrorAction SilentlyContinue
    Remove-Variable iac_dir -ErrorAction SilentlyContinue
    Remove-Variable is_testing -ErrorAction SilentlyContinue
    Remove-Variable msg -ErrorAction SilentlyContinue
    Remove-Variable STAMP -ErrorAction SilentlyContinue
    Remove-Variable TIMESTAMP -ErrorAction SilentlyContinue

    Remove-Variable subscriptionID -ErrorAction SilentlyContinue
    Remove-Variable location -ErrorAction SilentlyContinue
    Remove-Variable email -ErrorAction SilentlyContinue
    Remove-Variable ipRange -ErrorAction SilentlyContinue
    Remove-Variable prefix -ErrorAction SilentlyContinue
    Remove-Variable appname -ErrorAction SilentlyContinue
    
    Remove-Variable json -ErrorAction SilentlyContinue

    Write-Host "Script complete $(Get-Date -Format 'yyyyMMdd_T_HHmmss')" -ForegroundColor DarkYellow
    try {
        Stop-Transcript
    } catch {}
}

