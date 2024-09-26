// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace CFS.SK.Sustainability.AI.Host
{
    public class Startup
    {
        public static WebApplicationBuilder CreateHostBuilder(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration
                .AddJsonFile("appsettings.dev.json", optional: true, reloadOnChange: true);
               
            builder.Logging.SetMinimumLevel(LogLevel.Trace);
            
            return builder;
        }
    }
}
