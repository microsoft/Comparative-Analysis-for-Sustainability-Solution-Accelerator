# Deploy PowerApps Solution Zip File Using PowerShell Script 

The deployment of the Power Platform solution .zip file can be automated by PowerShell script. This is provided as an optional method, as it may be deemed useful for process automation in future. 

### Steps to Execute the Script

1. Open PowerShell App.

2. Navigate to the Script Directory and execute the script:

   ```cd Deployment\scripts
   cd Deployment\scripts
   .\deployPowerPlatformSolution.ps1
   ```


3. You will need the permission to deploy solution for your environment, and you will be asked to provide authentication information to login. 

4. You will need to provide below input:

   `$environmentUrl:` The Power Platform environment URL (e.g. https://your-environment-name.crm.dynamics.com) 

   `$environmentId`: The Power Platform environment ID (e.g. dbd8ef12-e60a-efb1-b841-716cb9527f17)

### Prerequisites to be installed by PowerShell Scripts it not exist 

1. **.NET SDK**: The script installs the .NET SDK.

2. **Microsoft Power Platform CLI**: The script installs the Power Platform CLI tool.

   
