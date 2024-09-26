// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Azure.Storage.Queues;
using CFS.SK.Sustainability.AI.Models;
using CFS.SK.Sustainability.AI.Services.Queue;
using CFS.SK.Sustainability.AI.Services.Queue.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory.Orchestration.AzureQueues;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UglyToad.PdfPig.Graphics.Operations.TextPositioning;

namespace CFS.SK.Sustainability.AI.Services
{
    //define Worker Service Class hosted by ASP.net core
    public class QueueServiceHost : BackgroundService
    {
        private IConfiguration configuration;
        private Dictionary<string, IQueue> _queues = new(StringComparer.InvariantCultureIgnoreCase);
        private ESRSGapAnalysisManager eSRSGapAnalyzer;
        private BenchmarkJobManager benchmarkJobManager;
        private ILogger<AzureStorageQueueService> _logger { get; }
        public QueueServiceHost(IConfiguration config, 
                                ILoggerFactory? logger, 
                                ESRSGapAnalysisManager eSRSGapAnalyzer,
                                BenchmarkJobManager benchmarkJobManager)
        {
            this._logger = logger.CreateLogger<AzureStorageQueueService>();
            this.eSRSGapAnalyzer = eSRSGapAnalyzer;
            this.benchmarkJobManager = benchmarkJobManager;

            configuration = config;
            _queues.Add("GapAnalysis", new AzureStorageQueueService(config["ConnectionStrings:BlobStorage"], "GapAnalysis", this._logger));
            _queues.Add("Benchmark", new AzureStorageQueueService(config["ConnectionStrings:BlobStorage"], "Benchmark", this._logger));

            initialize_QueueService();
        }

        async public override Task<Task> StartAsync(CancellationToken cancellationToken)
        {
            await SetupMessageHandlersForQueues();
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            foreach(var queue in _queues)
            {
                queue.Value.Dispose();
            }

            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
            }
        }

        private async Task<bool> SetupMessageHandlersForQueues()
        {
            _queues["GapAnalysis"].OnDequeue(async message =>
            {
                // Deserialize the message as an Object
                // If Object is not valid or null return false
                // If Object is valid, process the message and return true
                try
                {
                    var gapAnalysisServiceRequest = JsonSerializer.Deserialize<GapAnalysisServiceRequest>(message);
                    if (gapAnalysisServiceRequest == null)
                    {
                        return false;
                    }else
                    {
                        // Invoke Process
                        _ = await eSRSGapAnalyzer.ExecuteAndReturnResult(gapAnalysisServiceRequest);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "Error processing Gap Analysis Request");
                    return false;
                }
            });

            _queues["Benchmark"].OnDequeue(async message =>
            {
                // Deserialize the message as an Object
                // If Object is not valid or null return false
                // If Object is valid, process the message and return true
                try
                {
                    var benchmarkServiceRequest = JsonSerializer.Deserialize<BenchmarkServiceRequest>(message);
                    if (benchmarkServiceRequest == null)
                    {
                        return false;
                    }
                    else
                    {
                        // Invoke Process
                        _ = await benchmarkJobManager.ExecuteAndReturnResult(benchmarkServiceRequest);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "Error processing Benchmark Request");
                    return false;
                };
            });

            return true;
        }

        private void initialize_QueueService()
        {
            foreach (var queue in _queues)
            {
                var options = QueueOptions.PubSub;
                options.DequeueEnabled = true;
                _ = queue.Value.ConnectToQueueAsync(queue.Key, options).Result;
            }
        }
    }
}
