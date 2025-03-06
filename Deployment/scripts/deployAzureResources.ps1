# Copyright (c) Microsoft Corporation.
# Licensed under the MIT license.
param(
    [Parameter(
      HelpMessage='Enter the Azure subscription ID to deploy your resources')]
    [string]$subscriptionID = $env:AZSUB,

    [Parameter(
      HelpMessage='Enter the Azure Data Center Region to deploy your resources')]
    [ValidateSet(
        'EastUS', 'EastUS2', 'WestUS', 'WestUS2', 'WestUS3', 'CentralUS', 'NorthCentralUS', 'SouthCentralUS', 
        'WestEurope', 'NorthEurope', 'SoutheastAsia', 'EastAsia', 'JapanEast', 'JapanWest', 
        'AustraliaEast', 'AustraliaSoutheast', 'CentralIndia', 'SouthIndia', 'CanadaCentral', 
        'CanadaEast', 'UKSouth', 'UKWest', 'FranceCentral', 'FranceSouth', 'KoreaCentral', 
        'KoreaSouth', 'GermanyWestCentral', 'GermanyNorth', 'NorwayWest', 'NorwayEast', 
        'SwitzerlandNorth', 'SwitzerlandWest', 'UAENorth', 'UAECentral', 'SouthAfricaNorth', 
        'SouthAfricaWest', 'BrazilSouth', 'BrazilSoutheast', 'QatarCentral', 'ChinaNorth', 
        'ChinaEast', 'ChinaNorth2', 'ChinaEast2'
    )]
    [string]$location = $env:AZLOC,

    [Parameter(
      HelpMessage='Enter Your Email address for the certificate management')]
    [string]$email = $env:ADMIN_EMAIL,

    [Parameter(
      HelpMessage='Enter an IP range as comma separated list of CIDRs to allow access to the services')]
    [string]$ipRange = $env:ipRange
)
function Get-CurrentLine() {
    # $s = $(Get-Date -Format "yyyyMMddThhmmss")
    $s = $STAMP
    $n = $MyInvocation.ScriptLineNumber
    $f = Split-Path -Path $MyInvocation.ScriptName -Leaf
    return "${s}:${f}(${n})"
}
function LoginAzure([string]$subscriptionID) {
    Write-Host "$('#' * 10) $(Get-CurrentLine)::$($MyInvocation.MyCommand.Name)" -ForegroundColor Blue
    try {
        # Write-Host "Log in to Azure.....`r`n" -ForegroundColor Yellow
        # az login -t $env:AZTENANT
        # az account set --subscription $subscriptionID
        # Write-Host "Switched subscription to '$subscriptionID' `r`n" -ForegroundColor Yellow
        # $json = $(az account show --query "{Subscription:name,SubscriptionID:id,Type:user.type,User:user.name,Tenant:tenantId}" -o json)
        # $jsonObject = $json | ConvertFrom-Json
        # $jsonObject
    } catch {
        Write-Host "$($MyInvocation.MyCommand.Name): no login" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host $_.InvocationInfo.PositionMessage -ForegroundColor Red
        Write-Host $_.ScriptStackTrace -ForegroundColor Red
        exit 1
    }
}

function DeployAzureResources([string]$location) {
    Write-Host "$('#' * 10) $(Get-CurrentLine)::$($MyInvocation.MyCommand.Name)" -ForegroundColor Blue
    try {
        # Generate a random number between 0 and 99999
        $randomNumber = Get-Random -Minimum 0 -Maximum 99999
        # Pad the number with leading zeros to ensure it is 5 digits long
        $randomNumberPadded = $randomNumber.ToString("D5")

        # Perform a what-if deployment to preview changes
        Write-Host "Evaluating Deployment resource availabilities to preview changes..." -ForegroundColor Yellow
        $whatIfResult = az deployment sub what-if --template-file ../bicep/main_services.bicep -l $location -n "ESG_Document_Analysis_Deployment$randomNumberPadded"

        if ($LASTEXITCODE -ne 0) {
            Write-Host "There might be something wrong with your deployment." -ForegroundColor Red
            Write-Host $whatIfResult -ForegroundColor Red
            exit 1            
        }

        Write-Host "Deployment resource availabilities have been evaluated successfully." -ForegroundColor Green
        Write-Host "$(Get-CurrentLine) Staring subscription Deployment: ESG_Document_Analysis_Deployment$randomNumberPadded`r`n" -ForegroundColor Yellow
      
        # Make deployment name unique by appending random number ###  --parameters @bicep/main_services.json --parameters is_new=true
        $deploymentResult = az deployment sub create --template-file ../bicep/main_services.bicep -l $location -n "ESG_Document_Analysis_Deployment$randomNumberPadded"
Write-Host "$(Get-CurrentLine) az deployment sub create --template-file ../bicep/main_services.bicep -l $location -n \"ESG_Document_Analysis_Deployment$randomNumberPadded\"" -ForegroundColor DarkMagenta
Write-Host "$(Get-CurrentLine) `$deploymentResult is $deploymentResult"

        # $joinedString = $deploymentResult -join "" 
        # $jsonString = ConvertFrom-Json $joinedString 
Write-Host "$(Get-CurrentLine) ******************* `$jsonString:`n$jsonString"
        return $deploymentResult
    } catch {
        Write-Host "$($MyInvocation.MyCommand.Name):An error occurred during the deployment process:" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host $_.InvocationInfo.PositionMessage -ForegroundColor Red
        Write-Host $_.ScriptStackTrace -ForegroundColor Red
        exit 1
    }
}

