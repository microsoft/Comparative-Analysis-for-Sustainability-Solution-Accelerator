// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Azure.Monitor.OpenTelemetry.Exporter;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace CFS.SK.Abstracts
{
    public class ApplicationContext
    {
        public HttpClient httpClient;

        public IConfiguration configuration { get; }
        public ApplicationContext(HttpClient httpClient, IConfiguration config)
        {
            configuration = config;
            SetupHttpClient(httpClient);
            InitializeSettings();
            InitializeSemanticKernel();
        }
        public OpenAIConfiguration? OpenAIConfig { get; set; }
        public Kernel Kernel { get; set; }
        public ILoggerFactory? SKLoggerFactory { get; set; }

        private void InitializeSettings()
        {
            // Initialize any other properties here
            OpenAIConfig = new OpenAIConfiguration();
            configuration.GetSection(nameof(OpenAIConfiguration)).Bind(OpenAIConfig);
        }

        private void SetupHttpClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.httpClient.Timeout = TimeSpan.FromMinutes(60);
        }

        private void InitializeSemanticKernel()
        {
            //Set Logger
            this.SKLoggerFactory = LoggerFactory.Create(builder =>
            {
                // Add OpenTelemetry as a logging provider
                // builder.AddOpenTelemetry(options =>
                // {
                //     options.AddAzureMonitorLogExporter(options =>
                //         options.ConnectionString = configuration["ApplicationInsights:ConnectionString"]);
                //     options.IncludeFormattedMessage = true;
                // });
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var kernelBuilder = Kernel.CreateBuilder()
                       .AddAzureOpenAIChatCompletion(deploymentName: OpenAIConfig.ModelName,
                                                     endpoint: OpenAIConfig.EndPoint,
                                                     apiKey: OpenAIConfig.Key,
                                                     httpClient: this.httpClient)
                        
                       .AddAzureOpenAITextEmbeddingGeneration(deploymentName: OpenAIConfig.EmbeddingModelName,
                                                              endpoint: OpenAIConfig.EndPoint,
                                                              apiKey: OpenAIConfig.Key,
                                                              httpClient: this.httpClient);
            
            kernelBuilder.Services.AddSingleton(this.SKLoggerFactory);
            this.Kernel = kernelBuilder.Build();
#pragma warning restore SKEXP0011 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


            //Tracing
            //using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            //    .AddAzureMonitorTraceExporter(options =>
            //    {
            //        options.ConnectionString = configuration["ApplicationInsights:ConnectionString"];
            //    })
            //.Build();


        }

    }
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public class PromptFilter : IPromptRenderFilter
    {
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            var renderedPrompt = context.RenderedPrompt;

            //Perform some actions after the prompt is rendered
            await next(context);
        }
    }
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    public class  OpenAIConfiguration
    {
        public string? EndPoint { get; set; }
        public string? Key { get; set; }
        public string? ModelName { get; set; }
        public string? ModelId { get; set; }
        public string? EmbeddingModelName { get; set; }
    }
}
