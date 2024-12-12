using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Spectre.Console;
using RestSharp;
using System.Text;


namespace Tester.ConsoleApp.Functions
{
    public static class GapAnalysis
    {
        public static void PerformGapAnalysis(Uri baseUri, IConfiguration config)
        {
            var standardType = string.Empty;
            while (standardType != "CSRD" && standardType != "GRI")
            {
                standardType = AnsiConsole.Ask<string>("CSRD or GRI?:");
                if (standardType != "CSRD" && standardType != "GRI")
                {
                    AnsiConsole.MarkupLine("[red]Invalid input. Valid values: 'CSRD' or 'GRI'.[/]");
                }
            }

            AppConfig appConfig = new AppConfig();
            appConfig = Helpers.ConfigHelper.GetAppConfig(standardType, config);

            var docID = AnsiConsole.Ask<string>("Please enter the document ID for Gap Analysis:");
            // Construct the JSON body
            var jsonBody = $@"
            {{
                ""JobOwner"": ""{appConfig.JobOwner}"",
                ""disclosureName"": ""{appConfig.disclosureName}"",
                ""disclosureNumber"": ""{appConfig.disclosureNumber}"",
                ""jobOwner"": ""{appConfig.JobOwner}"",
                ""disclosureRequirement"": ""{appConfig.disclosureRequirement}"",
                ""disclosureRequirementDetail"": ""{appConfig.disclosureRequirementDetail}"",
                ""disclosureAnnex"": ""{appConfig.disclosureAnnex}"",
                ""documentId"": ""{docID}""
            }}";


            // Create a custom HttpClientHandler to ignore SSL certificate errors
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var client = new RestClient(new RestClientOptions(baseUri)
            {
                ConfigureMessageHandler = _ => handler
            });

            ////BaseUrl/ESRS/ESRSGapAnalyzerOnQueue
            var request = new RestRequest("ESRS/ESRSGapAnalyzerOnQueue", Method.Post);

            //// Add the JSON body to the request
            request.AddStringBody(jsonBody, DataFormat.Json);

            var response = client.Execute(request);

            AnsiConsole.WriteLine("ESRSGapAnalyzerOnQueue Response Status: " + response.StatusCode);
            AnsiConsole.WriteLine("Response Content: " + response.Content);
        }


        public static void GetAllGapAnalysisResults(Uri uri)
        {

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var client = new RestClient(new RestClientOptions(uri)
            {
                ConfigureMessageHandler = _ => handler
            });
            //BaseUrl/ESRS/GetAllESRSGapAnalysisResults
            var request = new RestRequest("ESRS/GetAllESRSGapAnalysisResults", Method.Get);

            RestResponse response = client.Execute(request);
            var content = response.Content;

            AnsiConsole.WriteLine("GetAllESRSGapAnalysisResults Response Status: " + response.StatusCode);
            AnsiConsole.WriteLine("GetAllESRSGapAnalysisResults Response Content: " + content);
        }
    }
}
