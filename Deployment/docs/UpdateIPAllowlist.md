# Update IP Allowlist

When you [deploy Azure resources](../DeployAzureResources.md), you are [prompted by the deployment script](../DeployAzureResources.md#run-deployment) to supply a range of IP addresses to allow to access the [API services](./TestApis.md). The client application will not work if the appropriate [Power Automate (Azure Logic Apps) outbound IP addresses](https://learn.microsoft.com/en-us/azure/logic-apps/logic-apps-limits-and-config?tabs=consumption#outbound-ip-addresses) are not allowlisted. You can refer to the official [Azure Service Tags JSON](https://www.microsoft.com/en-us/download/details.aspx?id=56519) as well. Additionally, if you need to test the API from Postman, you will need to add your own IP address to the list. If you need to add IP addresses to the allowlist post-deployment, you can follow these instructions:

1. Go to the resource group for your deployment (rg-esgdocanalysisxxxxx)
1. Select the Azure Kubernetes Service resource (aks-esgdocanalysisxxxxx)
1. Press the **Start** button if your Kubernetes service is stopped and wait until it is running
1. Select **Kubernetes resources > Services and ingresses > Ingresses > ingress-aiservice > YAML** to edit your ingress resource's metadata
1. Add your desired IP addresses to the `nginx.ingress.kubernetes.io/whitelist-source-range` annotation and the `kubectl.kubernetes.io/last-applied-configuration` annotation
1. Click the **Review + save** button
1. Check the **confirm manifest changes** box and click **Save**

**IMPORTANT SECURITY NOTE:** The API service endpoints can only be accessed from client apps with IPs that are white listed. After deployment, you will need to implement additional API security to prevent unauthorized use. It is advised to monitor access and scan system logs to detect unusual patterns.