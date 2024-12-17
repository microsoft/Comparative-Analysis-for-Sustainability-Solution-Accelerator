// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System.Security.Cryptography.X509Certificates;
using Spectre.Console;

namespace Tester.ConsoleApp.Functions
{
    public static class ApiConnection
    {
        public static async void TestConnection(Uri uri)
        {
            // Create a custom HttpClientHandler to handle SSL certificate validation
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    // Here you can add custom logic to validate the certificate
                    // For example, you can check the certificate thumbprint, issuer, etc.
                    // Returning true will bypass the SSL certificate validation
                    if (cert == null)
                    {
                        AnsiConsole.WriteLine("No Server Certificate Found");
                        return false;
                    }
                    return ValidateCertificate(cert);
                }
            };

            // Create an HttpClient using the custom handler
            using (var client = new HttpClient(handler))
            {
                // Set the base address of the API
                client.BaseAddress = uri;
                try
                {
                    // Make a GET request to the API
                    HttpResponseMessage response = await client.GetAsync("/api/endpoint");
                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content
                        string content = await response.Content.ReadAsStringAsync();
                        AnsiConsole.WriteLine("\nTest Connection Response Content: " + content);
                        AnsiConsole.WriteLine(); // Write a new line
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteLine("\nTest Connection Failed with Exception (check your appsettings.json and services): " + ex.Message);
                    AnsiConsole.WriteLine(); // Write a new line
                }
            }
        }

        // Validate the SSL certificate
        static bool ValidateCertificate(X509Certificate2 cert)
        {
            // Add custom validation logic here
            // For example, check the certificate thumbprint, issuer, expiration date, etc.
            AnsiConsole.WriteLine("Certificate Subject: " + cert.Subject);
            AnsiConsole.WriteLine("Certificate Issuer: " + cert.Issuer);
            AnsiConsole.WriteLine("Certificate Thumbprint: " + cert.Thumbprint);
            AnsiConsole.WriteLine("Certificate Expiration: " + cert.NotAfter);
            AnsiConsole.WriteLine(); // Write a new line

            // Example: Validate the certificate thumbprint. This is just an example, you should use a valid thumbprint.
            string expectedThumbprint = "B89BB8B0BEF4B6CF59A472284B4F8F234525302B";
            if (cert.Thumbprint == expectedThumbprint)
            {
                return true;
            }

            // Example: Validate the certificate issuer. This is just an example, you should use a valid issuer.
            string expectedIssuer = "Kubernetes Ingress Controller Fake Certificate";
            if (cert.Issuer == expectedIssuer)
            {
                return true;
            }

            // Example: Validate the certificate expiration date
            if (DateTime.Now < cert.NotAfter)
            {
                return true;
            }

            // If none of the validation checks pass, return false
            return false;
        }
    }
}
