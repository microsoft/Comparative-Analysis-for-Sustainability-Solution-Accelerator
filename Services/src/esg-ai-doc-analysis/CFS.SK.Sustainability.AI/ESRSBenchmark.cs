// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Amazon.Auth.AccessControlPolicy;
using CFS.SK.Abstracts;
using CFS.SK.Abstracts.Model;
using CFS.SK.Sustainability.AI.Models;
using CFS.SK.Sustainability.AI.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

namespace CFS.SK.Sustainability.AI
{
    public class ESRSBenchmark : SemanticKernelLogicBase<ESRSBenchmark>
    {
        public ESRSBenchmark(ApplicationContext appContext, ILogger<ESRSBenchmark> logger) : base(appContext, logger)
        {
        }

        public async override Task<string?> ExecuteAndReturnString(IParameter benchmarkServiceExecutionRqeuest)
        {
            var param_benchmarkServiceExecutionRqeuest = (BenchmarkServiceExecutionRequest)benchmarkServiceExecutionRqeuest;

            var pluginDirectoryPath = Utils.Plugin.GetPluginDirectoryPath("plugins", "CSRDPlugin");

            if (this._appContext.Kernel.Plugins.Where(x => x.Name == "CSRDPlugin").SingleOrDefault() == null)
            {
                this._appContext.Kernel.ImportPluginFromPromptDirectory(pluginDirectory: pluginDirectoryPath);
            }

            StringBuilder disclosureBuilder = new StringBuilder();
            foreach (var item in param_benchmarkServiceExecutionRqeuest.Disclosures)
            {
                //disclosures.Add(item);
                disclosureBuilder.Append(item);
                disclosureBuilder.Append("\n");
            }


            var csrdPlugin = this._appContext.Kernel.Plugins["CSRDPlugin"];
            var result = await this._appContext.Kernel.InvokeAsync(csrdPlugin["AnalyzeReporter"],
                                                      new KernelArguments
                                                      {
                                                          ["disclosure_number"] = param_benchmarkServiceExecutionRqeuest.DisclosureNumber,
                                                          ["disclosure_name"] = param_benchmarkServiceExecutionRqeuest.DisclosureName,
                                                          ["disclosure_requirement"] = param_benchmarkServiceExecutionRqeuest.DisclosureRequirement,
                                                          ["disclosure_annex"] = param_benchmarkServiceExecutionRqeuest.DisclosureAnnex,
                                                          ["disclosures"] = disclosureBuilder.ToString(),
                                                      });

            return result.GetValue<string>();
        }

        public class Disclosure
        {
            public required string disclosure { get; set; }
        }
    }
}
