# Security
Please note that the architecture of this solution accelerator is built for demonstration only. **The provided code serves as a demonstration and is not an officially supported Microsoft offering**. 

If you follow the instructions in [DeployAzureResources.md](../DeployAzureResources.md) to deploy the services which are hosted in Azure Kubernetes Services (AKS), the services will be established with client IP range white listed based on your input to the deployment scripts.  It is advised you to monitor access and scan system logs to detect unusual patterns. It is also advised that you build  automation process that shut down the AKS when you are not using the services to save cost. You can manually shut down the AKS cluster for planned periods before completing the automation process.  For more information on AKS, please refer to [Azure Kubernetes Service (AKS)](https://learn.microsoft.com/en-us/azure/aks/).

It is advised that you add a stronger security layer, for example, with methods outlined in this article [Use Azure API Management with Microservices Deployed in Azure Kubernetes Service](https://learn.microsoft.com/en-us/azure/api-management/api-management-kubernetes#kubernetes-services-and-apis).  

You can work with your company's network and security teams to understand what existing security measures are already implemented. For example, your company may have established base-line architecture for Azure Kubernetes Services (AKS) as described in [Baseline architecture for an AKS cluster - Azure Architecture Center | Microsoft Learn](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks/baseline-aks?toc=%2Fazure%2Faks%2Ftoc.json&bc=%2Fazure%2Faks%2Fbreadcrumb%2Ftoc.json).  


