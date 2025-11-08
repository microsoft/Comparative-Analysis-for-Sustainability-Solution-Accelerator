using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace CFS.SK.Sustainability.AI.Utils;

/// <summary>
/// Provides token credentials for Azure services using either Azure CLI or Managed Identity
/// </summary>
public static class TokenCredentialProvider
{
    /// <summary>
    /// Gets an appropriate TokenCredential based on the runtime environment (production vs development)
    /// </summary>
    /// <param name="clientId">Optional client ID for user-assigned managed identity</param>
    /// <param name="logger">Optional logger for diagnostic information</param>
    /// <returns>A TokenCredential instance</returns>
    public static TokenCredential GetCredential(ILogger? logger = null)
    {
        TokenCredential credential;
        
        // Detect environment - Production uses Managed Identity, Development uses Azure CLI
        bool isProduction = IsProductionEnvironment();
        
        logger?.LogInformation("Detected environment: {Environment}", isProduction ? "Production" : "Development");

        if (isProduction)
        {
            logger?.LogInformation("Using ManagedIdentityCredential for production authentication");
            credential = new ManagedIdentityCredential();
        }
        else
        {
            logger?.LogInformation("Using AzureCliCredential for development authentication");
            credential = new AzureCliCredential();
        }

        return credential;
    }

    /// <summary>
    /// Determines if the current environment is production based on environment variables and settings
    /// </summary>
    /// <returns>True if running in production, false if in development</returns>
    private static bool IsProductionEnvironment()
    {
        // Check AZURE_TOKEN_CREDENTIALS
        string? environment = Environment.GetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS");
        
        if (!string.IsNullOrEmpty(environment))
        {
            return environment.Equals("ManagedIdentityCredential", StringComparison.OrdinalIgnoreCase);
        }
        
        return false;
    }
}