function DisplayResult([pscustomobject]$jsonString) {
    Write-Host "$('#' * 10) $(Get-CurrentLine)::$($MyInvocation.MyCommand.Name)" -ForegroundColor Blue
    Write-Host "$(Get-CurrentLine) DisplayResult"
    $resourcegroupName = $jsonString.properties.outputs.gs_resourcegroup_name.value
    $storageAccountName = $jsonString.properties.outputs.gs_storageaccount_name.value
    $azsearchServiceName = $jsonString.properties.outputs.gs_azsearch_name.value
    $logicAppDocumentProcessWatcherName = $jsonString.properties.outputs.gs_logicapp_docregistprocesswatcher_name.value
    $logicAppBenchmarkProcessWatcherName = $jsonString.properties.outputs.gs_logicapp_benchmarkprocesswatcher_name.value
    $logicAppGapAnalysisProcessWatcherName = $jsonString.properties.outputs.gs_logicapp_ProcessWatcher_name.value
    $aksName = $jsonString.properties.outputs.gs_aks_name.value
    $containerRegistryName = $jsonString.properties.outputs.gs_containerregistry_name.value
    $azcognitiveserviceName = $jsonString.properties.outputs.gs_azcognitiveservice_name.value
    $azopenaiServiceName = $jsonString.properties.outputs.gs_openaiservice_name.value
    $azcosmosDBName = $jsonString.properties.outputs.gs_cosmosdb_name.value
    
    $azLogicAppDocumentRegistProcessWatcherUrl = $jsonString.properties.outputs.gs_logicapp_docregistprocesswatcher_endpoint.value
    $azLogicAppBenchmarkProcessWatcherUrl = $jsonString.properties.outputs.gs_logicapp_benchmarkprocesswatcher_endpoint.value
    $azLogicAppGapAnalysisProcessWatcherUrl = $jsonString.properties.outputs.gs_logicapp_docregistprocesswatcher_endpoint.value
    
    
    Write-Host "--------------------------------------------`r`n" -ForegroundColor White
    Write-Host "Deployment output: `r`n" -ForegroundColor White
    Write-Host "Subscription Id: $subscriptionID `r`n" -ForegroundColor Yellow
    Write-Host "ESG AI Document Analysis resource group: $resourcegroupName `r`n" -ForegroundColor Yellow
    Write-Host "Azure Kubernetes Account $aksName has been created" -ForegroundColor Yellow
    Write-Host "Azure Container Registry $containerRegistryName has been created" -ForegroundColor Yellow
    Write-Host "Azure Search Service $azsearchServiceName has been created" -ForegroundColor Yellow
    Write-Host "Azure Open AI Service $azopenaiServiceName has been created" -ForegroundColor Yellow
    Write-Host "Azure Cognitive Service $azcognitiveserviceName has been created" -ForegroundColor Yellow
    Write-Host "Azure Stroage Account $storageAccountName has been created" -ForegroundColor Yellow 
    Write-Host "Azure Cosmos DB $azcosmosDBName has been created " -ForegroundColor Yellow
    Write-Host "Document Registration Process Watcher Logic App $logicAppDocumentProcessWatcherName has been deployed " -ForegroundColor Yellow
    Write-Host "($azLogicAppDocumentRegistProcessWatcherUrl) " -ForegroundColor Yellow
    Write-Host "Benchmark Process Watcher $logicAppBenchmarkProcessWatcherName has been deployed" -ForegroundColor Yellow
    Write-Host "($azLogicAppBenchmarkProcessWatcherUrl) " -ForegroundColor Yellow
    Write-Host "GapAnalysis Process Watcher $logicAppGapAnalysisProcessWatcherName has been deployed" -ForegroundColor Yellow
    Write-Host "($azLogicAppGapAnalysisProcessWatcherUrl) " -ForegroundColor Yellow
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
        # Prefix
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
    Write-Host "$('#' * 10) $(Get-CurrentLine)::$($MyInvocation.MyCommand.Name)" -ForegroundColor Blue
    Write-Host "$(Get-CurrentLine) Get-LogicAppTriggerUrl"
    $url = "https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Logic/workflows/$logicAppName/triggers/$triggerName/listCallbackUrl?api-version=2016-06-01"
    $callbackUrl = az rest --method POST --uri $url --query "value" -o tsv
    return $callbackUrl
}

