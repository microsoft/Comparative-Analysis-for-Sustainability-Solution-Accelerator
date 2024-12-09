# Set up Fabric 

## Disclaimer

Fabric usage in this solution is **optional**. If you would like to use this accelerator without Fabric, please skip this section and make sure the 2 Power Automate workflows (Output doc upload from SharePoint to Fabric and File Upload from SharePoint List to Fabric) are switched off. 

## Pre Requisites	

1. Fabric License
2. Privileges to create a Fabric Workspace and a Lakehouse.



## Step 1: Create Lakehouse

1. Go to https://app.fabric.microsoft.com/

2. Click on Data Engineering

3. In the left navigation menu, click on Workspaces.

4. Click 'New Workspace'

5. Give a name to the workspace and click Apply. All other fields are optional.

6. In your workspace, click 'New Item' from the top left.

7. Select Lakehouse and give a name to the Lakehouse. Click Create.

8. In the Lakehouse, right click on Files> Properties> Copy the URL. Save this URL which will be used in the next step.

   

   ![](C:\repos2\Comparative-Analysis-for-Sustainability-Solution-Accelerator\Deployment\images\client\fabriclakehouse.png)



## Step 2: Power Automate Flows Update

The fabric integration in this solution leverages 2 Power Automate workflows. The 1st flow (File Upload from SharePoint List to Fabric) is used to copy the Sustainability reports to Fabric from SharePoint and the 2nd flow (Output doc upload from SharePoint to Fabric) is used to copy the AI generated Benchmark or Gap Analysis Output doc to Fabric. In both Flows, we will need to update the Fabric API URL copied from Step 1.8.

1. Go to https://make.powerapps.com/ and make sure you are working on the correct environment from the top right created earlier from the DeployPowerPlatformClient readme.
2. Navigate to Flows from the menu on the right and click 'Edit' on the 'File Upload from Sharepoint List to Fabric' flow.
3. In the fabric API URL step, paste the URL that was copied from Step 1.8.
4. Click Save.
5. ![](C:\repos2\Comparative-Analysis-for-Sustainability-Solution-Accelerator\Deployment\images\client\fabricapiurlupdate.png)
6. Make sure the flow is switched on
7. Repeat steps 2-5 for the flow 'Output doc upload from Sharepoint to Fabric'.

