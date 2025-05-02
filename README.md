# Comparative Analysis for Sustainability

This solution accelerator enables you to advance your sustainability initiatives with precision by using AI to compare your reports to peers, benchmark them against industry standards, and quickly create a plan of action. you can compare your business to peers using Corporate Sustainability Reporting Directive ([CSRD](https://finance.ec.europa.eu/capital-markets-union-and-financial-markets/company-reporting-and-auditing/company-reporting/corporate-sustainability-reporting_en)) and Global Reporting Initiatives ([GRI](https://www.globalreporting.org/standards)). 

<br/>

<div align="center">
[**SOLUTION OVERVIEW**](#solution-overview)  \| [**QUICK DEPLOY**](#quick-deploy)  \| [**BUSINESS USE CASE**](#business-use-case)  \| [**SUPPORTING DOCUMENTATION**](#supporting-documentation)
</div>
<br/>

<h2><img src="./Deployment/images/readme/solution-overview.png" width="48" />
Solution overview
</h2>
This solution accelerator enables companies to detect compliance gaps, benchmark against their peers, and generate action plans to ensure they’re on track to meet their sustainability goals. Leveraging the simplicity of PowerApps, users can easily upload documents and generate gap analyses and benchmarking reports for GRI and CSRD indicators. An AI-generated report is then created, which provides a brief summary of the company’s (and their peer’s) performance, similarities/dissimilarities, and an action plan for improvement. The user can also use natural language with a Teams Copilot to ask questions about the reports to further aid understanding and actionability.

Please review the [**Release Notes Page**]( ./Deployment/docs/ReleaseNotes.md) to see what is new.  

### Solution architecture

Below is a sample landing page of the solution accelerator after it is deployed, set up, and ready to be used. You will need to replace Microsoft Logo with your own company's logo. 

![image](./Deployment/images/readme/solution-architecture.png) 

**Note 1**: Please note that **the provided code serves as a demonstration only**. The solution is **not an officially supported Microsoft offering**.

**Note 2**: Before deploying the solution accelerator, **please read** [Security.md](./Deployment/docs/Security.md) for security information. 

**Note 3**: Some features contained in this repository are in private preview. Certain features might not be supported or might have constrained capabilities. For more information, see [Supplemental Terms of Use for Microsoft Azure Previews](https://azure.microsoft.com/en-us/support/legal/preview-supplemental-terms).

### How to customize

If you'd like to customize the solution accelerator, here are some common areas to start:

{🟨TODO: Fill in links to supplementary documentations}

[Doc name](./docs/...)

[Doc name](./docs/...)

[Doc name](./docs/...)

<br/>

### Additional resources

13. 

<br/>

{🟨TODO: Fill in with key features}
### Key features
<details open>
  <summary>Click to learn more about the key features this solution enables</summary>

  - **Benchmark Against Peers** <br/>
    See how your work measures up to the competition by comparing lengthy and complex sustainability reports.
  - **Spot Compliance Gaps** <br/>
    Detect gaps in your compliance reporting by gauging your work directly against Sustainability reporting standards like CSRD and GRI.
  - **Generate Action Plans** <br/>
    Get actionable insights and recommendations from AI analyses to identify opportunities and learn how to improve your sustainability work.
  - **Multiply Productivity** <br/>
    Access and chat with reports within your existing workflows and use a simple interface to generate reports.
  - **Talk like a human** <br/>
    Use natural language to ask questions of your benchmark and gap analysis reports to improve understanding.

</details>



<br /><br />

<h2><img src="./Deployment/images/readme/quick-deploy.png" width="48" />
Quick deploy
</h2>


### How to install or deploy
Follow the steps described in the deployment guide to deploy this solution:

[Click here to launch the deployment guide](./Deployment/README.md)
<br/>

{🟨TODO: Remove if Azure OpenAI quota check is not required }

> ⚠️ **Important: Check Azure OpenAI Quota Availability**
 <br/>To ensure sufficient quota is available in your subscription, please follow [quota check instructions guide](./docs/QuotaCheck.md) before you deploy the solution.

<br/>

### Prerequisites and Costs
To deploy this solution accelerator, ensure you have access to an [Azure subscription](https://azure.microsoft.com/free/) with the necessary permissions to create **resource groups, resources, app registrations, and assign roles at the resource group level**. This should include Contributor role at the subscription level and  Role Based Access Control role on the subscription and/or resource group level. 

Check the [Azure Products by Region](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/?products=all&regions=all) page and select a **region** where the following services are available. Here is an example region where the services are available: `canadacentral`.

You will also need to have Power Platform License to deploy the sample Power Platform Client Solution.  A prior understanding of Microsoft Copilot Studio, Microsoft Power Power Platform, Azure Open AI, Azure AI Search, Azure AI Document Intelligence will be helpful. 

Products and serviecs utilized in this solution accelerator is listed below:

1. [Microsoft Power Platform](https://learn.microsoft.com/en-us/power-platform/)
2. [Microsoft Copilot Studio](https://learn.microsoft.com/en-us/microsoft-copilot-studio/)
3. [Microsoft SharePoint](https://learn.microsoft.com/en-us/sharepoint/)
4. [Azure Blob Storage](https://learn.microsoft.com/en-us/azure/storage/blobs/)
5. [Azure Queue storage](https://learn.microsoft.com/en-us/azure/storage/queues/)
6. [Azure Open AI](https://learn.microsoft.com/en-us/azure/ai-services/openai/) 
7. [Azure AI Search](https://learn.microsoft.com/en-us/azure/search/)
8. [Azure AI Document Intelligence](https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/?view=doc-intel-4.0.0)
9. [Azure Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/)
10. [Azure Logic Apps](https://learn.microsoft.com/en-us/azure/logic-apps/)
11. [Azure Container Registry](https://learn.microsoft.com/en-us/azure/container-registry/)
12. [Azure Kubernetes Service (AKS)](https://learn.microsoft.com/en-us/azure/aks/)
13. [Microsoft Fabric](https://learn.microsoft.com/en-us/fabric/) (only if opt-in for the Fabric Integration )







{🟨TODO: Call out specific pricing "gotchas" like Azure Container Registry if known}

Pricing varies per region and usage, so it isn't possible to predict exact costs for your usage. The majority of the Azure resources used in this infrastructure are on usage-based pricing tiers. However, Azure Container Registry has a fixed cost per registry per day.

{🟨TODO: Update with solution specific estimate sheet}

Use the [Azure pricing calculator](https://azure.microsoft.com/en-us/pricing/calculator) to calculate the cost of this solution in your subscription. 

Review a [sample pricing sheet](https://azure.com/e/68b51f4cb79a4466b631a11aa57e9c16) in the event you want to customize and scale usage.

_Note: This is not meant to outline all costs as selected SKUs, scaled use, customizations, and integrations into your own tenant can affect the total consumption of this sample solution. The sample pricing sheet is meant to give you a starting point to customize the estimate for your specific needs._

<br/>

{🟨TODO: Update with all products, decription of product use, and product specific pricing links}

| Product | Description | Cost |
|---|---|---|
| [Product Name with Link to Learn content](https://learn.microsoft.com) | Decription of how the product is used | [Pricing]() |
| [Product Name with Link to Learn content](https://learn.microsoft.com) | Decription of how the product is used | [Pricing]() |
| [Product Name with Link to Learn content](https://learn.microsoft.com) | Decription of how the product is used | [Pricing]() |
| [Product Name with Link to Learn content](https://learn.microsoft.com) | Decription of how the product is used | [Pricing]() |


<br/>

>⚠️ **Important:** To avoid unnecessary costs, remember to take down your app if it's no longer in use,
either by deleting the resource group in the Portal or running `azd down`.

<br /><br />

<h2><img src="./Deployment/images/readme/business-scenario.png" width="48" />
Business Use Case
</h2>

As a sustainability manager or sustainability analyst, you will be able to use the solution to perform below business functions:  

- Analyze your company's sustainability document and obtain insights and recommendations to help your corporation to identify gaps and take actions to achieve set sustainability goals. 
- Compare your sustainability document with your peers' sustainability documents and obtain benchmark and gap analysis reports. In most of the cases, you will be able to download your peers' sustainability documents from their public facing websites. 

|![image](./Deployment/images/readme/landingPage.png)|
|---|

<br/>

{🟨TODO: Fill in with overview of the use case as represented in the solution}

Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam viverra et dolor rutrum vestibulum. Vestibulum non porta dolor, eu vulputate lacus. 

In tempus nibh vel lectus vestibulum, eget suscipit turpis auctor. Nam egestas ante vel mi tempor, ac suscipit elit tempor. Aliquam eget hendrerit lacus. Nullam euismod eget tortor congue interdum. Vestibulum laoreet, tellus laoreet consequat facilisis, quam purus tincidunt tellus, non maximus dolor lacus a risus. Aliquam erat volutpat. 

Nulla sit amet mollis magna. Sed pellentesque vestibulum ante non vestibulum. In congue interdum dolor, et blandit nisi consectetur quis. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.

Nulla pretium malesuada purus, vel euismod urna ultrices eu. Nullam enim neque, suscipit quis hendrerit iaculis, iaculis a metus. 

⚠️ The sample data used in this repository is synthetic and generated using Azure OpenAI service. The data is intended for use as sample data only.


{🟨TODO: Fill in with business value}
### Business value
<details>
  <summary>Click to learn more about what value this solution provides</summary>

  - **Business value name** <br/>
    Business value description goes here.

  - **Business value name** <br/>
    Business value description goes here.

  - **Business value name** <br/>
    Business value description goes here.

  - **Business value name** <br/>
    Business value description goes here.


</details>

<br /><br />

<h2><img src="./Deployment/images/readme/supporting-documentation.png" width="48" />
Supporting documentation
</h2>


### Security guidelines

{🟨TODO: Fill in with solution specific security guidelines similar to the below}

This template uses Azure Key Vault to store all connections to communicate between resources.

This template also uses [Managed Identity](https://learn.microsoft.com/entra/identity/managed-identities-azure-resources/overview) for local development and deployment.

To ensure continued best practices in your own repository, we recommend that anyone creating solutions based on our templates ensure that the [Github secret scanning](https://docs.github.com/code-security/secret-scanning/about-secret-scanning) setting is enabled.

You may want to consider additional security measures, such as:

* Enabling Microsoft Defender for Cloud to [secure your Azure resources](https://learn.microsoft.com/azure/security-center/defender-for-cloud).
* Protecting the Azure Container Apps instance with a [firewall](https://learn.microsoft.com/azure/container-apps/waf-app-gateway) and/or [Virtual Network](https://learn.microsoft.com/azure/container-apps/networking?tabs=workload-profiles-env%2Cazure-cli).

<br/>

### Frequently asked questions

{🟨TODO: Remove this section if you don't have FAQs}

[Click here](./docs/FAQs.md) to learn more about common questions about this solution.

<br/>

### Cross references
Check out similar solution accelerators

{🟨TODO: Identify related accelerators - fill in the name and a one sentence description. The name should have non-breaking spaces in them to make sure the layout doesn't break.}

| Solution Accelerator | Description |
|---|---|
| [Document&nbsp;knowledge&nbsp;mining](https://github.com/microsoft/Document-Knowledge-Mining-Solution-Accelerator) | Provides REST API access to OpenAI's powerful language models including o3-mini, o1, o1-mini, GPT-4o, GPT-4o mini |
| [Conversation&nbsp;knowledge&nbsp;mining](https://github.com/microsoft/Conversation-Knowledge-Mining-Solution-Accelerator) | Description of solution accelerator |
| [Document&nbsp;generation](https://github.com/microsoft/document-generation-solution-accelerator) | Analyzes various media content—such as audio, video, text, and images—transforming it into structured, searchable data |


<br/>   


## Provide feedback

{🟨TODO: Update link to create new issues for this repo}

Have questions, find a bug, or want to request a feature? [Submit a new issue](https://github.com/microsoft/content-processing-solution-acclerator/issues) on this repo and we'll connect.

<br/>

## Responsible AI Transparency FAQ 
Please refer to [Transparency FAQ](./TRANSPARENCY_FAQ.md) for responsible AI transparency details of this solution accelerator.

<br/>

## Disclaimers

To the extent that the Software includes components or code used in or derived from Microsoft products or services, including without limitation Microsoft Azure Services (collectively, “Microsoft Products and Services”), you must also comply with the Product Terms applicable to such Microsoft Products and Services. You acknowledge and agree that the license governing the Software does not grant you a license or other right to use Microsoft Products and Services. Nothing in the license or this ReadMe file will serve to supersede, amend, terminate or modify any terms in the Product Terms for any Microsoft Products and Services. 

You must also comply with all domestic and international export laws and regulations that apply to the Software, which include restrictions on destinations, end users, and end use. For further information on export restrictions, visit https://aka.ms/exporting. 

You acknowledge that the Software and Microsoft Products and Services (1) are not designed, intended or made available as a medical device(s), and (2) are not designed or intended to be a substitute for professional medical advice, diagnosis, treatment, or judgment and should not be used to replace or as a substitute for professional medical advice, diagnosis, treatment, or judgment. Customer is solely responsible for displaying and/or obtaining appropriate consents, warnings, disclaimers, and acknowledgements to end users of Customer’s implementation of the Online Services. 

You acknowledge the Software is not subject to SOC 1 and SOC 2 compliance audits. No Microsoft technology, nor any of its component technologies, including the Software, is intended or made available as a substitute for the professional advice, opinion, or judgement of a certified financial services professional. Do not use the Software to replace, substitute, or provide professional financial advice or judgment.  

BY ACCESSING OR USING THE SOFTWARE, YOU ACKNOWLEDGE THAT THE SOFTWARE IS NOT DESIGNED OR INTENDED TO SUPPORT ANY USE IN WHICH A SERVICE INTERRUPTION, DEFECT, ERROR, OR OTHER FAILURE OF THE SOFTWARE COULD RESULT IN THE DEATH OR SERIOUS BODILY INJURY OF ANY PERSON OR IN PHYSICAL OR ENVIRONMENTAL DAMAGE (COLLECTIVELY, “HIGH-RISK USE”), AND THAT YOU WILL ENSURE THAT, IN THE EVENT OF ANY INTERRUPTION, DEFECT, ERROR, OR OTHER FAILURE OF THE SOFTWARE, THE SAFETY OF PEOPLE, PROPERTY, AND THE ENVIRONMENT ARE NOT REDUCED BELOW A LEVEL THAT IS REASONABLY, APPROPRIATE, AND LEGAL, WHETHER IN GENERAL OR IN A SPECIFIC INDUSTRY. BY ACCESSING THE SOFTWARE, YOU FURTHER ACKNOWLEDGE THAT YOUR HIGH-RISK USE OF THE SOFTWARE IS AT YOUR OWN RISK.  