# replace placeholders in a template with actual values
function Invoke-PlaceholdersReplacement($template, $placeholders) {
    Write-Host "$('#' * 10) $(Get-CurrentLine)::$($MyInvocation.MyCommand.Name)" -ForegroundColor Blue
    Write-Host "$(Get-CurrentLine) Invoke-PlaceholdersReplacement"
    foreach ($key in $placeholders.Keys) {
        $template = $template -replace $key, $placeholders[$key]
    }
    return $template
}
# get the external IP address of a service
function Get-ExternalIP {
    param (
        [string]$serviceName,
        [string]$namespace
    )
    Write-Host "$('#' * 10) $(Get-CurrentLine)::$($MyInvocation.MyCommand.Name)" -ForegroundColor Blue
    Write-Host "$(Get-CurrentLine) Get-ExternalIP"
    $externalIP = kubectl get svc $serviceName -n $namespace -o jsonpath='{.status.loadBalancer.ingress[0].ip}'
    return $externalIP
}

function deploy_main_services() {
    ###############################################################
    # Step 1 : Deploy Azure resources
    $msg = "Deploy Azure resources"
    Write-Host "$('#' * 5) Step1 $($MyInvocation.MyCommand.Name) $('#' * 10) $(Get-CurrentLine) $msg" -ForegroundColor Yellow
    ###############################################################
  
Write-Host "$(Get-CurrentLine) `$deploymentResult is $deploymentResult"
    LoginAzure($subscriptionID)
    # Deploy Azure Resources
    Write-Host "Deploying Azure resources in $location region.....`r`n" -ForegroundColor Yellow
    $resultJson = DeployAzureResources($location)
    # Map the deployment result to DeploymentResult object
    $deploymentResult.MapResult($resultJson)
    # Display the deployment result
    DisplayResult($resultJson)

    return $resultJson
}
function get_service_info() {
    ###############################################################
    # Step 2 : Get Secrets from Azure resources
    $msg = "Get Secrets from Azure resources"
    Write-Host "$('#' * 5) Step2 $($MyInvocation.MyCommand.Name) $('#' * 10) $(Get-CurrentLine) $msg" -ForegroundColor Yellow
    ###############################################################
    # Get the storage account key
$msg = "az storage account keys list --account-name $deploymentResult.StorageAccountName --resource-group $deploymentResult.ResourceGroupName"
Write-Host ($(Get-CurrentLine)) $msg
    $storageAccountKey = az storage account keys list --account-name $deploymentResult.StorageAccountName --resource-group $deploymentResult.ResourceGroupName --query "[0].value" -o tsv

    ## Construct the connection string manually
$msg = "getting connection string"
Write-Host ($(Get-CurrentLine)) $msg
    $storageAccountConnectionString = "DefaultEndpointsProtocol=https;AccountName=$($deploymentResult.StorageAccountName);AccountKey=$storageAccountKey;EndpointSuffix=core.windows.net"

    ## Assign the connection string to the deployment result object
$msg = "setting connection string"
Write-Host ($(Get-CurrentLine)) $msg
    $deploymentResult.StorageAccountConnectionString = $storageAccountConnectionString    

    # Get the Azure Logic App workflow URLs
    $triggerName = "When_a_HTTP_request_is_received" # Logic App trigger name for all logic apps HTTP trigger

    ## Get Logic Workflow URL for Document Registration Process Watcher
$msg = "Get-LogicAppTriggerUrl -subscriptionId $subscriptionID -resourceGroupName $deploymentResult.ResourceGroupName -logicAppName $deploymentResult.LogicAppDocumentProcessWatcherName -triggerName $triggerName"
Write-Host ($(Get-CurrentLine)) $msg
    $deploymentResult.AzLogicAppDocumentRegistProcessWatcherUrl = Get-LogicAppTriggerUrl -subscriptionId $subscriptionID -resourceGroupName $deploymentResult.ResourceGroupName -logicAppName $deploymentResult.LogicAppDocumentProcessWatcherName -triggerName $triggerName

    ## Get Logic Workflow URL for Benchmark Process Watcher
$msg = "Get-LogicAppTriggerUrl -subscriptionId $subscriptionID -resourceGroupName $deploymentResult.ResourceGroupName -logicAppName $deploymentResult.LogicAppBenchmarkProcessWatcherName -triggerName $triggerName"
Write-Host ($(Get-CurrentLine)) $msg
    $deploymentResult.AzLogicAppBenchmarkProcessWatcherUrl = Get-LogicAppTriggerUrl -subscriptionId $subscriptionID -resourceGroupName $deploymentResult.ResourceGroupName -logicAppName $deploymentResult.LogicAppBenchmarkProcessWatcherName -triggerName $triggerName

    ## Get Logic Workflow URL for GapAnalysis Process Watcher
$msg = "Get-LogicAppTriggerUrl -subscriptionId $subscriptionID -resourceGroupName $deploymentResult.ResourceGroupName -logicAppName $deploymentResult.LogicAppGapAnalysisProcessWatcherName -triggerName $triggerName"
Write-Host ($(Get-CurrentLine)) $msg
    $deploymentResult.AzLogicAppGapAnalysisProcessWatcherUrl = Get-LogicAppTriggerUrl -subscriptionId $subscriptionID -resourceGroupName $deploymentResult.ResourceGroupName -logicAppName $deploymentResult.LogicAppGapAnalysisProcessWatcherName -triggerName $triggerName

    # Get MongoDB connection string
$msg = "az cosmosdb keys list --name $deploymentResult.AzCosmosDBName --resource-group $deploymentResult.ResourceGroupName --type connection-strings"
Write-Host ($(Get-CurrentLine)) $msg
    $deploymentResult.AzCosmosDBConnectionString = az cosmosdb keys list --name $deploymentResult.AzCosmosDBName --resource-group $deploymentResult.ResourceGroupName --type connection-strings --query "connectionStrings[0].connectionString" -o tsv

    # Get Azure Cognitive Service API Key
$msg = "az cognitiveservices account keys list --name $deploymentResult.AzCognitiveServiceName --resource-group $deploymentResult.ResourceGroupName"
Write-Host ($(Get-CurrentLine)) $msg
    $deploymentResult.AzCognitiveServiceKey = az cognitiveservices account keys list --name $deploymentResult.AzCognitiveServiceName --resource-group $deploymentResult.ResourceGroupName --query "key1" -o tsv

    # Get Azure Search Service Admin Key
$msg = "az search admin-key show --service-name $deploymentResult.AzSearchServiceName --resource-group $deploymentResult.ResourceGroupName"
Write-Host ($(Get-CurrentLine)) $msg
    $deploymentResult.AzSearchAdminKey = az search admin-key show --service-name $deploymentResult.AzSearchServiceName --resource-group $deploymentResult.ResourceGroupName --query "primaryKey" -o tsv

    # Get Azure Open AI Service API Key
$msg = "az cognitiveservices account keys list --name $deploymentResult.AzOpenAiServiceName --resource-group $deploymentResult.ResourceGroupName"
Write-Host ($(Get-CurrentLine)) $msg
    $deploymentResult.AzOpenAiServiceKey = az cognitiveservices account keys list --name $deploymentResult.AzOpenAiServiceName --resource-group $deploymentResult.ResourceGroupName --query "key1" -o tsv
    
}
function update_app_configs() {
    ######################################################################################################################
    # Step 3 : Update App Configuration files with Secrets and information for AI Service and Kernel Memory Service.
    $msg = "Update App Configuration files with Secrets and information for AI Service and Kernel Memory Service."
    Write-Host "$('#' * 5) Step3 $($MyInvocation.MyCommand.Name) $('#' * 10) $(Get-CurrentLine) $msg" -ForegroundColor Yellow
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
    $msg = "docker build and push container images to Azure Container Registry"
    Write-Host "$('#' * 5) Step4 $($MyInvocation.MyCommand.Name) $('#' * 10) $(Get-CurrentLine) $msg" -ForegroundColor Yellow
    ######################################################################################################################
    
    # 1. Login to Azure Container Registry
    az acr login --name $deploymentResult.ContainerRegistryName
    $acrNamespace = "esg-ai-docanalysis"
Write-Host ($(Get-CurrentLine)) "Current Path is $(Get-Location)"

    # 2. Build and push the images to Azure Container Registry
    #  2-1. Build and push the AI Service container image to  Azure Container Registry
    $acrAIServiceTag = "$($deploymentResult.ContainerRegistryName).azurecr.io/$acrNamespace/aiservice"
    Set-Location -Path "../../Services/src/esg-ai-doc-analysis/"
Write-Host ($(Get-CurrentLine)) "Current Path is $(Get-Location)"
    $docker = "docker build -t $acrAIServiceTag ."
Write-Host ($(Get-CurrentLine)) running: $docker
    Write-Host "skipping docker build" # Invoke-Expression $docker
Write-Host ($(Get-CurrentLine)) "pushing $acrAIServiceTag" -ForegroundColor Blue
    $docker = "docker push $acrAIServiceTag"
Write-Host ($(Get-CurrentLine)) running: $docker
    Write-Host "skipping docker push" # Invoke-Expression $docker
Write-Host ($(Get-CurrentLine)) "$acrAIServiceTag pushed" -ForegroundColor Green
    Set-Location -Path $CWD
Write-Host ($(Get-CurrentLine)) "Current Path is $(Get-Location)" -ForegroundColor Yellow

    #  2-2. Build and push the Kernel Memory Service container image to Azure Container Registry
    $acrKernelMemoryTag = "$($deploymentResult.ContainerRegistryName).azurecr.io/$acrNamespace/kernelmemory"
    Set-Location -Path "../../Services/src/kernel-memory/"
Write-Host ($(Get-CurrentLine)) "Current Path is $(Get-Location)"
    $docker = "docker build -t $acrKernelMemoryTag ."
Write-Host ($(Get-CurrentLine)) running: $docker
    Write-Host "skipping docker build" # Invoke-Expression $docker
Write-Host ($(Get-CurrentLine)) "pushing $acrKernelMemoryTag" -ForegroundColor Blue
    $docker = "docker push $acrKernelMemoryTag"
Write-Host ($(Get-CurrentLine)) running: $docker
    Write-Host "skipping docker push" # Invoke-Expression $docker
Write-Host ($(Get-CurrentLine)) "$acrKernelMemoryTag pushed" -ForegroundColor Green
    Set-Location -Path $CWD
Write-Host ($(Get-CurrentLine)) "Current Path is $(Get-Location)" -ForegroundColor Yellow
    
}
function configure_k8s() {
    ######################################################################################################################
    # Step 5 : Configure Kubernetes Infrastructure
    $msg = "Configure Kubernetes Infrastructure"
    Write-Host "$('#' * 5) Step5 $($MyInvocation.MyCommand.Name) $('#' * 10) $(Get-CurrentLine) $msg" -ForegroundColor Yellow
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
            $msg = "Attempt to update the AKS cluster"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
$aks = $deploymentResult.AksNam
$rg = $deploymentResult.ResourceGroupName
$acr = $deploymentResult.ContainerRegistryName
Write-Host "$(Get-CurrentLine) az aks update --name $aks --resource-group $rg --attach-acr $acr" -ForegroundColor DarkMagenta
            az aks update --name $deploymentResult.AksName --resource-group $deploymentResult.ResourceGroupName --attach-acr $deploymentResult.ContainerRegistryName
            $msg = "AKS cluster updated successfully."
Write-Host ($(Get-CurrentLine)) $msg -ForegroundColor Green
            break
        } catch {
            $errorMessage = $_.Exception.Message
            if ($errorMessage -match "OperationNotAllowed" -and $errorMessage -match "Another operation \(Updating\) is in progress") {
                $msg = "Operation not allowed: Another operation is in progress. Retrying in $delay seconds..."
Write-Host ($(Get-CurrentLine)) $msg -ForegroundColor Yellow
                Start-Sleep -Seconds $delay
                $retryCount++
            } else {
Write-Host ($(Get-CurrentLine)) "An unexpected error occurred: $errorMessage" -ForegroundColor Red
                throw $_
            }
        }
    }

    if ($retryCount -eq $maxRetries) {
        Write-Host "Max retries reached. Failed to update the AKS cluster." -ForegroundColor Red
        exit 1
    }

    $kubenamepsace = "ns-esg-docanalysis"

    # 1. Get the Kubernetes resource group
    $aksResourceGroupName = $(az aks show --resource-group $deploymentResult.ResourceGroupName --name $deploymentResult.AksName --query nodeResourceGroup --output tsv)
