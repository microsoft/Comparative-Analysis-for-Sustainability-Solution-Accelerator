using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using Spectre.Console;
using System.Text;

namespace Tester.ConsoleApp.Functions
{
    public static class BenchMarks
    {
        public static async Task PerformBenchmarks(Uri baseUri, IConfiguration config)
        {

            //var appConfig = new AppConfig
            //{
            //    JobOwner = config["AppConfig:JobOwner"] ?? string.Empty,
            //    Type = config["AppConfig:Type"] ?? string.Empty,
            //    disclosureNumber = config["AppConfig:disclosureNumber"] ?? string.Empty,
            //    disclosureName = config["AppConfig:disclosureName"] ?? string.Empty,
            //    disclosureRequirement = config["AppConfig:disclosureRequirement"] ?? string.Empty,
            //    disclosureRequirementDetail = config["AppConfig:disclosureRequirementDetail"] ?? string.Empty,
            //    disclosureAnnex = config["AppConfig:disclosureAnnex"] ?? string.Empty
            //};

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

            var client = new HttpClient();
            client.BaseAddress = baseUri;

            var myJobName = AnsiConsole.Ask<string>("Enter a Job Name for Benchmarks Analysis:");

            // user input for documentIds
            var myDocumentIds = new List<string>();
            while (true)
            {
                var myDocumentId = AnsiConsole.Ask<string>("Enter a document ID or the word 'Finished' to end:");
                if (myDocumentId.Equals("Finished", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                myDocumentIds.Add(myDocumentId);
            }
            var requestBody = new
            {
                jobName = myJobName,
                documentIds = myDocumentIds,
                jobOwner = config["AppConfig:JobOwner"] ?? string.Empty,
                disclosureNumber = config["AppConfig:disclosureNumber"] ?? string.Empty,
                disclosureName = config["AppConfig:disclosureName"] ?? string.Empty,
                disclosureRequirement = config["AppConfig:disclosureRequirement"] ?? string.Empty,
                disclosureRequirementDetail = config["AppConfig:disclosureRequirementDetail"] ?? string.Empty,
                disclosureAnnex = config["AppConfig:disclosureAnnex"] ?? string.Empty
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("ESRS/ESRSDisclosureBenchmarkOnQueue", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine("[green]Benchmark documents request was successful.[/]");
                AnsiConsole.WriteLine(responseContent);
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Benchmark documents request failed.[/]");
                AnsiConsole.WriteLine(response.ReasonPhrase ?? "No reason phrase provided.");
            }
        }

        public static void GetAllBenchmarksResults(Uri uri)
        {
            // Implement the logic to list all models
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var client = new RestClient(new RestClientOptions(uri)
            {
                ConfigureMessageHandler = _ => handler
            });

            //BaseUrl/ESRS/GetAllESRSBenchmarkResults
            var request = new RestRequest("ESRS/GetAllESRSBenchmarkResults", Method.Get);

            RestResponse response = client.Execute(request);
            var content = response.Content;

            AnsiConsole.WriteLine("GetAllESRSBenchmarkResults Response Status: " + response.StatusCode);
            AnsiConsole.WriteLine("GetAllESRSBenchmarkResults Response Content: " + content);

        }

    }
}
