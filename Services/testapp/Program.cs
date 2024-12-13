// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Spectre.Console;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Reflection;
using Tester.ConsoleApp.Functions;
using Microsoft.Extensions.Primitives;

// This program implements a console application that allows the user to interact with a REST API.
// The user can list registered documents, register new documents, delete documents,
//   perform gap analysis, and perform benchmarks. Below REST APIs are tested 
//GET / DocumentManager / GetAllDocuments
//POST / DocumentManager / DeleteDocumentByDocumentId / docId
//POST / DocumentManager / RegisterDocument
//POST / ESRS / ESRSDisclosureBenchmarkOnQueue
//GET / ESRS / GetAllESRSDisclosureBenchmarkResults
//POST / ESRS / ESRSGapAnalyzerOnQueue
//GET / ESRS / GetAllESRSBenchmarkResults


namespace Tester.ConsoleApp
{
    class Program
    {

        static async Task Main(string[] args)
        {

            Uri uri;
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appconfig.json", optional: false, reloadOnChange: true)
                .Build();

            var urlString = config["UrlSettings:UrlString"];

            try
            {
                uri = new Uri(urlString ?? throw new InvalidOperationException("URL string cannot be null"));
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine($"Error: {ex.Message}");
                return;
            }

            // Subscribe to configuration changes
            ChangeToken.OnChange(() => config.GetReloadToken(), () =>
            {
                AnsiConsole.WriteLine("Configuration has been reloaded.");
                // Handle any specific actions you need to take when the configuration is reloaded
            });

            while (true)
            {
                var selection = AnsiConsole.Prompt(
                    new SelectionPrompt<Choice>()
                    .Title("What would you like to do (use arrow keys to move cursor)?")
                    .PageSize(10)
                    .AddChoices(new[] {
                        Choice.ListRegisteredDocuments,
                        Choice.RegisterDocuments,
                        Choice.DeleteDocuments,
                        Choice.GapAnalysis,
                        Choice.GetAllGapAnalysisResults,
                        Choice.BenchMarks,
                        Choice.GetAllBenchMarksResults,
                        Choice.TestApiConnection,
                        Choice.Exit
                    })
                    .UseConverter(choice =>
                    {
                        var fieldInfo = choice.GetType().GetField(choice.ToString());
                        var attribute = fieldInfo?.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;

                        return attribute?.Description ?? choice.ToString();
                    }));

                switch (selection)
                {
                    case Choice.ListRegisteredDocuments:
                        ManageDocuments.ListDocuments(uri);
                        break;

                    case Choice.RegisterDocuments:
                        ManageDocuments.RegisterDocument(uri);
                        break;

                    case Choice.DeleteDocuments:
                        ManageDocuments.DeleteDocumentById(uri);
                        break;

                    case Choice.GapAnalysis:
                        GapAnalysis.PerformGapAnalysis(uri, config);
                        break;

                    case Choice.GetAllGapAnalysisResults:
                        GapAnalysis.GetAllGapAnalysisResults(uri);
                        break;

                    case Choice.BenchMarks:
                        await BenchMarks.PerformBenchmarks(uri, config);
                        break;

                    case Choice.GetAllBenchMarksResults:
                        BenchMarks.GetAllBenchmarksResults(uri);
                        break;

                    case Choice.TestApiConnection:
                        ApiConnection.TestConnection(uri);
                        break;

                    case Choice.Exit:
                        return;

                    default:
                        AnsiConsole.WriteLine("Sorry.  We didn't understand your selection.");
                        break;
                };
                AnsiConsole.WriteLine();
            }
        }
    }
}
