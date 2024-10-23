# Architecture Description
The solution accelerator architecture is depicture below with a brief description. 

![Architecture](../../Deployment/images/readme/architecture.png)



Before deploying the solution accelerator, **please read** [Security.md](./Security.md) for security information. 

The user interaction and flow of information are as follows: 

**1 Upload / Register Document for AI analysis**

Note: The user can be one of these roles: Sustainability Manager, Head of Extra-Financial Reporting, or other designated person in the organization.  

The user logs into the Power App and uploads a sustainability reports he & his (she & her) colleagues would use to create Benchmarking & Gap Analysis reports. Power Automate sends these documents to backend services so they are ready to be processed. 

The services in Azure hosted in `Document Pods` perform below tasks:

- [ ]  Extract the file content using Document Intelligent services
- [ ] Break down the document into chunks
- [ ] Vectorize the chucks using GPT embedding services
- [ ] Save documents, chunks into Azure Blob Storage 
- [ ] Save the entire processed files into AI search, through multiple data process pipelines within Azure Storage Blob Process queues 

**2 Initiate Benchmarking, Gap Analysis, and Output Document Generation** 

The User navigates to the Benchmarking or Gap Analysis section of the app and creates a record.
Upon record creation, an integration power automate flow kicks in and initiates AI infused document creation process in the backend using the documents the user selected. 

Upon successful submission of the Benchmarking or Gap Analysis record from the app, power automate sends all the required data to the backend services where document contents, prompts & AI services work together to produce the output document for Benchmarking or Gap Analysis. 

The services in Azure hosted in `Document Pods`  and `AI Pods` perform below tasks assisted with Azure Open AI Services:

- [ ] `Document Pods` extract knowledge and provide summarization using GPT 4o
- [ ] `AI Pods` extract knowledge and provide summarization for each company using GPT 4 32K 

**3 Output Documents Uploaded to SharePoint** 

Once the output document is successfully created in the backend, power automate uploads the document in repository where it is ready to be viewed.

**4 User Accesses the Output Documents**

Note: The user can be one of these roles: Sustainability Manager, Head of Extra-Financial Reporting, or Chief Sustainability Officer, or other designated person in the organization, as long as the user is added to the Teams channel. 

After the document is available in SharePoint, the user gets a notification in Teams from a custom copilot. At this point, the user can view the document by clicking a hyperlink from Teams or he can choose to chat with the copilot to get insights about the generated document.

> **Note:** If you do not have access to an environment where you can deploy and set up the Power Apps Client and Copilot Studio, you can optionally deploy and test only the **Services in Azure** portion of this architecture. You can write your own clients to utilize the REST APIs. For additional information, please review  [ArchitectureDescriptionServices.md](./ArchitectureDescriptionServices.md). 

