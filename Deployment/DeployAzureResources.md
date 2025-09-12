# Deployment Guide for Services

> This repository provides a solution and reference architecture for AI-driven comparative analysis of sustainability reports.  
>  
>  **Note:** The provided code is for demonstration purposes only and is not an officially supported Microsoft product.  
>  
> For improved security, consider integrating [Azure API Management with microservices deployed in Azure Kubernetes Service (AKS)](https://learn.microsoft.com/en-us/azure/api-management/api-management-kubernetes).

---

## Contents

- [Prerequisites](#prerequisites)  
- [Regional Availability](#regional-availability)  
- [Deploy to Azure](#deploy-to-azure)  
- [Post-Deploy Configuration](#post-deploy-configuration)  
- [Troubleshooting](#troubleshooting)  
- [Next Steps](#next-steps)  
  - [Test APIs](./docs/TestApis.md)  
  - [Deploy Power Platform Client](./DeployPowerPlatformClient.md)  

---

## ✅ Prerequisites

Before deploying the solution, ensure you have the following tools and access in place:

1.  **[Git](https://git-scm.com/downloads)**  
  Required to clone the project repository.

2.  **[PowerShell (v5.1+)](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell)**  
  Script execution environment, available on Windows, macOS, and Linux.

3.  **[Azure CLI (v2.0+)](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)**  
  Command-line tool to manage Azure resources.

  - **kubectl** – CLI for interacting with Kubernetes clusters.  
    Install via PowerShell:
    ```powershell
    az aks install-cli
    ```

  - **aks-preview** – Azure CLI extension to enable AKS advanced features.  
    Install via PowerShell:
    ```powershell
    az extension add --name aks-preview
    ```

4.  **[Helm](https://helm.sh/docs/intro/install/)**  
  Kubernetes package manager used for deploying services.

5.  **[Docker Desktop](https://docs.docker.com/get-docker/)**  
  Required to containerize applications and publish to Azure Container Registry.  
  >💡 Make sure Docker Desktop is running before executing deployment scripts.

6.  **Azure Access**  
  You must have a **subscription-level** role of either:  
  - `Owner` or  
  - `User Access Administrator`

---

## Regional Availability

*Some services in this solution are restricted to specific Azure regions due to model availability.*

- **Azure OpenAI (Sweden Central)**  
  This solution uses the following models and deploys in the Sweden Central region:
    -	gpt-4o – Model version 2024-11-20
    -	text-embedding-3-large
  
  *Note: OpenAI models are periodically updated, and older models may be deprecated or retired. Always refer to the [model availability table](https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models)  to ensure compatibility and continued availability in your region.*

- **Azure AI Document Intelligence (East US)**  
  Requires API version `2023-10-31-preview` or later, currently available in **East US**.  
  For more details, see the [troubleshooting guide](./TROUBLESHOOTING.md).



## Deploy to Azure

The automated deployment process is streamlined and easy to use, executed through a single [deployment script](./scripts/deployAzureResources.ps1) that completes in roughly 10–15 minutes.

### Automated Deployment Steps:
1. Deploy Azure resources.
2. Get secret information from Azure resources.
3. Update application configuration files with secrets.
4. Compile application, build image, and push to Azure Container Registry.
5. Configure Kubernetes cluster infrastructure.
6. Update Kubernetes configuration files.
7. Deploy certificates, ingress controller, and application service images.
8. Deploy and integrate Power Platform.


## Step 1 Run Deployment:
### Step 1.1 Clone the repository

Before starting the deployment, clone the project repository to your local machine.

1. **Open your terminal or command prompt**  
   (On Windows, you can use Command Prompt or Git Bash. On Mac/Linux, use Terminal.)

2. **Navigate to the folder where you want to save the project**  
   Example:
   ```bash
   cd Documents
3. **Run the following command to clone the repository**
    ```bash
    git clone https://github.com/microsoft/Comparative-Analysis-for-Sustainability-Solution-Accelerator.git
4. **Move into the project folder**
    ```bash
    cd Comparative-Analysis-for-Sustainability-Solution-Accelerator
### Step 1.2 Run Deployment Script 
Once the repository is cloned, follow these steps to deploy the required Azure resources:
1. Open PowerShell, change directory to script location
```powershell
    cd .\Deployment\scripts\
```
2. Run the deployment script 
```powershell
    .\deployAzureResources.ps1
```
>Note: If you encounter an error about the script not being digitally signed, you can use the following command instead:
```
powershell.exe -ExecutionPolicy Bypass -File ".\deployAzureResources.ps1"
```
3. Enter Required Parameters

    When you run the deployment script, you will be prompted to provide the following parameters:
    
    +  **Subscription ID** - Your Azure Subscription ID (copy/paste from the Azure portal).

    +  **Environment Name** - A unique environment name (e.g., dev, test, prod).This is used to scope resource names and group deployments logically.
    
    +  **Resource Group Name** - The Azure resource group to deploy resources into.You may either:
    
       - Specify an existing resource group to reuse it, [see below](#configuring-a-new-or-existing-resource-group) for more details, or
       - Leave blank to auto-generate a new name.
   
   +    **Location** - Azure data center where resources will be deployed. Please [check Azure resource availability and note hardcoded regions](#regional-availability). The following locations are currently supported: 
        
        ```
        'EastUS', 'EastUS2', 'WestUS', 'WestUS2', 'WestUS3', 'CentralUS', 'NorthCentralUS', 'SouthCentralUS','WestEurope', 'NorthEurope', 'SoutheastAsia', 'EastAsia', 'JapanEast', 'JapanWest', 'AustraliaEast', 'AustraliaSoutheast', 'CentralIndia', 'SouthIndia', 'CanadaCentral','CanadaEast', 'UKSouth', 'UKWest', 'FranceCentral', 'FranceSouth', 'KoreaCentral','KoreaSouth', 'GermanyWestCentral', 'GermanyNorth', 'NorwayWest', 'NorwayEast', 'SwitzerlandNorth', 'SwitzerlandWest', 'UAENorth', 'UAECentral', 'SouthAfricaNorth','SouthAfricaWest', 'BrazilSouth','BrazilSoutheast', 'QatarCentral', 'ChinaNorth', 'ChinaEast', 'ChinaNorth2', 'ChinaEast2'
        ```
     + **Email** - used for issuing certificates in Kubernetes clusters Via [Let's Encrypt](https://letsencrypt.org/) service. 
        ><span style="color:green">Note: Use a valid email that is the same email used to set up your identity in your target Azure Tenant.</span>
    
     + **IP Range Allow-list** - This is used to restrict access to the API service endpoints. To enable front-end cloud flows to access the service, you need to add the [Power Automate (Azure Logic Apps) outbound IP addresses](https://learn.microsoft.com/en-us/azure/logic-apps/logic-apps-limits-and-config?tabs=consumption#outbound-ip-addresses) for the region where your flows are deployed, in CIDR notation. For example, if your Power Platform environment is in the US region, you must allow the outbound Azure Logic Apps IPs for all US sub-regions.You can refer to the official [Azure Service Tags JSON](https://www.microsoft.com/en-us/download/details.aspx?id=56519) as well. The example provided includes the extracted Logic Apps + Power Platform Infra IP ranges for US regions.

        ```
        13.86.98.126/32,13.89.178.48/28 ,20.109.202.29/32 ,20.109.202.36/31 ,20.118.195.224/27 ,52.141.218.55/32 ,52.141.221.6/32 ,52.182.141.160/27 ,172.212.239.224/28 ,2603:1030:10:402::3c0/124 ,2603:1030:10:402::3e0/123 ,20.45.245.152/29 ,20.45.245.160/28 ,20.45.245.176/29 ,40.78.204.208/28 ,40.78.204.224/27 ,172.215.35.112/28 ,2603:1030:f:400::bc0/124 ,2603:1030:f:400::be0/123 ,4.156.25.14/32 ,4.156.25.188/31 ,4.156.26.80/32 ,4.156.27.7/32 ,4.156.28.117/32 ,4.156.241.47/32 ,4.156.241.165/32 ,4.156.241.183/32 ,4.156.241.191/32 ,4.156.241.195/32 ,4.156.241.229/32 ,4.156.242.12/31 ,4.156.242.26/32 ,4.156.242.49/32 ,4.156.242.86/32 ,4.156.242.92/32 ,4.156.242.96/31 ,4.156.243.164/31 ,4.156.243.170/32 ,4.156.243.172/31 ,4.156.243.174/32 ,20.42.64.48/28 ,20.42.72.160/27 ,20.84.29.18/32 ,20.84.29.29/32 ,20.84.29.150/32 ,20.88.159.144/29 ,20.88.159.160/27 ,20.242.168.24/32 ,20.242.168.44/32 ,40.76.148.50/32 ,40.76.151.25/32 ,40.76.151.124/32 ,40.76.174.39/32 ,40.76.174.83/32 ,40.76.174.148/32 ,52.224.145.30/32 ,52.224.145.162/32 ,52.226.216.187/32 ,52.226.216.197/32 ,52.226.216.209/32 ,57.152.113.64/28 ,172.212.32.196/32 ,172.212.37.35/32 ,2603:1030:210:402::3c0/124 ,2603:1030:210:402::3e0/123 ,4.152.125.62/32 ,4.152.126.158/32 ,4.152.127.229/32 ,4.152.127.230/32 ,4.152.128.205/32 ,4.152.128.227/32 ,4.152.128.241/32 ,4.152.129.54/32 ,4.152.129.221/32 ,4.152.129.229/32 ,4.153.159.226/32 ,4.153.194.246/32 ,4.153.195.56/32 ,4.153.201.239/32 ,4.153.201.240/32 ,20.1.240.241/32 ,20.44.17.224/27 ,20.96.58.28/32 ,20.96.58.139/32 ,20.96.58.140/32 ,20.96.89.48/32 ,20.96.89.54/32 ,20.96.89.98/32 ,20.96.89.234/32 ,20.96.89.254/32 ,20.96.90.28/32 ,20.98.195.0/27 ,20.98.195.32/29 ,20.122.237.189/32 ,20.122.237.191/32 ,20.122.237.205/32 ,20.122.237.221/32 ,20.122.237.225/32 ,20.122.237.232/32 ,40.84.77.224/28 ,52.167.109.80/28 ,52.251.20.210/32 ,52.253.79.47/32 ,2603:1030:40c:402::3c0/124 ,2603:1030:40c:402::3e0/123 ,20.47.236.96/27 ,20.47.249.56/32 ,20.47.249.65/32 ,20.47.249.165/32 ,20.102.164.100/32 ,20.102.166.9/32 ,20.102.166.10/32 ,40.74.149.96/27 ,40.75.35.240/28 ,68.220.127.128/28 ,2603:1030:40b:400::bc0/124 ,2603:1030:40b:400::be0/123 ,40.67.60.176/28 ,40.67.60.192/27 ,40.67.88.80/28 ,40.67.92.0/28 ,2603:1030:104:402::3c0/124 ,2603:1030:104:402::3e0/123 ,20.51.6.176/28 ,20.51.6.192/28 ,20.80.33.190/32 ,20.88.47.77/32 ,20.88.49.21/32 ,20.88.49.23/32 ,20.88.51.31/32 ,20.88.55.77/32 ,20.98.61.245/32 ,20.102.251.70/32 ,20.102.255.209/32 ,20.102.255.252/32 ,23.101.160.111/32 ,23.101.167.207/32 ,23.101.174.98/32 ,40.116.64.218/32 ,40.116.65.34/32 ,40.116.65.125/32 ,40.116.66.226/32 ,52.162.111.144/28 ,52.162.111.160/27 ,52.162.177.30/32 ,52.162.177.90/32 ,52.162.177.104/32 ,172.183.48.9/32 ,172.183.48.31/32 ,172.183.48.234/32 ,172.183.48.255/32 ,172.183.49.208/32 ,172.183.50.30/32 ,172.183.50.180/32 ,172.183.51.138/32 ,172.183.51.180/32 ,172.183.52.146/32 ,172.183.233.80/28 ,2603:1030:608:402::3c0/124 ,2603:1030:608:402::3e0/123 ,13.65.39.247/32 ,13.65.82.190/32 ,13.65.86.56/32 ,13.73.244.144/28 ,13.73.244.160/27 ,13.84.159.168/32 ,13.85.79.155/32 ,20.88.209.88/32 ,20.88.209.97/32 ,20.88.209.113/32 ,20.94.149.199/32 ,20.94.150.220/32 ,20.94.151.41/32 ,20.94.168.143/32 ,20.94.168.235/32 ,20.94.168.241/32 ,20.97.39.48/29 ,20.97.39.64/27 ,23.101.191.106/32 ,52.152.91.74/32 ,52.152.91.77/32 ,52.249.31.63/32 ,52.255.124.118/32 ,52.255.126.229/32 ,52.255.127.24/32 ,52.255.127.125/32 ,52.255.127.201/32 ,52.255.127.211/32 ,52.255.127.233/32 ,52.255.127.243/32 ,104.210.153.89/32 ,172.206.187.57/32 ,172.206.187.62/32 ,172.206.187.90/32 ,172.206.187.92/32 ,172.206.187.98/32 ,172.206.187.101/32 ,172.206.187.132/32 ,172.206.187.135/32 ,172.215.135.240/28 ,2603:1030:807:402::3c0/124 ,2603:1030:807:402::3e0/123 ,48.216.12.32/28 ,48.216.12.64/27 ,48.216.48.80/28 ,48.216.48.96/27 ,2603:1030:1102:3::6f0/124 ,2603:1030:1102:4::2c0/123 ,2603:1030:1102:400::340/124 ,2603:1030:1102:400::360/123 ,20.44.4.176/28 ,20.44.4.192/27 ,23.100.223.16/28 ,57.152.135.32/28 ,13.64.215.62/32 ,13.64.224.17/32 ,13.64.231.196/32 ,13.64.236.166/32 ,13.64.236.222/32 ,13.64.237.74/32 ,13.64.239.120/32 ,13.64.240.133/32 ,13.64.241.219/32 ,13.64.243.209/32 ,13.83.10.112/32 ,13.83.10.141/32 ,13.83.14.75/32 ,13.83.15.162/32 ,13.86.209.140/32 ,13.86.221.240/28 ,13.86.223.0/27 ,13.87.204.182/32 ,13.87.204.210/32 ,13.87.207.39/32 ,13.87.207.79/32 ,13.88.18.200/32 ,13.88.19.4/32 ,13.88.56.138/32 ,13.88.168.158/32 ,13.88.169.158/32 ,13.88.169.213/32 ,13.88.171.218/32 ,13.88.177.160/32 ,13.91.17.147/32 ,13.91.18.168/32 ,13.91.20.94/32 ,13.91.33.16/32 ,13.91.35.39/32 ,13.91.40.58/32 ,13.91.40.136/32 ,13.91.46.132/32 ,13.91.46.140/32 ,13.91.65.198/32 ,13.91.70.215/32 ,13.91.81.69/32 ,13.91.81.188/32 ,13.91.81.221/32 ,13.91.87.195/32 ,13.91.102.122/32 ,13.91.103.48/32 ,13.91.105.180/32 ,13.91.130.111/32 ,13.91.231.159/32 ,13.91.247.104/32 ,13.91.247.124/32 ,13.93.161.57/32 ,13.93.163.29/32 ,13.93.167.155/32 ,13.93.177.253/32 ,13.93.179.126/32 ,13.93.180.131/32 ,13.93.180.161/32 ,13.93.180.221/32 ,13.93.183.170/32 ,13.93.193.254/32 ,13.93.203.72/32 ,13.93.211.175/32 ,13.93.215.87/32 ,13.93.223.133/32 ,13.93.233.226/32 ,13.93.239.25/32 ,13.93.239.166/32 ,20.59.80.224/27 ,20.59.103.128/29 ,40.78.65.112/32 ,40.78.65.193/32 ,40.83.133.96/32 ,40.83.134.97/32 ,40.112.136.59/32 ,40.112.138.23/32 ,40.112.147.242/32 ,52.159.214.96/28 ,52.160.32.120/32 ,52.160.39.166/32 ,52.160.68.27/32 ,52.160.70.105/32 ,52.160.70.221/32 ,52.160.93.229/32 ,52.160.94.54/32 ,104.40.32.148/32 ,104.40.34.112/32 ,104.40.34.169/32 ,104.42.40.164/32 ,104.42.44.28/32 ,104.42.128.212/32 ,104.42.129.159/32 ,104.42.134.185/32 ,104.42.142.214/32 ,104.42.226.197/32 ,168.62.9.74/32 ,168.62.9.100/32 ,168.62.28.191/32 ,2603:1030:a07:402::340/124 ,2603:1030:a07:402::360/123 ,4.149.67.227/32 ,4.149.68.5/32 ,4.149.68.65/32 ,4.149.68.80/32 ,4.149.68.107/32 ,4.149.68.115/32 ,4.155.160.115/32 ,4.155.162.75/32 ,4.155.162.242/32 ,4.155.163.32/32 ,4.155.163.91/32 ,4.155.164.225/32 ,20.72.243.225/32 ,20.72.244.58/32 ,20.72.244.108/32 ,20.99.189.70/32 ,20.99.189.158/32 ,20.99.190.19/32 ,20.125.1.80/28 ,20.125.1.96/28 ,40.78.245.144/28 ,40.78.245.160/27 ,172.179.35.160/28 ,172.179.145.85/32 ,172.179.145.245/32 ,172.179.155.210/32 ,172.179.155.215/32 ,2603:1030:c06:400::bc0/124 ,2603:1030:c06:400::be0/123 ,4.227.74.141/32 ,4.227.75.85/32 ,4.227.76.10/32 ,4.227.76.97/32 ,4.227.76.180/32 ,4.227.77.19/32 ,4.227.77.116/32 ,4.227.77.128/32 ,4.227.77.218/32 ,4.227.77.224/32 ,4.227.78.222/32 ,4.227.78.227/32 ,4.236.45.223/32 ,4.236.46.212/32 ,4.236.55.86/32 ,4.236.55.100/32 ,20.106.85.228/32 ,20.106.116.172/32 ,20.106.116.186/32 ,20.106.116.207/32 ,20.106.116.225/32 ,20.118.139.136/29 ,20.118.139.144/28 ,20.118.139.160/29 ,20.150.159.163/32 ,20.150.172.240/28 ,20.150.173.192/27 ,20.150.181.32/27 ,172.182.185.208/28 ,2603:1030:504:402::250/124 ,2603:1030:504:402::260/123 ,172.212.241.0/24 ,172.212.242.0/25 ,172.212.243.0/24 ,2603:1061:2002:200::/57 ,20.42.24.159/32 ,20.42.39.188/32 ,20.49.110.84/30 ,20.49.111.48/28 ,20.49.111.64/26 ,20.49.111.128/25 ,20.62.129.136/29 ,20.62.157.223/32 ,20.62.180.13/32 ,20.62.212.114/32 ,20.62.235.189/32 ,20.62.235.247/32 ,20.72.130.4/32 ,20.72.132.26/32 ,20.81.0.146/32 ,20.81.55.62/32 ,20.81.113.146/32 ,20.83.131.174/32 ,20.84.25.107/32 ,20.85.173.165/32 ,20.85.179.67/32 ,20.88.154.32/27 ,20.88.154.64/26 ,20.88.155.128/25 ,20.88.156.0/25 ,20.88.156.128/27 ,20.88.157.64/29 ,20.119.120.190/32 ,20.121.156.117/32 ,20.124.54.195/32 ,20.124.56.83/32 ,20.185.8.74/32 ,20.185.72.53/32 ,20.185.73.73/32 ,20.185.78.168/32 ,20.185.211.94/32 ,20.185.215.62/32 ,20.185.215.91/32 ,20.231.112.182/32 ,20.237.81.39/32 ,20.237.83.167/32 ,20.237.112.231/32 ,20.241.129.50/32 ,40.71.233.8/32 ,40.71.233.189/32 ,40.71.234.201/32 ,40.71.236.15/32 ,40.76.128.89/32 ,40.76.128.191/32 ,40.76.133.236/32 ,40.76.149.246/32 ,40.76.161.144/32 ,40.76.161.165/32 ,40.76.161.168/32 ,40.88.16.44/32 ,40.88.18.208/32 ,40.88.18.248/32 ,40.88.23.15/32 ,40.88.23.202/32 ,40.88.48.237/32 ,40.88.231.249/32 ,40.88.251.157/32 ,48.211.10.64/26 ,48.211.10.128/25 ,48.211.11.0/24 ,48.211.12.0/23 ,48.211.14.0/24 ,48.211.32.64/26 ,48.211.32.128/25 ,48.211.33.0/24 ,48.211.34.0/23 ,48.211.36.0/24 ,48.219.240.4/30 ,48.219.240.8/29 ,48.219.240.16/29 ,48.219.240.32/27 ,48.219.240.64/26 ,48.219.240.128/25 ,48.219.241.0/27 ,52.142.16.162/32 ,52.142.28.86/32 ,52.146.24.0/32 ,52.146.24.96/32 ,52.146.24.106/32 ,52.146.24.114/32 ,52.146.24.226/32 ,52.146.26.125/32 ,52.146.26.218/32 ,52.146.26.244/32 ,52.146.50.100/32 ,52.146.60.149/32 ,52.146.72.0/22 ,52.146.76.0/23 ,52.146.78.0/24 ,52.146.79.0/25 ,52.146.79.128/30 ,52.147.222.228/32 ,52.149.169.236/32 ,52.149.238.57/32 ,52.149.240.75/32 ,52.149.243.177/32 ,52.150.35.132/32 ,52.150.37.207/32 ,52.150.39.143/32 ,52.150.39.180/32 ,52.151.208.38/32 ,52.151.208.126/32 ,52.151.212.53/32 ,52.151.212.119/32 ,52.151.213.195/32 ,52.151.231.104/32 ,52.151.238.19/32 ,52.151.243.194/32 ,52.151.246.107/32 ,52.152.194.10/32 ,52.152.204.86/32 ,52.152.205.65/32 ,52.152.205.137/32 ,52.188.43.247/32 ,52.188.77.154/32 ,52.188.79.60/32 ,52.188.143.191/32 ,52.188.177.124/32 ,52.188.180.105/32 ,52.188.181.97/32 ,52.188.182.12/32 ,52.188.183.159/32 ,52.188.216.65/32 ,52.188.221.237/32 ,52.188.222.168/32 ,52.188.222.206/32 ,52.190.24.61/32 ,52.190.27.148/32 ,52.190.30.136/32 ,52.190.30.145/32 ,52.190.39.65/32 ,52.191.39.181/32 ,52.191.44.48/29 ,52.191.217.43/32 ,52.191.232.133/32 ,52.191.237.186/32 ,52.191.238.79/32 ,52.191.238.157/32 ,52.191.239.208/32 ,52.191.239.246/32 ,52.224.17.48/32 ,52.224.17.98/32 ,52.224.137.160/32 ,52.224.142.152/32 ,52.224.149.89/32 ,52.224.150.63/32 ,52.224.184.205/32 ,52.224.184.221/32 ,52.224.185.216/32 ,52.224.195.119/32 ,52.224.200.26/32 ,52.224.201.114/32 ,52.224.201.121/32 ,52.224.203.192/32 ,52.224.204.110/32 ,52.226.41.202/32 ,52.226.41.235/32 ,52.226.49.104/32 ,52.226.49.156/32 ,52.226.51.138/32 ,52.226.139.204/32 ,52.226.141.200/32 ,52.226.143.0/32 ,52.226.148.5/32 ,52.226.148.225/32 ,52.226.175.58/32 ,52.226.201.162/32 ,52.226.254.118/32 ,52.249.201.87/32 ,52.249.204.114/32 ,52.255.212.164/32 ,52.255.213.211/32 ,52.255.221.231/32 ,104.45.174.26/32 ,104.45.175.45/32 ,104.45.188.240/32 ,104.45.191.89/32 ,135.222.58.232/29 ,135.222.59.192/26 ,172.178.6.72/29 ,172.178.6.96/27 ,172.178.6.192/26 ,172.178.7.0/26 ,172.178.7.64/27 ,172.178.7.96/28 ,2603:1061:2002::/56 ,40.84.87.192/26 ,40.84.88.0/23 ,40.84.90.0/26 ,2603:1061:2002:300::/57 ,13.66.80.131/32 ,13.73.253.128/25 ,13.73.254.0/25 ,13.73.254.128/26 ,13.85.191.89/32 ,20.65.130.80/29 ,20.97.33.128/26 ,20.97.33.192/27 ,20.97.33.240/29 ,20.188.77.155/32 ,40.74.183.82/32 ,40.74.183.121/32 ,40.74.200.156/32 ,40.74.201.230/32 ,40.74.202.22/32 ,40.119.1.22/32 ,40.119.42.85/32 ,40.119.42.86/32 ,40.124.136.2/32 ,40.124.136.75/32 ,40.124.136.138/32 ,52.185.226.247/32 ,52.185.230.20/32 ,52.249.59.157/32 ,52.249.60.80/32 ,52.249.63.45/32 ,2603:1061:2002:1200::/57 ,4.150.232.8/29 ,4.150.233.64/26 ,4.150.233.128/25 ,4.150.234.0/28 ,4.150.234.16/29 ,13.64.27.44/32 ,13.64.35.24/32 ,13.64.38.167/32 ,13.64.39.170/32 ,13.64.128.119/32 ,13.64.174.215/32 ,13.64.177.224/32 ,13.83.17.188/32 ,13.83.23.194/32 ,13.83.56.37/32 ,13.83.64.166/32 ,13.83.66.89/32 ,13.83.66.124/32 ,13.83.68.60/32 ,13.83.70.105/32 ,13.83.97.180/32 ,13.83.97.188/32 ,13.83.102.38/32 ,13.83.145.222/32 ,13.83.147.192/32 ,13.83.151.212/32 ,13.83.248.248/32 ,13.83.249.34/32 ,13.83.249.58/32 ,13.86.136.222/32 ,13.86.137.20/32 ,13.86.139.229/32 ,13.86.155.216/32 ,13.86.177.32/32 ,13.86.185.5/32 ,13.86.185.6/32 ,13.86.185.35/32 ,13.86.185.81/32 ,13.86.185.91/32 ,13.86.192.20/32 ,13.86.193.65/32 ,13.86.194.190/32 ,13.86.249.98/32 ,13.86.250.62/32 ,13.86.250.244/32 ,13.86.252.116/32 ,13.86.254.118/32 ,13.86.254.191/32 ,13.87.135.122/32 ,13.87.153.50/32 ,13.87.154.24/32 ,13.87.154.100/32 ,13.87.154.164/32 ,13.87.157.188/32 ,13.87.160.104/32 ,13.87.160.143/32 ,13.87.160.212/32 ,13.87.161.18/32 ,13.87.161.235/32 ,13.87.161.241/32 ,13.87.162.91/32 ,13.87.163.230/32 ,13.87.164.20/32 ,13.87.164.30/32 ,13.87.164.186/32 ,13.87.164.205/32 ,13.87.167.46/32 ,13.87.167.63/32 ,13.87.167.172/32 ,13.87.167.174/32 ,13.87.167.198/32 ,13.87.207.81/32 ,13.87.216.21/32 ,13.87.216.130/32 ,13.87.217.11/32 ,13.87.217.75/32 ,13.87.217.80/32 ,13.87.218.70/32 ,13.87.218.169/32 ,13.88.56.107/32 ,13.88.65.140/32 ,13.88.65.204/32 ,13.88.128.218/32 ,13.88.129.116/32 ,13.88.129.160/32 ,13.88.132.123/32 ,13.88.133.160/32 ,13.88.135.42/32 ,13.88.135.67/32 ,13.88.135.72/32 ,13.91.22.243/32 ,13.91.126.78/32 ,13.91.136.144/32 ,13.91.138.172/32 ,20.49.121.192/26 ,20.49.122.0/23 ,20.49.124.0/24 ,20.49.125.0/25 ,20.49.125.128/27 ,20.49.125.160/28 ,20.49.125.176/29 ,20.49.125.184/30 ,20.49.125.192/26 ,20.49.126.0/25 ,20.49.127.248/29 ,20.59.77.128/25 ,20.59.78.0/24 ,20.59.79.80/29 ,20.184.251.143/32 ,20.189.142.58/32 ,20.237.137.112/32 ,20.237.160.38/32 ,20.237.183.140/32 ,20.237.199.13/32 ,20.237.243.36/32 ,20.237.252.8/32 ,20.245.1.212/32 ,20.245.8.110/32 ,20.245.34.183/32 ,20.245.107.170/32 ,20.245.139.209/32 ,20.253.152.61/32 ,20.253.209.242/32 ,20.253.224.215/32 ,20.253.228.153/32 ,23.99.89.156/32 ,23.101.203.146/32 ,23.101.203.241/32 ,40.65.49.103/32 ,40.65.49.140/32 ,40.65.49.151/32 ,40.83.173.74/32 ,40.83.184.82/32 ,40.86.161.9/32 ,40.86.164.89/32 ,40.112.252.78/32 ,40.118.185.80/32 ,40.118.200.18/32 ,40.118.213.65/32 ,40.118.237.211/32 ,52.159.218.64/26 ,52.159.218.128/25 ,52.159.219.0/24 ,52.159.220.0/23 ,52.160.108.225/32 ,52.180.96.196/32 ,52.180.102.55/32 ,52.234.104.49/32 ,52.241.138.151/32 ,52.241.140.217/32 ,52.246.120.190/32 ,52.250.228.36/30 ,52.250.228.40/29 ,52.250.228.48/28 ,52.250.228.128/25 ,52.250.229.0/24 ,52.250.230.0/23 ,57.154.136.4/30 ,57.154.136.8/29 ,57.154.136.16/28 ,57.154.136.32/27 ,57.154.136.64/26 ,57.154.136.128/25 ,72.153.247.52/30 ,72.153.247.56/29 ,72.153.247.64/26 ,72.153.247.128/25 ,72.153.248.0/22 ,72.153.252.0/24 ,72.153.253.0/25 ,72.153.253.128/26 ,72.153.253.192/27 ,72.153.253.224/30 ,104.40.18.140/32 ,104.40.27.168/32 ,104.40.34.134/32 ,104.40.72.149/32 ,104.42.32.228/32 ,104.42.35.16/32 ,104.42.42.161/32 ,104.42.45.52/32 ,104.42.45.226/32 ,104.42.134.6/32 ,104.42.134.169/32 ,104.42.214.62/32 ,104.45.230.45/32 ,104.45.230.48/32 ,104.45.231.252/32 ,104.210.57.39/32 ,137.135.53.196/32 ,168.61.3.73/32 ,172.185.119.112/29 ,172.185.119.192/26 ,2603:1061:2002:100::/56 ,40.64.134.144/28 ,40.64.134.192/26 ,40.91.87.146/32 ,2603:1061:2002:500::/57 ,13.67.236.125/32 ,104.208.25.27/32 ,40.122.170.198/32 ,40.113.218.230/32 ,23.100.86.139/32 ,23.100.87.24/32 ,23.100.87.56/32 ,23.100.82.16/32 ,20.109.202.36/32 ,13.92.98.111/32 ,40.121.91.41/32 ,40.114.82.191/32 ,23.101.139.153/32 ,23.100.29.190/32 ,23.101.136.201/32 ,104.45.153.81/32 ,23.101.132.208/32 ,4.156.25.188/32 ,4.156.243.164/32 ,4.156.243.172/32 ,4.156.242.12/32 ,40.84.30.147/32 ,104.208.155.200/32 ,104.208.158.174/32 ,104.208.140.40/32 ,40.70.131.151/32 ,40.70.29.214/32 ,40.70.26.154/32 ,40.70.27.236/32 ,168.62.248.37/32 ,157.55.210.61/32 ,157.55.212.238/32 ,52.162.208.216/32 ,52.162.213.231/32 ,65.52.10.183/32 ,65.52.9.96/32 ,65.52.8.225/32 ,104.210.144.48/32 ,13.65.82.17/32 ,13.66.52.232/32 ,23.100.124.84/32 ,70.37.54.122/32 ,70.37.50.6/32 ,23.100.127.172/32 ,23.101.183.225/32 ,52.161.27.190/32 ,52.161.18.218/32 ,52.161.9.108/32 ,13.78.151.161/32 ,13.78.137.179/32 ,13.78.148.140/32 ,13.78.129.20/32 ,13.78.141.75/32 ,13.71.199.128/27 ,13.78.212.163/32 ,13.77.220.134/32 ,13.78.200.233/32 ,13.77.219.128/32 ,52.150.226.148/32 ,4.255.161.16/32 ,4.255.195.186/32 ,4.255.168.251/32 ,4.255.219.152/32 ,20.165.235.148/32 ,20.165.249.200/32 ,20.165.232.68/32 ,52.160.92.112/32 ,40.118.244.241/32 ,40.118.241.243/32 ,157.56.162.53/32 ,157.56.167.147/32 ,104.42.49.145/32 ,40.83.164.80/32 ,104.42.38.32/32 ,13.86.223.0/32 ,13.86.223.1/32 ,13.86.223.2/32 ,13.86.223.3/32 ,13.86.223.4/32 ,13.86.223.5/32 ,13.66.210.167/32 ,52.183.30.169/32 ,52.183.29.132/32 ,13.66.201.169/32 ,13.77.149.159/32 ,52.175.198.132/32 ,13.66.246.219/32 ,20.150.181.32/32 ,20.150.181.33/32 ,20.150.181.34/32 ,20.150.181.35/32 ,20.150.181.36/32 ,20.150.181.37/32 ,20.150.181.38/32 ,20.150.173.192/32
        ```

         You must also add your own IP address if you would like to [test the API services](./docs/TestApis.md) using Postman after deployment. You can also [update the IP allowlist after deployment](./docs/UpdateIPAllowlist.md) if needed.


>**IMPORTANT SECURITY NOTE:** The API service endpoints can only be accessed from client apps with IPs that are white listed which you defined when running this script. After deployment, you will need to implement additional API security to prevent unauthorized use. It is advised to monitor access and scan system logs to detect unusual patterns.

 ## Configuring a New or Existing Resource Group 
   
  When configuring your deployment, you have the option to use either a new or an existing Azure resource group. Please follow the instructions below based on your selection:

  - **Creating a New Resource Group**  :  You can create a new resource group in one of the following two ways:

      - Manually specify the name of the new resource group.
                Example: rg-esgdocanalysis

      - Leave the input field blank and press Enter. A new resource group name will be automatically generated.

  - **Using an Existing Resource Group** : If you prefer to use an existing resource group, please ensure that:

      - You enter the exact name of the existing resource group.

      - You provide the same environment name that was used previously with this resource group.

  This ensures consistency and avoids configuration conflicts during deployment.

  ⚠️ After deployment, please restart the AKS (Kubernetes) service to ensure updated configurations are applied when using a reused resource group.

![Enter Resource Group](./images/services/enter-rg.png)
![Enter Environment](./images/services/enter-rg-env.png)

### Step 1.3 
Copy and save the API Service Endpoint from the deployment output for further steps.:<br></br>
    ![Deployment Success](./images/services/logicappwork02.png)
 
## Step 2 Post-Deploy Configuration

The following integrations require manual configuration and cannot be fully automated.

1. [Configure Microsoft Teams](#configure-microsoft-teams)
2. [Configure Teams API Connection](#configure-teams-api-connection)
3. [Configure Logic App Actions](#configure-logic-app-actions)
4. [Configure Logic App HTTP Call](#configure-logic-app-http-call)
5. [Configure Azure OpenAI Rate Limits](#configure-azure-openai-rate-limits)


### Step 2.1 Configure Microsoft Teams

If required, create a dedicated Teams channel for the IT team to receive notifications. Since all document registration, processing, analysis, and generation are handled asynchronously, the solution sends status updates to the Teams channel to support ongoing monitoring.

1. Click `Teams > Add Channel` from left sidebar:<br>
    ![Create Channel01](./images/services/teams_setting01.png)

1. Create new channel to recieve status updates:<br>
    ![Create Channel02](./images/services/teams_setting02.png)


### Step 2.2 Configure Teams API Connection
To access Teams from Logic Apps, we need to authorization a connection.

1. Navigate to the deployed resource group in the Azure Portal, then select the ` teams ` resource from the list.<br>
    ![Check Teams API connection](./images/services/teamsconnection_setting01.png)

1. Click `Authorize` on the `General > Edit API connection` page:<br>
    ![Check Teams API connection and Authorize](./images/services/teamsconnection_setting02.png)

1. You will get a login flow to authorize connection:<br>
    ![Check Teams API connection - authorization process](./images/services/teamsconnection_setting03.png)

1. Once successfully logged in, click `Save` buton.


### Step 2.3 Configure Logic App Actions
**Note: This step must be taken in all 3 deployed Logic Apps>**
* logicapp-benchmarkprocesswatcher{*}
* logicapp-docregistprocesswatcher{*}
* logicapp-gapanalysisprocesswatcher{*}

1. Within each Logic App, open the `Logic app designer`:<br>
    ![open logic app designer](./images/services/logicapp_setting01.png)

2. Select the Teams actions to update Channel information:<br><br>
    Set Team:<br>
    ![Set Teams](./images/services/logicapp_setting02.png)<br>

    Set Channel:<br>
    ![Set Channel](./images/services/logicapp_setting03.png)


### Step 2.4 Configure Logic App HTTP Call

1. Copy the `API Service Endpoint` from the deployment script summary:<br>
    ![Deployment Success](./images/services/logicappwork02.png)
1. Open the Logic App named `logicapp-docregistprocesswatcher{*}`
1. Select and click `HTTP` action, which has the text `HTTP` next to a green logo. 
1. Paste URL replacing only `[...]` prefix and preserving API path:<br>
    ![Logic App](./images/services/logicappwork01.png)<br>
    ![Logic App](./images/services/logicappwork03.png)


### Step 2.5 Configure Azure OpenAI Rate Limits

> **Capacity Note:**
> * The deployment script creates models using the minimum tokens per minute (TPM) rate limit.
> * Faster performance can be achieved by increasing the TPM limit with Azure OpenAI Foundry.
> * If capacity is too low, you may experience timeout errors.
> * Capacity varies for [regional quota limits](https://learn.microsoft.com/en-us/azure/ai-services/openai/quotas-limits#regional-quota-limits) as well as for [provisioned throughput](https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/provisioned-throughput).

1. Browse to Azure OpenAI Foundry and select **each of the 3 models** within the `Deployments` menu:
    ![Select Model](./images/services/Control_Model_TPM000.png)  

1. Increase the TPM value for **each of the 3 models** for faster report generation:  
    ![Set Token per limit](./images/services/Control_Model_TPM001.png)

## Troubleshooting

If you experienced any issues during deployment, please review the [troubleshooting guide](./TROUBLESHOOTING.md).

## Next Steps

### 🥳🎉 First, congrats on finishing backend deployment!

* [Test backend APIs](./docs/TestApis.md)
* [Deploy Power Platform Client and set up Client integration with Services](./DeployPowerPlatformClient.md)

