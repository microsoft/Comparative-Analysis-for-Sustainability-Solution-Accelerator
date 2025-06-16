# How to customize
If you'd like to customize the solution accelerator, you can update the client app and backend services as follows: 

**Client App**: You can import the solution zip file in `Client` folder and update the solution in Power App. 

**Azure Services**:  The Services are implemented using .NET C#, Kernel Memory, and Azure Open AI services, and other services illustrated in the solution architecture. You will need to update the source code in the `Services` folder to customize the solution. In addition, the deployment of these services are implemented in BICEP and PowerShell scripts. These scripts may need to be updated based on how the solution is customized. 