Write-Host "$(Get-CurrentLine) got aksResourceGroupName: $aksResourceGroupName"

    try {
        Write-Host "Getting the Kubernetes resource group..." -ForegroundColor Cyan
$rg = $deploymentResult.ResourceGroupName
$aks = $deploymentResult.AksName
Write-Host "$(Get-CurrentLine) az aks show --resource-group $rg --name $aks --query nodeResourceGroup --output tsv"
        Write-Host "Kubernetes resource group: $aksResourceGroupName" -ForegroundColor Green
    }
    catch {
        Write-Host "$($MyInvocation.MyCommand.Name): Failed to get the Kubernetes resource group." -ForegroundColor Red
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
        Write-Host "$($MyInvocation.MyCommand.Name): Failed to connect to AKS cluster." -ForegroundColor Red
        Write-Host "Error details:" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host $_.Exception.StackTrace -ForegroundColor Red
        exit 1
    }

    ###################################################################
    # 3. Create System Assigned Managed Identity for AKS
    ###################################################################
    # Get vmss Resource group Name
    $rg = $deploymentResult.ResourceGroupName
    $aks = $deploymentResult.AksName
Write-Host "$(Get-CurrentLine) az aks show --resource-group $rg --name $aks --query nodeResourceGroup --output tsv" -ForegroundColor DarkMagenta
    $vmssResourceGroupName = $(az aks show --resource-group $deploymentResult.ResourceGroupName --name $deploymentResult.AksName --query nodeResourceGroup --output tsv)
