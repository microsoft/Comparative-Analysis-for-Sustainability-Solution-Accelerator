// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.Core;
using Azure.Core.Diagnostics;
using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Utils.TokenGenerator;

//using UglyToad.PdfPig;
//using UglyToad.PdfPig.Content;
//using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Microsoft.KernelMemory.DataFormats.Pdf;

public class PdfDecoder
{
    private DocumentIntelligenceClient _client;
    // Lock helpers
    private readonly object _lock = new();
    private bool _busy = false;
    private static string apiKey;
    private static string endpoint;

    public PdfDecoder()
    {
    }

    //static constructor
    static PdfDecoder()
    {
        //Set the file location
        var appConfigFileLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var appConfigFile = Path.Combine(Path.GetDirectoryName(appConfigFileLocation), "appsettings.Development.json");

        //Read Configuration from appsettings.Development.json file for Development Environment
        //the file should be changed in production
        //Read configuration file under executing directory
        //It must changed in production

        var config = new ConfigurationBuilder()
            .AddJsonFile(appConfigFile)
            .Build();

        //Read configuration value under KernelMemory/Services/AzureAIDocIntel/APIKey
        //PdfDecoder.apiKey = config["KernelMemory:Services:AzureAIDocIntel:APIKey"];
        //Read configuration value under KernelMemory/Services/AzureAIDocIntel/Endpoint
        PdfDecoder.endpoint = config["KernelMemory:Services:AzureAIDocIntel:Endpoint"];

    }

    public FileContent ExtractContent(string filename)
    {
        using var stream = File.OpenRead(filename);
        return this.ExtractContent(stream);
    }

    //Method Invocation count for ExtractContent Method within the instance
    static int invocationCount = 0;

    public FileContent ExtractContent(BinaryData data)
    {
        if (this._busy)
        {
            return null;
        }

        lock (this._lock)
        {
            this._busy = true;

            //using AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger();
            var content = new AnalyzeDocumentContent() { Base64Source = data };

            //this invocation should be blocking during process
            DocumentIntelligenceClientOptions options = new()
            {
                Retry = { Delay = TimeSpan.FromSeconds(90), MaxDelay = TimeSpan.FromSeconds(180), MaxRetries = 3, Mode = RetryMode.Exponential },
            };

            var credential = TokenCredentialProvider.GetCredential();
            this._client = new DocumentIntelligenceClient(new Uri(PdfDecoder.endpoint), credential, options);

            Operation<AnalyzeResult> operation = null;
            operation = this._client.AnalyzeDocument(WaitUntil.Completed, "prebuilt-layout", content, outputContentFormat: ContentFormat.Markdown);
            AnalyzeResult result = operation.Value;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Invoked {PdfDecoder.invocationCount++} Times");
            Console.ResetColor();

            var extracted_result = new FileContent();

            extracted_result.Sections.Add(new(1, result.Content, true));
            this._busy = false;
            return extracted_result;
        }
    }

    public FileContent ExtractContent(Stream data)
    {
        //Stream to BinaryData
        using var memoryStream = new MemoryStream();
        data.CopyTo(memoryStream);
        BinaryData binaryData = new BinaryData(memoryStream.ToArray());

        return this.ExtractContent(binaryData);
    }
}

//public class PdfDecoder
//{
//    public FileContent ExtractContent(string filename)
//    {
//        using var stream = File.OpenRead(filename);
//        return this.ExtractContent(stream);
//    }

//    public FileContent ExtractContent(BinaryData data)
//    {
//        using var stream = data.ToStream();
//        return this.ExtractContent(stream);
//    }

//    public FileContent ExtractContent(Stream data)
//    {
//        var result = new FileContent();

//        using PdfDocument? pdfDocument = PdfDocument.Open(data);
//        if (pdfDocument == null) { return result; }

//        foreach (Page? page in pdfDocument.GetPages().Where(x => x != null))
//        {
//            // Note: no trimming, use original spacing
//            string pageContent = (ContentOrderTextExtractor.GetText(page) ?? string.Empty);
//            result.Sections.Add(new FileSection(page.Number, pageContent, false));
//        }

//        return result;
//    }
//}
