// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Monitor.OpenTelemetry.Exporter;
using CFS.SK.Abstracts;
using CFS.SK.Sustainability.AI;
using CFS.SK.Sustainability.AI.Host;
using CFS.SK.Sustainability.AI.Services;
using CFS.SK.Sustainability.AI.Storage.Benchmark;
using CFS.SK.Sustainability.AI.Storage.Document;
using CFS.SK.Sustainability.AI.Storage.GapAnalysis;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.KernelMemory;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net;
using System.Reflection.PortableExecutable;

var builder = Startup.CreateHostBuilder(args);

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS")))
{
    Environment.SetEnvironmentVariable("AZURE_TOKEN_CREDENTIALS", "dev");
}

// Configure Logging
ConfigureLogging(builder);

// Configure Services
ConfigureServices(builder);

// Configure HTTP Clients
ConfigureHttpClients(builder);

// Configure Hosted Services
ConfigureHostedServices(builder);

var app = builder.Build();

// Configure Middleware
ConfigureMiddleware(app);

var startTime = DateTime.Now;
app.MapGet("/", () =>
{
    var upTime = DateTime.Now - startTime;
    var responseString = $"Running up so far.... {upTime.ToString()}";
    return Results.Ok(responseString);
});

app.Run();

void ConfigureLogging(WebApplicationBuilder builder)
{
    // Adding OpenTelemetry Logging
    //builder.Logging.AddOpenTelemetry(options =>
    //{
    //    options.AddAzureMonitorLogExporter(options =>
    //        options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]);
    //});

    // Add Dependency for Microsoft.Extension.Logging
    builder.Logging.Services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddConsole();
        loggingBuilder.AddDebug();
    });
}

void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.WriteIndented = true;
    });

    builder.Services.Configure<KestrelServerOptions>(options =>
    {
        options.Limits.MaxRequestBodySize = 500 * 1024 * 1024; // 500 MB
    });

    builder.Services
        .AddEndpointsApiExplorer()
        .AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "ESG AI Document Service API", Version = "v1" });
            options.UseCustomSchemaIds();
        })
        .AddSingleton<IKernelMemory, MemoryWebClient>(x => 
                new MemoryWebClient(endpoint: builder.Configuration["KernelMemory:ServiceUrl"], new HttpClient() { Timeout = new TimeSpan(0,20,0)}))
        .AddSingleton<DocumentRepository>(x => new DocumentRepository(builder.Configuration["ConnectionStrings:MongoDB"], "Documents"))
        .AddSingleton<BenchmarkJobRepository>(x => new BenchmarkJobRepository(builder.Configuration["ConnectionStrings:MongoDB"], "Benchmarks"))
        .AddSingleton<GapAnalysisJobRepository>(x => new GapAnalysisJobRepository(builder.Configuration["ConnectionStrings:MongoDB"], "GapAnalysis"))
        .AddSingleton<ApplicationContext>()
        .AddSingleton<ESRSDisclosureRetriever>()
        .AddSingleton<DocumentManager>()
        .AddSingleton<BenchmarkJobManager>()
        .AddSingleton<ESRSGapAnalysisManager>()
        .AddSingleton<ESRSBenchmarkReportGenerator>();
}

void ConfigureHttpClients(WebApplicationBuilder builder)
{
    builder.Services
        .AddHttpClient<ApplicationContext>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(15);
        })
        .AddPolicyHandler((services, request) =>
        {
            return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        });

    builder.Services
        .AddHttpClient<DocumentManager>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
            client.BaseAddress = new Uri(builder.Configuration["DocumentPreprocessing:processwatcherUrl"]);
        })
        .AddPolicyHandler((services, request) =>
        {
            return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        });
}

void ConfigureHostedServices(WebApplicationBuilder builder)
{
    // Hosted Services by Kestrel
    builder.Services.AddHostedService<QueueServiceHost>();
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ESG AI Document Service API v1"));
    }

    app.UseSwaggerUI();
    app.UseSwagger();
    app.UseRouting();
    app.UseESRSEndpoint();
    app.UseDocumentManagerEndpoint();
}
