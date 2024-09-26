// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using CFS.SK.Abstracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using Microsoft.KernelMemory;
using System.Runtime.CompilerServices;
using ZstdSharp;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using CFS.SK.Sustainability.AI.Utils;
using CFS.SK.Sustainability.AI.Models;
using CFS.SK.Abstracts.Model;
using CFS.SK.Sustainability.AI.plugins.CSRDPlugin;


namespace CFS.SK.Sustainability.AI
{
    public class ESRSDisclosureRetriever : SemanticKernelLogicBase<ESRSDisclosureRetriever>
    {
        IKernelMemory memory;
        public ESRSDisclosureRetriever(ApplicationContext appContext, IKernelMemory memoryClient, ILogger<ESRSDisclosureRetriever> logger) : base(appContext, logger)
        {
            this.memory = memoryClient;
        }

        public override async Task<MemoryAnswer?> ExecuteAndReturnResultAsMemoryAnswer(IParameter serviceRequest)
        {
            var param_serviceRequest = (ESRSDisclosureRetrieverServiceRequest)serviceRequest;
            
            var disclosure_name = param_serviceRequest.DisclosureName;
            var disclosure_no = param_serviceRequest.DisclosureNumber;
            var disclosure_requirement = param_serviceRequest.DisclosureRequirement;
            var disclosure_requirement_detail = param_serviceRequest.DisclosureRequirementDetail;
            disclosure_requirement = disclosure_requirement?.TrimEnd(new char[] { '\r', '\n' });
            var annex_description = param_serviceRequest.AnnexDescription;
            var document_id = param_serviceRequest.DocumentId;
            var sizeofChars = param_serviceRequest.SizeOfChars;

            if (!Plugin.IsRegistered(this._appContext.Kernel, "DisclosureRetrieverPlugin"))
            {
                this._appContext.Kernel.Plugins.AddFromType<DisclosureRetrieverPlugin>();
            }

            KernelPlugin funcPlugin = this._appContext.Kernel.Plugins["DisclosureRetrieverPlugin"];

            var result = await this._appContext.Kernel.InvokeAsync<MemoryAnswer>(funcPlugin["GetRelevantContent"],
                                                                    new KernelArguments
                                                                    {
                                                                        ["memory"] = this.memory,
                                                                        ["documentId"] = document_id,
                                                                        ["disclosureNo"] = disclosure_no,
                                                                        ["disclosureName"] = disclosure_name,
                                                                        ["disclosure_requirement"] = disclosure_requirement,
                                                                        ["disclosure_requirement_detail"] = disclosure_requirement_detail,
                                                                        ["annex"] = annex_description,
                                                                        ["sizeOfChars"] = sizeofChars
                                                                    });
            return result;
         
        }
    }
}
