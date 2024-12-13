// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using RestSharp;
using Newtonsoft.Json.Linq;
using Spectre.Console;


namespace Tester.ConsoleApp.Functions
{
    public static class ManageDocuments
    {
        public static void ListDocuments(Uri uri)

        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var client = new RestClient(new RestClientOptions(uri)
            {
                ConfigureMessageHandler = _ => handler
            });

            //GET / DocumentManager / GetAllDocuments
            var request = new RestRequest("DocumentManager/GetAllDocuments", Method.Get);

            RestResponse response = client.Execute(request);
            var content = response.Content;

            AnsiConsole.WriteLine("Response Status: " + response.StatusCode);
            AnsiConsole.WriteLine("Response Content: " + content);
        }

        public static async void RegisterDocument(Uri uri)
        {

            // Create a custom HttpClientHandler to ignore SSL certificate errors
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var client = new RestClient(new RestClientOptions(uri)
            {
                ConfigureMessageHandler = _ => handler
            });

            var request = new RestRequest("DocumentManager/RegisterDocument", Method.Post);

            // Prompt user to input the full file path
            var filePath = AnsiConsole.Ask<string>("  Please enter the full file path:"); 
            //var filePath = "C:\\temp\\data\\sustainability";
            AnsiConsole.WriteLine($"The File path is: {filePath}");

            // Prompt user to input the file name
            var fileName = AnsiConsole.Ask<string>("  Please enter the file name only:");

            var fullFilePath = Path.Combine(filePath, fileName);

            if (File.Exists(fullFilePath))
            {
                byte[] fileData = File.ReadAllBytes(fullFilePath);
                request.AddFile("fileBinary", fileData, fileName, "application/octet-stream");
            }
            else
            {
                AnsiConsole.WriteLine("File not found: " + fullFilePath);
                return;
            }

            // Prompt user to input tag values

            // Prompt user to input tag values
            var classification = AnsiConsole.Ask<string>("  Please enter the Classification which can be the company name or the word competitor:");
            var title = AnsiConsole.Ask<string>("  Please enter the Title which is the company name:");
            var fy = AnsiConsole.Ask<string>("  Please enter the FY such as FY2024:");
            var year = AnsiConsole.Ask<string>("  Please enter the Year such as 2024:");
            var type = AnsiConsole.Ask<string>("  Please enter the Type that can be ESG or Sustainability:");


            // Add tags as form data
            var tags = $@"
                    [
                        {{ ""key"": ""Classification"", ""value"": ""{classification}"" }},
                        {{ ""key"": ""Title"", ""value"": ""{title}"" }},
                        {{ ""key"": ""FY"", ""value"": ""{fy}"" }},
                        {{ ""key"": ""Year"", ""value"": ""{year}"" }},
                        {{ ""key"": ""Type"", ""value"": ""{type}"" }}
                    ]";

            request.AddParameter("Tags", tags, ParameterType.GetOrPost);

            RestResponse response = await client.ExecuteAsync(request);
            var content = response.Content;

            AnsiConsole.WriteLine("Response Status: " + response.StatusCode);
            AnsiConsole.WriteLine("Response Content: " + content);

            // Check if the response is successful and content is not empty
            if (response.IsSuccessful && !string.IsNullOrEmpty(content))
            {
                try
                {
                    // Parse JSON response
                    var json = JObject.Parse(content);
                    AnsiConsole.WriteLine("Title: " + json["title"]);
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteLine("Error parsing JSON: " + ex.Message);
                }
            }
            else
            {
                AnsiConsole.WriteLine("Failed to get a valid response or content is empty.");
                AnsiConsole.WriteLine("Status Code: " + response.StatusCode);
                AnsiConsole.WriteLine("Error Message: " + response.ErrorMessage);
                AnsiConsole.WriteLine("Error Exception: " + response.ErrorException);
            }
        }

        public static void DeleteDocumentById(Uri uri)
        {
            var docId = AnsiConsole.Ask<string>(" Please provide document ID:");

            // Create a custom HttpClientHandler to ignore SSL certificate errors
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var client = new RestClient(new RestClientOptions(uri)
            {
                ConfigureMessageHandler = _ => handler
            });

            //Post / DocumentManager / DeleteDocumentByDocumentId /docId
            var request = new RestRequest("DocumentManager/DeleteDocumentByDocumentId", Method.Post);

            request.AddParameter("documentId", docId, ParameterType.GetOrPost);

            RestResponse response = client.Execute(request);
            var content = response.Content;

            AnsiConsole.WriteLine("Response Status: " + response.StatusCode);
            AnsiConsole.WriteLine("Response Content: " + content);

        }

    }

}
