# Set up Client side
## Pre Requisites
1. Power Platform environment with System Administrator access.
2. PowerApps license.
3. Copilot Studio license.
4. AI Builder / AI Prompts access.
5. Privilege to grant admin consent for tenant in an Azure App Registration.
6. Access to create a SharePoint site.
7. Backend services of this accelerator must have been deployed already.
8. Download [PowerPlatformClient.zip](../Client/PowerPlatformClient.zip) file from the `Client` folder of this accelerator.
9. Download the 'Disclosure Detail' excel file from the `Client` folder of this accelerator.

## Key Components included in the Client Solution Deployment
The client solution will consist of the following major components, which will be set up step by step in the subsequent sections:
1. GenAI Benchmarking and Gap Analysisv2 PowerApp.
2. Power Automate flows.
3. Custom Copilot.
4. SharePoint Site _(Note: The SharePoint site itself will not be deployed as part of the solution but is required for the setup. Additional details are provided in the section below.)_

### Step 1: Create SharePoint Site
This is a completely manual step, as the Power Platform solution does not deploy a `SharePoint` site. This site is required to store documents uploaded via the frontend and pass them to the backend and Fabric (if applicable), where they are used to generate Benchmark and Gap Analysis reports.

1. Navigate to https://(your-tenant).sharepoint.com/ and click Create site.
2. Select Team Site.
3. The site must include:
    + One SharePoint List for storing sustainability report records.
    + Two custom SharePoint Document Libraries:
      + One for storing AI-generated output reports.
      + One for storing documents sent to Fabric (only if Fabric is used).
    + The default Document Library to store an Excel file containing metadata required by the backend API (based on the disclosure selected during record creation).

#### Step 1.1: Upload Disclosure metadata File to Default Document Library
1. Upload the 'Disclosure Detail' excel file downloaded from the Client folder in the Document Library. 