Write-Host ($(Get-CurrentLine)) vmssResourceGroupName: $vmssResourceGroupName -ForegroundColor Cyan

    # Get vmss Name
Write-Host "$(Get-CurrentLine) az vmss list --resource-group $vmssResourceGroupName --query '[0].name' --output tsv" -ForegroundColor DarkMagenta
    $vmssName = $(az vmss list --resource-group $vmssResourceGroupName --query "[0].name" --output tsv)
Write-Host ($(Get-CurrentLine)) vmssName: $vmssName -ForegroundColor Cyan

    # Create System Assigned Managed Identity for AKS
Write-Host "$(Get-CurrentLine) az vmss identity assign --resource-group $vmssResourceGroupName --name $vmssName --query systemAssignedIdentity --output tsv" -ForegroundColor DarkMagenta
    $systemAssignedIdentity = $(az vmss identity assign --resource-group $vmssResourceGroupName --name $vmssName --query systemAssignedIdentity --output tsv)
Write-Host ($(Get-CurrentLine)) systemAssignedIdentity: $systemAssignedIdentity -ForegroundColor Cyan

    # Assign the role for aks system assigned managed identity to Azure blob Storage Data contributor role with the scope of the storage account
    az role assignment create --role "Storage Blob Data Contributor" --assignee $systemAssignedIdentity --scope "/subscriptions/$subscriptionID/resourceGroups/$($deploymentResult.ResourceGroupName)/providers/Microsoft.Storage/storageAccounts/$($deploymentResult.StorageAccountName)"

    # Assigne the role for aks system assigned managed identity to Azure queue data contributor role with the scope of the storage account
    az role assignment create --role "Storage Queue Data Contributor" --assignee $systemAssignedIdentity --scope "/subscriptions/$subscriptionID/resourceGroups/$($deploymentResult.ResourceGroupName)/providers/Microsoft.Storage/storageAccounts/$($deploymentResult.StorageAccountName)"


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

    # 3.Create namespace for AI Service
    kubectl create namespace $kubenamepsace
    
    Write-Host "Enable Add routing addon for AKS" -ForegroundColor Blue
    # 4.approuting enable and enable addons for http_application_routing
    Import-Module ../kubernetes/enable_approuting.psm1
    Enable-AppRouting -ResourceGroupName $deploymentResult.ResourceGroupName -ClusterName $deploymentResult.AksName
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
    
    # 5. Deploy nginx ingress public controller for dedicated public IP address
    # https://learn.microsoft.com/en-us/azure/aks/app-routing-nginx-configuration
    Write-Host "Deploy nginx ingress public controller for dedicated public IP address" -ForegroundColor Green
    
    Set-Location -Path $CWD
