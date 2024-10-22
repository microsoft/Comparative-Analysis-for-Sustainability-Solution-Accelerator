// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

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
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace CFS.SK.Sustainability.AI
{
    public class ESRSBenchmarkReportGenerator : SemanticKernelLogicBase<ESRSBenchmarkReportGenerator>
    {
        public ESRSBenchmarkReportGenerator(ApplicationContext appContext, ILogger<ESRSBenchmarkReportGenerator> logger) : base(appContext, logger)
        {
            this._logger = logger;
        }

        public async override Task<string?> ExecuteAndReturnString(IParameter benchmarkServiceExecutionRqeuest)
        {
            var param_benchmarkServiceExecutionRqeuest = (BenchmarkServiceExecutionRequest)benchmarkServiceExecutionRqeuest;

            var pluginDirectoryPath = Utils.Plugin.GetPluginDirectoryPath("plugins", "CSRDPlugin");

            if (this._appContext.Kernel.Plugins.Where(x => x.Name == "CSRDPlugin").SingleOrDefault() == null)
            {
                this._appContext.Kernel.ImportPluginFromPromptDirectory(pluginDirectory: pluginDirectoryPath);
            }

            var csrdPlugin = this._appContext.Kernel.Plugins["CSRDPlugin"];
     
            var result_stream = this._appContext.Kernel.InvokeStreamingAsync<string>(csrdPlugin["BenchmarkReportGenerator"],
                                                      new KernelArguments
                                                      {
                                                          ["disclosure_number"] = param_benchmarkServiceExecutionRqeuest.DisclosureNumber,
                                                          ["disclosure_name"] = param_benchmarkServiceExecutionRqeuest.DisclosureName,
                                                          ["disclosure_requirement"] = param_benchmarkServiceExecutionRqeuest.DisclosureRequirement,
                                                          ["disclosure_requirement_detail"] = param_benchmarkServiceExecutionRqeuest.DisclosureRequirementDetail,
                                                          ["disclosure_annex"] = param_benchmarkServiceExecutionRqeuest.DisclosureAnnex,
                                                          ["disclosures"] = param_benchmarkServiceExecutionRqeuest.Disclosures,
                                                      });

            //Getting the result from the stream and generate string
            var sb_Result = new StringBuilder();
            await foreach (var item in result_stream)
            {
                this._logger.LogInformation(item);
                sb_Result.Append(item);
            }

            //return result.GetValue<string>();
            return sb_Result.ToString();
        }

        public class Disclosure
        {
            public string disclosure { get; set; }
        }
    }
}
