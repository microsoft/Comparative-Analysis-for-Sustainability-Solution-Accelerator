# Fabric Setup (Optional Integration)

---

##  Disclaimer

Fabric integration in this solution is **optional**.  
If you choose **not** to use Fabric integration in this accelerator:
- You may skip this section entirely.
- Ensure the following Power Automate flows are **turned off**:
  - `Output doc upload from SharePoint to Fabric`
  - `File Upload from SharePoint List to Fabric`

---

##  Prerequisites

Before proceeding, ensure the following:

1. You have a valid **Microsoft Fabric License**.
2. You have permissions to **create a Fabric Workspace** and **Lakehouse**.
3. The **Power Platform Client solution** has already been imported and deployed.
4. Download the solution file: [`CompAnalysisFabricSolution.zip`](../Client/CompAnalysisFabricsolution.zip) from the `Client` folder in this accelerator.

---

##  Step 1: Create a Fabric Lakehouse

1. Go to [Fabric Portal](https://app.fabric.microsoft.com/).
2. Click on **Data Engineering**.
3. In the left panel, go to **Workspaces** > **New Workspace**.
4. Provide a name and click **Apply**. (Other fields are optional.)
5. Inside the new workspace, click **New Item** > **Lakehouse**.
6. Name your Lakehouse and click **Create**.
7. After the Lakehouse is created, right-click on **Files** > **Properties** > **Copy the URL**.
8. Save this URL for use in the next step.

![Fabric Lakehouse](./images/client/fabriclakehouse.png)

---

##  Step 2: Import the Fabric Power Platform Solution

1. Open the [PowerApps Maker Portal](https://make.powerapps.com).
2. Go to **Solutions** > **Import Solution**.
3. Browse and select `CompAnalysisFabricSolution.zip` > click **Next**.
4. During the import, all connections should be established automatically.
   - If not, click the **ellipsis (⋯)** > **Add new connection**.
   - For the **HTTP with Microsoft Entra ID** connector:
     - Click **Add new connection**.

     ![Connector Setup](./images/client/createconnectionfabric.png)

     - **Base Resource URL**: `https://onelake.dfs.fabric.microsoft.com`  
     - **Microsoft Entra Resource URI**: `https://storage.azure.com`  
     - Click **Sign In** > **Import**.

     ![OneLake Connector Setup](./images/client/onelakeconnectorsetup.png)

5. Once imported, verify that all solution components appear as expected.

   ![Solution Components](./images/client/fabricsolutioncomponents.png)

---

##  Step 3: Update Power Automate Flows for Fabric Integration

This solution includes **2 Fabric-specific flows**:

- `File Upload from SharePoint List to Fabric`  
  → Copies **sustainability reports** to Fabric.

- `Output doc upload from SharePoint to Fabric`  
  → Copies **AI-generated output documents** to Fabric.

### A. Configure `File Upload from SharePoint List to Fabric`

1. Go to [PowerApps Maker Portal](https://make.powerapps.com) and select the correct environment.
2. Navigate to **Flows** > **Edit** the `File Upload from SharePoint List to Fabric` flow.
3. Update the SharePoint steps:
   - Select your **SharePoint site** from the dropdown for:
     - `When an item is created`
     - `Get Attachment`
     - `Get File`
   - Choose the **SharePoint List** used to upload sustainability reports.

4. In the `Apply to each 2` step:
   - Update `Get Attachment content` with your SharePoint site and list.
   - Update `Create file` with SharePoint site and folder path (library: `Fabricoutputdocs`).
   - Update `Get file metadata` with the SharePoint site.
   - Update `Create txt file` with SharePoint site and path (`Fabricoutputdocs`).

5. In the **Fabric API URL** step:
   - Paste the **Lakehouse URL** copied from **Step 1.8**.

6. Click **Save**.

   ![Fabric API URL](./images/client/fabricapiurlupdate.png)

7. Ensure the flow is **turned on**.

---

### B. Configure `Output doc upload from SharePoint to Fabric`

1. Repeat steps **5–6** above for this flow.
2. Update SharePoint steps:
    - For `When a file is created`: select **your SharePoint site** and the `BenchmarkGapAnalysisOutputdocs` library.
    - For `Create txt file 2`: select **your SharePoint site** and the `Fabricoutputdocs` library.
    - For `Get file content`: select **your SharePoint site** only.

3. Click **Save**.

---

You have now completed the Fabric integration setup for this accelerator.