Write-Host ($(Get-CurrentLine)) "Current Path is $(Get-Location)" -ForegroundColor Blue
    kubectl apply -f ../kubernetes/deploy.nginx-public-contoller.yaml
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
Write-Host "$(Get-CurrentLine) got aksResourceGroupName: $aksResourceGroupName"
    $msg = "6.1. Get Az Network resource Name with the public IP address"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    Write-Host "Assign DNS Name to the public IP address" -ForegroundColor Green
    $publicIpName=$(az network public-ip list --query "[?ipAddress=='$externalIP'].name" --output tsv)
Write-Host "$(Get-CurrentLine) got publicIpName: $publicIpName"

    $msg = "6.2. Generate Unique ESG API fqdn Name - esgdocanalysis-3 digit random number with padding 0"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    $dnsName = "esgdocanalysis-$($(Get-Random -Minimum 0 -Maximum 999).ToString("D3"))"
Write-Host "$(Get-CurrentLine) got dnsName: $dnsName"

    $msg = "6.3. Assign DNS Name to the public IP address"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    az network public-ip update --resource-group $aksResourceGroupName --name $publicIpName --dns-name $dnsName

    $msg = "6.4. Get FQDN for the public IP address"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    $fqdn = az network public-ip show --resource-group $aksResourceGroupName --name $publicIpName --query "dnsSettings.fqdn" --output tsv
    Write-Host "FQDN for the public IP address is: $fqdn" -ForegroundColor Green

