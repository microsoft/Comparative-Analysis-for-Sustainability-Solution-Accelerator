# Deployment Guide 

**Step 1** - Follow instructions in [DeployAzureResources.md](./DeployAzureResources.md) to deploy and set up services.

**Step 2** - Follow instructions in [DeployPowerPlatformClient.md](./DeployPowerPlatformClient.md) to deploy and set up power platform client that utilizes the deployed services. This instructions also include steps to set up up the client and service integration. 

**Step 3** - Follow instructions in [TestSolutionAccelerator.md](./TestSolutionAccelerator.md) to test the solution accelerator.

**Step 4** - <u>Optional</u> if your purpose is only to deploy this solution accelerator and want to see how it works. 

Otherwise, you will need to at least modify two prompts files, replacing the text '**Microsoft**' with your own company's name. You can review and modify the grounding prompt files to suit your company's specific needs. The two prompts files are located in the [prompts location](../Services/src/esg-ai-doc-analysis/CFS.SK.Sustainability.AI/plugins/CSRDPlugin/): 

* [Benchmarking Report Generator Prompt file skprompt.txt](../Services/src/esg-ai-doc-analysis/CFS.SK.Sustainability.AI/plugins/CSRDPlugin/BenchmarkReportGenerator/skprompt.txt)

* [Gap Analysis Report Generator Prompt file skprompt.txt](../Services/src/esg-ai-doc-analysis/CFS.SK.Sustainability.AI/plugins/CSRDPlugin/GAPAnalyzeReportGenerator/skprompt.txt)

**Step 5** - Follow instruction in the [Power App Client Guide](../Client/README.md) to test the the power platform client.
