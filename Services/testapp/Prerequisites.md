# Prerequisites 
## Prerequisites

1.	.NET 8 SDK: Ensure that you have the .NET 8 SDK installed on your machine. You can download it from  [.NET Download page](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
2.	Install either Visual Studio Code or Visual Studio if not already installed 
   1.	Visual Studio Code 
      •	Install [Visual Studio Code](https://code.visualstudio.com/)
      •	Install the following extensions:
      •	[C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
      •	[NuGet Package Manager](https://marketplace.visualstudio.com/items?itemName=jmrog.vscode-nuget-package-manager)
   2.	Visual Studio
      •	Install [Visual Studio](https://visualstudio.microsoft.com/vs/)
      •	During installation, ensure you select the ".NET desktop development" workload.
      Instructions

## Instructions for Visual Studio Code

1. Open the Project: Open Visual Studio Code and select File > Open Folder..., then navigate to the Tester-App folder and open it.
2. Restore Dependencies: Open the integrated terminal in Visual Studio Code (View > Terminal) and run the following command to restore the project dependencies: `dotnet restore` 

3. Build the Project: In the terminal, run the following command to build the project: `dotnet build`

4. Run the Project: To run the project, use the following command: `dotnet run --project Tester-App` 

## Instructions for Visual Studio

1. Open the Solution: Open Visual Studio 2022 or later and select File > Open > Project/Solution..., then navigate to the Tester-App folder and open the Tester-App.sln file.

2.	Restore Dependencies: Visual Studio will automatically restore the project dependencies when you open the solution. If it doesn't, you can manually restore them by right-clicking on the solution in the Solution Explorer and selecting Restore NuGet Packages.
3.	Build the Project: To build the project, select Build > Build Solution from the menu or press Ctrl+Shift+B.
4.	Run the Project: To run the project, press F5 or select Debug > Start Debugging from the menu.

## Additional Notes 

Ensure that the `appsettings.json` and `appconfig.json` files are present in the output directory. These files are configured to be copied to the output directory during the build process.

If you encounter any issues with missing dependencies or build errors, ensure that all required NuGet packages are restored and that you have the correct version of the .NET SDK installed.