# function Step6() {

    #########################################################################################################################################
    # Step 6 : Update Kubernetes configuration files with the FQDN, Container Image Path and Email address for the certificate management
    $msg = "Update Kubernetes configuration files with the FQDN, Container Image Path and Email address for the certificate management"
    Write-Host "$('#' * 5) $($MyInvocation.MyCommand.Name) $('#' * 10) $(Get-CurrentLine) $msg" -ForegroundColor Yellow
    #########################################################################################################################################

    $msg = "6.5 Update deploy.certclusterissuer.yaml.template file and save as deploy.certclusterissuer.yaml"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    $certManagerTemplate = Get-Content -Path ../kubernetes/deploy.certclusterissuer.yaml.template -Raw
    $certManagerTemplate = $certManagerTemplate -replace '{{ your-email }}', $email
    $certManagerPath = "../kubernetes/deploy.certclusterissuer.yaml"
    $certManagerTemplate | Set-Content -Path $certManagerPath -Force

    $msg = "6.6 Update deploy.ingress.yaml.template file and save as deploy.ingress.yaml"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    $ingressTemplate = Get-Content -Path ../kubernetes/deploy.ingress.yaml.template -Raw
    $ingressTemplate = $ingressTemplate -replace '{{ fqdn }}', $fqdn
    $ingressTemplate = $ingressTemplate -replace '{{ ip_range }}', $ipRange
    $ingressPath = "../kubernetes/deploy.ingress.yaml"
    $ingressTemplate | Set-Content -Path $ingressPath -Force

    $msg = "6.7 Update deploy.deployment.yaml.template file and save as deploy.deployment.yaml"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    $deploymentTemplate = Get-Content -Path ../kubernetes/deploy.deployment.yaml.template -Raw
    $deploymentTemplate = $deploymentTemplate -replace '{{ aiservice-imagepath }}', "$($deploymentResult.ContainerRegistryName).azurecr.io/$acrNamespace/aiservice"
    $deploymentTemplate = $deploymentTemplate -replace '{{ kernelmemory-imagepath }}', "$($deploymentResult.ContainerRegistryName).azurecr.io/$acrNamespace/kernelmemory"
    $deploymentPath = "../kubernetes/deploy.deployment.yaml"
    $deploymentTemplate | Set-Content -Path $deploymentPath -Force

}
function Wait-ForCertManager {
    Write-Host "$('#' * 5) $($MyInvocation.MyCommand.Name) $('#' * 10) $(Get-CurrentLine)" -ForegroundColor Blue
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

function configure_aks() {
    ########################################################################################################################################################
    # Step 7 : Configure AKS (deploy Cert Manager, Ingress Controller) and Deploy Images on the kubernetes cluster
    $msg = "Configure AKS (deploy Cert Manager, Ingress Controller) and Deploy Images on the kubernetes cluster"
    Write-Host "$('#' * 5) Step7 $($MyInvocation.MyCommand.Name) $('#' * 10) $(Get-CurrentLine) $msg" -ForegroundColor Yellow
    ########################################################################################################################################################

    Write-Host "Deploying Cert Manager and Ingress Controller in Kubernetes cluster" -ForegroundColor Green
    $msg = "7.1. Install Cert Manager and nginx ingress controller in Kubernetes for SSL/TLS certificate"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    # Install Cert-Manager
    $msg = "Deploying with helm"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Green
    helm repo add jetstack https://charts.jetstack.io --force-update
    kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.15.3/cert-manager.yaml
    
    # Wait for Cert-Manager to be ready
    Wait-ForCertManager

    $msg = "7.2. Deploy ClusterIssuer in Kubernetes for SSL/TLS certificate"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    kubectl apply -f ../kubernetes/deploy.certclusterissuer.yaml

    $msg = "7.3. Deploy Deployment in Kubernetes"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    kubectl apply -f ../kubernetes/deploy.deployment.yaml

    $msg = "7.4. Deploy Services in Kubernetes"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    kubectl apply -f ../kubernetes/deploy.service.yaml

    $msg = "7.5. Deploy Ingress Controller in Kubernetes for external access"
Write-Host "$(Get-CurrentLine) $msg" -ForegroundColor Blue
    kubectl apply -f ../kubernetes/deploy.ingress.yaml

}
function closing_remarks() {
    #####################################################################
    # Step 8 : Display the deployment result and following instructions
    $msg = "Deployment completed successfully."
    Write-Host "$('#' * 5) $($MyInvocation.MyCommand.Name) $('#' * 10) $(Get-CurrentLine) $msg" -ForegroundColor Green
    #####################################################################
    $messageString = "Deployment completed successfully. Please find the deployment details below: `r`n" +
        "1. Check your Logic Apps Teams Channel connection `n`r" +
        "`t- Document Registration Process Watcher: $($deploymentResult.LogicAppDocumentProcessWatcherName) `n`r" +
        "`t- Benchmark Process Watcher: $($deploymentResult.LogicAppBenchmarkProcessWatcherName) `n`r" +
        "`t- GapAnalysis Process Watcher: $($deploymentResult.LogicAppGapAnalysisProcessWatcherName) `n`r" +
        "2. Check API Service Endpoint with this URL - https://$($fqdn) `n`r" +
        "3. Check GPT Model's TPM rate - Set each values high as much as you can set`n`r" +
        "`t- GPT4o Model - $($deploymentResult.AzGPT4oModelName) `n`r" +
        "`t- GPT4 32K Model - $($deploymentResult.AzGPT4_32KModelName) `n`r" +
        "`t- GPT Embedding Model - $($deploymentResult.AzGPTEmbeddingModelName) `n`r`n`r" +
        "`You may control the TPM rate in Azure Open AI Studio Deployments section."
    Write-Host $messageString -ForegroundColor Yellow
}

if ( -not ($subscriptionID -and $location -and $email -and $ipRange)) {
  Write-Error "Need subscriptionID, location, email, ipRange"
  throw $_
}
###########################################################
# main()
###########################################################
$LOG="~/log/deployAzureResources.20250305_T_000003.log"
$CWD = $(Get-Location)
#####
Start-Transcript -Path $LOG -Append -NoClobber
$STAMP = $(Get-Date -Format "yyyyMMdd_T_hhmmss")
$msg = $null
Write-Host "***** START $('*' * 30) ($(Get-CurrentLine)) $msg" -ForegroundColor Green

$deploymentResult = [DeploymentResult]::new()
$json = $null
$filePath = "deploymentResult.json"
try {
    If ( -not (Test-Path -Path $filePath)) {
      # Step1
      $json = deploy_main_services
      # SAVE JSON FOR LATER, CAREFUL NOT TO OVERWRITE
      $filePath = "deploymentResult.$($deploymentResult.resourceprefix).json"
Write-Host "deploymentResult 2 $filePath"
      Set-Content -Path $filePath -Value $json && Write-Host "$filePath written $($deploymentResult.resourceprefix)"
    }
    # READ THE CONTENTS OF THE FILE AND STORE IT IN A STRING VARIABLE
    $json = Get-Content -Path $filePath -Raw
    $jsonObject = $json | ConvertFrom-Json
    $deploymentResult.MapResult($jsonObject)

    # Step2
    get_service_info

    # Step3
    update_app_configs

    # Step4
    build_push_container_images

    # Step5
    configure_k8s

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
  Stop-Transcript
}

