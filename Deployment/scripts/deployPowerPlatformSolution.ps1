# Copyright (c) Microsoft Corporation.
# Licensed under the MIT license.

# Read input from the user
$environmentUrl = Read-Host "Enter the environment URL (e.g. https://your-environment.crm.dynamics.com)" 
$envId = Read-Host "Enter the environment ID (e.g. dbd8ef12-e60a-efb1-b841-716cb9527f17)"

# Path for solution file
$solFileRelPath = "../../Client/PowerPlatformClient.zip"
$solFilePath = Resolve-Path -Path $solFileRelPath

if (-not $solFilePath) {
    Write-Error "The solution file path is not valid, please make sure you are running the script from 'ComparativeAnalysis\Deployment\scripts' folder"
    exit
}

# Check if .NET SDK is installed
$dotnetPath = (Get-Command dotnet -ErrorAction SilentlyContinue)?.Path

# Download install script
$dotnetInstallScript = "dotnet-install.ps1"
Invoke-WebRequest -uri "https://dot.net/v1/dotnet-install.ps1" -OutFile $dotnetInstallScript

if ($dotnetPath) {
    Write-Host ".NET SDK is already installed at $dotnetPath. Skipping installation."
}
else {
    Write-Host ".NET SDK not found. Downloading and installing .NET SDK..."

    # Install .NET SDK
    & .\$dotnetInstallScript -Version latest -Channel 7.0

    # Check if installation was successful
    if (Get-Command dotnet -ErrorAction SilentlyContinue) {
        Write-Host ".NET SDK installed successfully."
    }
    else {
        Write-Host "Failed to install .NET SDK."
        exit 1
    }
}

# Check if ASP.NET Core runtime is installed
$aspnetRuntime = dotnet --list-runtimes | Select-String "Microsoft.AspNetCore.App"

if ($aspnetRuntime) {
    Write-Host "ASP.NET Core runtime is already installed. Skipping installation."
}
else {
    Write-Host "ASP.NET Core runtime not found. Installing ASP.NET Core runtime..."
    
    # Install ASP.NET Core runtime
    & .\$dotnetInstallScript -Version latest -Runtime aspnetcore

    # Verify ASP.NET Core runtime installation
    if (dotnet --list-runtimes | Select-String "Microsoft.AspNetCore.App") {
        Write-Host "ASP.NET Core runtime installed successfully."
    }
    else {
        Write-Host "Failed to install ASP.NET Core runtime."
        exit 1
    }
}

# Set environment variables for .NET
$env:DOTNET_ROOT = "$HOME\.dotnet"
$env:PATH = "$env:PATH;$dotnetRoot;$dotnetRoot\tools"

# Check if the PAC CLI command is available
if (-Not (Get-Command pac -ErrorAction SilentlyContinue)) {
    try {
        # Attempt to install the Power Platform CLI
        dotnet tool install --global Microsoft.PowerApps.CLI.Tool -ErrorAction Stop
        Write-Host "Power Platform CLI has been installed successfully."
    }
    catch {
        Write-Error "Failed to install Power Platform CLI. Error details: $_"
        Write-Host "If it continues to fail and alternate would be to install from the msi file at https://learn.microsoft.com/en-us/power-platform/developer/howto/install-cli-msi"
        Exit 1  # Exits with a status code of 1, indicating an error
    }
}
else {
    Write-Host "Power Platform CLI is already installed."
}

# Read this documentation for 'pac auth create' command:
# https://learn.microsoft.com/en-us/power-platform/developer/cli/reference/auth#pac-auth-create

#Code for Interective Authentication
write-host "Autheticating."
pac auth create --environment $environmentUrl 

write-host "Importing solution file..." -ForegroundColor Green
Write-Host "Solution file: $solFilePath"
Write-Host "Environment ID: $envId"

#Import the solution file
pac solution import --path $solFilePath --environment $envId  --force-overwrite