// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using CFS.SK.Sustainability.AI.Models;
using Microsoft.KernelMemory;
using Amazon.Auth.AccessControlPolicy;
using Microsoft.SemanticKernel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text;
using CFS.SK.Sustainability.AI.Storage.Benchmark;
using CFS.SK.Sustainability.AI.Storage.Benchmark.Entities;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Configuration;
using CFS.SK.Sustainability.AI.Services.Queue;
using ExcelDataReader.Log;
using Azure.Storage.Queues;
using CFS.SK.Abstracts;
using System;
using CFS.SK.Sustainability.AI.Storage.GapAnalysis;
using CFS.SK.Sustainability.AI.Storage.GapAnalysis.Entities;

namespace CFS.SK.Sustainability.AI.Host
{
    public static class ESRSHttpServiceMapper
    {
        public static void UseESRSEndpoint(this WebApplication app)
        {
            app.MapPost("/ESRS/ESRSDisclosureBenchmark", async ([FromServices] BenchmarkJobManager benchmarkJobManager ,[FromBody] BenchmarkServiceRequest benchmarkServiceRequest) =>
            {
                if (benchmarkJobManager != null)
                {
                    var result = (BenchmarkReportGenerationServiceExecutionResponse) await benchmarkJobManager.ExecuteAndReturnResult(benchmarkServiceRequest);
                    return Results.Ok<BenchmarkReportGenerationServiceExecutionResponse>(result);
                } else
                {
                    throw new Exception("Benchmark Job manager service not found");
                }

            })
            .Produces<BenchmarkReportGenerationServiceExecutionResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
            .WithRequestTimeout(TimeSpan.FromMinutes(60))
            .DisableAntiforgery();


            app.MapGet("/ESRS/ESRSDisclosureBenchmarkStatus/{JobId}", async ([FromServices] BenchmarkJobManager benchmarkJobManager,
                                                                       [FromServices] IConfiguration config,
                                                                       [FromServices] ILoggerFactory logger,
                                                                       string JobId) =>
            {
                if (JobId != null)
                {
                    var log = logger.CreateLogger<AzureStorageQueueService>();
                    var result = await benchmarkJobManager.GetBenchmarkJobByJobId(Guid.Parse(JobId));

                    if (result == null)
                    {
                        return Results.NotFound();
                    }
                    else
                    {
                        return Results.Ok(result);
                    }
                }
                else
                {
                    return Results.BadRequest();
                }
            })
            .Produces<BenchmarkReportGenerationServiceExecutionResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .DisableAntiforgery();



            app.MapPost("/ESRS/ESRSDisclosureBenchmarkOnQueue", async ([FromServices] BenchmarkJobManager benchmarkJobManager,
                                                                       [FromServices] IConfiguration config,
                                                                       [FromServices] ILoggerFactory logger,
                                                                       [FromServices] ApplicationContext appContext,
                                                                       [FromBody] BenchmarkServiceRequest benchmarkServiceRequest,
                                                                       HttpContext httpContext) =>
            {
                if (benchmarkServiceRequest != null)
                {
                    var log = logger.CreateLogger<AzureStorageQueueService>();
                    var queueClient = new AzureStorageQueueService(config["ConnectionStrings:BlobStorage"], "Benchmark", log);
                    await queueClient.ConnectToQueueAsync("Benchmark", QueueOptions.PublishOnly);

                    //Generate JobId
                    benchmarkServiceRequest.JobId = Guid.NewGuid();

                    //Serialize BenchmarkServiceRequest
                    var serializedBenchmarkServiceRequest = JsonConvert.SerializeObject(benchmarkServiceRequest);

                    await queueClient.EnqueueAsync(serializedBenchmarkServiceRequest);

                    var currentRequest = httpContext.Request;
                    var locationUrl = $"https://{currentRequest.Host.Value}/ESRS/ESRSDisclosureBenchmarkStatus/{benchmarkServiceRequest.JobId}";
                   
                    var paramJson = new { locationUrl = locationUrl };
                    var serializedParamJson = JsonConvert.SerializeObject(paramJson);
                    var content = new StringContent(serializedParamJson, Encoding.UTF8, "application/json");

                    //Invoke Process Watcher
                    var response = await appContext.httpClient.PostAsync(config["BenchmarkProcessing:processwatcherUrl"], content);

                    return Results.Accepted(locationUrl, benchmarkServiceRequest);
                }
                else
                {
                    return Results.BadRequest();
                }
            })
            .Produces<BenchmarkReportGenerationServiceExecutionResponse>(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
            .DisableAntiforgery();



            app.MapPost("/ESRS/ESRSDisclosureRetriever", async ([FromServices] ESRSDisclosureRetriever esrsDisclosureRetriever,
                                                                [FromBody] ESRSDisclosureRetrieverServiceRequest esrsDisclosureRetrieverServiceRequest) =>
            {
                if (esrsDisclosureRetriever != null)
                {
                    var result = await esrsDisclosureRetriever.ExecuteAndReturnResultAsMemoryAnswer(esrsDisclosureRetrieverServiceRequest);
                    return Results.Ok(result);
                }
                else
                {
                    throw new Exception("ESRSSampleGen service not found");
                }
            })
            .Produces<MemoryAnswer>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
            .DisableAntiforgery();

            app.MapGet("/ESRS/ESRSGapAnalysisStatus/{JobId}", async ([FromServices] ESRSGapAnalysisManager eSRSGapAnalysisManager,
                                                           [FromServices] IConfiguration config,
                                                           [FromServices] ILoggerFactory logger,
                                                           string JobId) =>
            {
                if (JobId != null)
                {
                    var log = logger.CreateLogger<AzureStorageQueueService>();
                    var result = await eSRSGapAnalysisManager.GetAnalysisJobByJobId(Guid.Parse(JobId));

                    if (result == null) 
                    {
                        return Results.NotFound();
                    }
                    else
                    {
                        return Results.Ok(result);
                    }
                }
                else
                {
                    return Results.BadRequest();
                }
            })
            .Produces<BenchmarkReportGenerationServiceExecutionResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .DisableAntiforgery();


            app.MapPost("/ESRS/ESRSGapAnalyzer", async ([FromServices] ESRSGapAnalysisManager eSRSGapAnalyzer, [FromBody] GapAnalysisServiceRequest gapAnalysisServiceRequest) =>
            {
                if (eSRSGapAnalyzer != null)
                {
                    var result = (GapAnalysisReportGenerationServiceExecutionResponse) await eSRSGapAnalyzer.ExecuteAndReturnResult(gapAnalysisServiceRequest);

                    //object deserialize
                    //var retObj = JsonConvert.DeserializeObject<GapAnalysisReportGenerationServiceExecutionResponse>(result);
                    return Results.Ok(result);
                }
                else
                {
                    throw new Exception("GapAnalyzer service not found");
                }
            })
            .Produces<GapAnalysisReportGenerationServiceExecutionResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
            .DisableAntiforgery();


            app.MapPost("/ESRS/ESRSGapAnalyzerOnQueue", async ([FromServices] ESRSGapAnalysisManager eSRSGapAnalyzer,
                                                               [FromServices] IConfiguration config,
                                                               [FromServices] ILoggerFactory logger,
                                                               [FromServices] ApplicationContext appContext,
                                                               [FromBody] GapAnalysisServiceRequest gapAnalysisServiceRequest,
                                                               HttpContext httpContext) =>
            {
                if (eSRSGapAnalyzer != null)
                {
                    var log = logger.CreateLogger<AzureStorageQueueService>();
                    var queueClient = new AzureStorageQueueService(config["ConnectionStrings:BlobStorage"], "GapAnalysis", log);
                    await queueClient.ConnectToQueueAsync("GapAnalysis", QueueOptions.PublishOnly);

                    //Generate JobId
                    gapAnalysisServiceRequest.JobId = Guid.NewGuid();

                    //Serialize GapAnalysisServiceRequest
                    var serializedGapAnalysisServiceRequest = JsonConvert.SerializeObject(gapAnalysisServiceRequest);

                    await queueClient.EnqueueAsync(serializedGapAnalysisServiceRequest);
                    var currentRequest = httpContext.Request;
                    var locationUrl = $"https://{currentRequest.Host.Value}/ESRS/ESRSGapAnalysisStatus/{gapAnalysisServiceRequest.JobId}";

                    var paramJson = new { locationUrl = locationUrl };
                    var serializedParamJson = JsonConvert.SerializeObject(paramJson);
                    var content = new StringContent(serializedParamJson, Encoding.UTF8, "application/json");

                    //Invoke Process Watcher
                    var response = await appContext.httpClient.PostAsync(config["GapAnalysisProcessing:processwatcherUrl"], content);

                    return Results.Accepted(locationUrl, gapAnalysisServiceRequest);
                }
                else
                {
                    return Results.BadRequest();
                }
            })
            .Produces<GapAnalysisReportGenerationServiceExecutionResponse>(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
            .DisableAntiforgery();


            app.MapGet("/ESRS/GetESRSBenchmarkResult", async ([FromServices] BenchmarkJobRepository benchmarkJobRepository, Guid benchmarkJobId) => {
                //Get Benchmark Job
                var benchmarkJob = await benchmarkJobRepository.FindByBenchmarkJobId(benchmarkJobId);
                if (benchmarkJob == null) return Results.NotFound();

                var blobConnectionString = app.Configuration["ConnectionStrings:BlobStorage"];
                //Create BlobServiceClient
                var blobServiceClient = new BlobServiceClient(blobConnectionString);
                //Create ContainerClient
                var blobContainerClient = blobServiceClient.GetBlobContainerClient("results");
                await blobContainerClient.CreateIfNotExistsAsync();
                //Get File
                var blobClient = blobContainerClient.GetBlobClient($"{benchmarkJob.JobName}-{benchmarkJob.JobId}.md");
                BlobDownloadInfo blobDownloadInfo = blobClient.Download();

                string strMarkdown = string.Empty;
                using (MemoryStream ms = new MemoryStream())
                {
                    blobDownloadInfo.Content.CopyTo(ms);
                    strMarkdown = Encoding.UTF8.GetString(ms.ToArray());
                }

                return Results.Content(strMarkdown, "text/markdown");
            });

            app.MapGet("/ESRS/GetAllESRSBenchmarkResults", async ([FromServices] BenchmarkJobRepository benchmarkJobRepository) => {
                //Get Benchmark Job
                var benchmarkJobs = await benchmarkJobRepository.GetAllDocuments();
                if (benchmarkJobs == null) return Results.NotFound();

                return Results.Ok(benchmarkJobs);
            })
            .Produces<IEnumerable<BenchmarkJob>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
            .DisableAntiforgery();

            app.MapGet("/ESRS/GetAllESRSGapAnalysisResults", async ([FromServices] GapAnalysisJobRepository eSRSGapAnalysisRepository) =>
            {
                //Get Analysis Job
                var gapAnalysisJobs = await eSRSGapAnalysisRepository.GetAllDocuments();
                if (gapAnalysisJobs == null) return Results.NotFound();

                return Results.Ok(gapAnalysisJobs);
            })
            .Produces<IEnumerable<GapAnalysisJob>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
            .DisableAntiforgery();
        }

    }
}