#### Step 1.2: Create new Document Library
1. [Create a new SharePoint document library](https://support.microsoft.com/en-us/office/create-a-document-library-in-sharepoint-306728fe-0325-4b28-b60d-f902e1d75939#ID0EBF=Modern) and name it 'BenchmarkGapAnalysisOutputdocs'.
2. This library will store all the output documents and will be used as a knowledge source from the Copilot. Please make sure the 'Disclosure details' excel file is NOT uploaded in this library.
3. <u>If you intend to use Fabric</u>, create another SharePoint document library and name it 'Fabricoutputdocs'.
4. This document library will store all Sustainability Reports and Output docs which will be sent over to a Fabric Lakehouse <u>(Fabric setup is covered in a subsequent readme).</u>

#### Step 1.3: Create the 'SustainabilityReports' SharePoint List

1. [Create a new SharePoint List](https://learn.microsoft.com/en-us/sharepoint/dev/business-apps/get-started/set-up-sharepoint-site-lists-libraries) and name it **`SustainabilityReports`**.

2. Add the following columns to the list:
   - `Title` (Type: Text)
   - `Year` (Type: Text)
   - `Classification` (Type: Choice)  
     - Example choices: `Your company name`, `Competitors`, etc.  
     - These represent different categories or types of sustainability reports that will be uploaded.

3. **Important:** Ensure there are no spelling errors in the above three column names, as they are referenced directly in Power Automate flows.

4. You may add additional columns as needed to support your specific use case.

5. *Note:* Once the SharePoint components are created, it may take up to **2 hours** for them to become available in Power Automate.


## Step 2: Import Power Platform Solution
Before configuring any components of the solution, you must first import the solution into your Power Platform environment.

1. Navigate to the [Power Apps Maker Portal](https://make.powerapps.com).
2. Go to **Solutions** > **Import Solution**.
3. Click **Browse**, select the file **`PowerPlatformClient.zip`**, and click **Next**.

![Import Power Platform Solution](./images/client/import-solution-in-powerplatform.png)

4. During the import, all required connections should be established automatically.  
   - If any connection isn't set up, click the **ellipsis (⋯)** next to it and select **"Add new connection"**.

5. Once the import is complete, the solution will be available in your environment.  
   - To verify a successful import, open the solution and ensure the number of components matches the example below.

![Validate Imported Components](./images/client/validate-imported-components.png)

### Step 3: Configure Power Automate Workflows

This solution includes **7 Power Automate workflows**.  
Ensure that **all 7 of them are turned on**. If any workflow is off, click the **ellipsis (⋯)** next to its name and select **"Turn On"**.

These flows require updates to two key configuration values:
- **Backend API URL**
- **SharePoint Site URL**

---

### Step 3.1: Update Backend API URL in Power Automate flows

1. Open the **`PA-GetAllBenchmarkResults`** flow.
2. Click **Edit**.
3. Locate the step for the **backend API URL**, and paste the deployed Azure backend URL into the **Value** field.
4. Click **Save**.

![Backend API URL](./images/client/backend-api-url.png)

5. Repeat steps 2–4 for the following flows:
   - `PA-GetAllGapAnalysisResults`
   - `Getalldocumentsfromapi`
   - `File upload from SharePoint list to API`
   - `PA-GapAnalysis`
   - `PA-Benchmark`

---

### Step 3.2: Update SharePoint URLs in Power Automate flows

#### 3.2.1 File Upload Flow
1. Open the **`File upload from SharePoint list to API`** flow.
2. Click **Edit**.
3. Update the three SharePoint actions with:
   - The **SharePoint Site URL**
   - The **SharePoint list** created earlier  
   *(Use the dropdowns to select the correct site address and list name.)*
4. Click **Save**.

![SharePoint URL - File Upload](./images/client/sharepointurl1.png)

---

#### 3.2.2 Output Notification in Teams Flow
5. Open the **`output notification in Teams`** flow.
6. Click **Edit**.
7. Update the SharePoint action with:
   - The same **SharePoint Site URL**
   - The **SharePoint Library Name**  
   *(Use the dropdowns to select them.)*
8. Click **Save**.

---

#### 3.2.3 PA-Benchmark Flow
9. Open the **`PA-Benchmark`** flow.
10. Click **Edit**.
11. Locate the **'Create File'** action and update it with:
    - The **SharePoint Site URL**
    - The **output document library** created earlier.
12. Click **Save**.

![SharePoint URL - PA-Benchmark](./images/client/pabenchmark-sharepointurl.png)

---

#### 3.2.4 PA-GapAnalysis Flow
13. Repeat steps **9–12** for the **`PA-GapAnalysis`** flow.


### Step 4: Configure Copilot Studio

---

#### Step 4.1: Connect SharePoint as a Knowledge Source

1. Go to [Power Virtual Agents (Copilot Studio)](https://web.powerva.microsoft.com/) and select the **Power Platform environment** where the client solution was deployed.
2. Navigate to the **Knowledge** section.
   - If any existing knowledge sources are present, remove them by clicking the **ellipsis (⋯)** next to each item and selecting **Delete**.
3. Add a new SharePoint-based knowledge source:
   - Click **Add Knowledge** > Select **SharePoint**.
   - Enter your **SharePoint site URL**.
   - Click **Save**.

![Copilot Studio – Add Knowledge Source](./images/client/copilotstudio-knowledge2.png)

4. Go to **Topics** > **Conversational Boosting**.
   - Locate the node titled **Create Generative Answers**.
   - Click the **ellipsis (⋯)** on that node.
   - Select the SharePoint knowledge source you added above.
   - Click **Save** in the top right corner.

![Copilot Studio – Configure Knowledge Source](./images/client/copilotstudioaddknowledgesource.png)

---

#### Step 4.2: Set Up Manual Authentication

1. Manual authentication is required for SharePoint to function as a knowledge source.
   - Follow the steps in this [official tutorial video](https://www.youtube.com/watch?v=rw4IwR68Wc0) by the Copilot Studio product team.
2. After configuring authentication, click **Publish** to make your Copilot changes live.

---

#### Step 4.3: Enable Microsoft Teams Channel

1. Navigate to **Channels** > **Microsoft Teams**.
2. Click **Turn On Teams** to enable your Copilot for Microsoft Teams.
   - This allows your Copilot to send notifications and respond to questions regarding output documents.
3. Click **Publish** again to finalize the setup.

![Copilot – Teams Configuration](./images/client/copilotteams.png)


### Step 5: Update the PowerApp

---

#### Step 5.1: Configure SharePoint URL and Classification Filters

1. Open the [Power Apps Maker Portal](https://make.powerapps.com) and navigate to:  
   **Apps > GenAI Benchmarking and Gap Analysis v2 App**.

2. In the **`Documents_API`** screen, select the **"Upload new"** button.

3. Update the **`OnSelect`** property of the button (Refer following screenshot):
   - Replace the SharePoint List URL value inside the quotation marks (`" "`).
   - Use the SharePoint List URL created in [Step 1.3](#step-13-create-the-sustainabilityreports-sharepoint-list).

   ![Update SharePoint URL](./images/client/updateurlpowerapp.png)

4. In the **`Benchmark-Create`** screen:
   - For the **`Items`** property in the **2_1 Gallery**, update the `Classification` value to your company’s value defined in Step 1.3.
   - For **Gallery 2**, update the `Classification` if you configured a different value than `"Competitor"`.  
     - *If "Competitor" is already used, no changes are needed.*

5. Repeat the update for **Gallery 2_2** in the **`Create Gap Analysis`** screen.

---

#### Step 5.2: Resolve Excel File Error (If Applicable)

6. If you encounter an Excel-related error while using the app in **Create Benchmark** or **Create Gap Analysis** screens:

   ![Excel File Error](./images/client/ExcelError.png)

7. This error typically indicates a **permission issue with the Excel file**.

8. To resolve:
   - Delete the Excel file from the **Data section** of the PowerApp.
   - Reupload the correct Excel file (from Step 1.1) into the **SharePoint Document Library**.

9. Refresh the SharePoint connection in the PowerApp as shown below:

   ![Refresh Connection](./images/client/refreshconnection.png)

---

10. Finally, click **Save** and then **Publish** the Power App from the top right corner.
    
### Disclaimer
The Copilot control in canvas app is currently non-functional and included for future use. It will be activated once support for the required data source becomes available.


