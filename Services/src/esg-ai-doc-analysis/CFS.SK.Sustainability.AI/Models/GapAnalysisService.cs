// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CFS.SK.Abstracts.Model;
using Microsoft.KernelMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFS.SK.Sustainability.AI.Models
{
    public class GapAnalysisServiceRequest : IParameter
    {
        public Guid JobId { get; set; }
        public required string JobOwner { get; set; }
        public required string DisclosureNumber { get; set; }
        public required string DisclosureName { get; set; }
        public required string DisclosureRequirement { get; set; }
        public required string DisclosureRequirementDetail { get; set; }
        public required string DisclosureAnnex { get; set; }
        public required string DocumentId { get; set; }
        public string ServiceUrl { get; set; }
    }

    public class GapAnalysisServiceExecutionRequest : IParameter
    {
        public required string DisclosureNumber { get; set; }
        public required string DisclosureName { get; set; }
        public required string DisclosureRequirement { get; set; }
        public required string DisclosureRequirementDetail { get; set; }
        public required string DisclosureAnnex { get; set; }
        public required string Disclosure { get; set; }
    }

    public class GapAnalysisServiceExecutionResponse : IParameter
    {
        public required GapAnalysisResult Result { get; set; }
    }

    public class GapAnalysisResult
    {
        public required string DisclosureNumber { get; set; }
        public required string DisclosureName { get; set; }
        public required string DisclosureRequirement { get; set; }
        public required string DisclosureRequirementDetail { get; set; }
        public required string Disclosure { get; set; }
        public required string GoodPart { get; set; }
        public required string MissingPart { get; set; }
        public required string Rephrase { get; set; }
        public required string ActionPlan { get; set; }
        public required string Score { get; set; }
    }

    public class GapAnalysisReportGenerationServiceExecutionResponse : IReturnValue
    {
        public required string Response { get; set; }
        public required string ResultFile { get; set; }
        public required string ResultHtmlFile { get; set; }
        public required string ResultPdfFile { get; set; }
        public required string ResultMetaDataFile { get; set; }
        public required MemoryAnswer MetaData { get; set; }
    }
}
