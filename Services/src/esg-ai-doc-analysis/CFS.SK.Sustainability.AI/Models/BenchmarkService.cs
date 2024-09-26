// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CFS.SK.Abstracts.Model;
using Microsoft.KernelMemory;

namespace CFS.SK.Sustainability.AI.Models
{
    public class BenchmarkServiceRequest : IParameter
    {
        public Guid JobId { get; set; }
        public required string JobName { get; set; }
        public required string[] DocumentIds { get; set; }
        public required string JobOwner {  get; set; }
        public required string DisclosureNumber { get; set; }
        public required string DisclosureName { get; set; }
        public required string DisclosureRequirement { get; set; }
        public required string DisclosureRequirementDetail { get; set; }
        public required string DisclosureAnnex { get; set; }
    }

    public class BenchmarkServiceExecutionRequest : IParameter
    {
        public required string DisclosureNumber { get; set; }
        public required string DisclosureName { get; set; }
        public required string DisclosureRequirement { get; set; }
        public required string DisclosureRequirementDetail { get; set; }
        public required string DisclosureAnnex { get; set; }
        public required string Disclosures { get; set; }
    }

    public class BenchmarkServiceExecutionResponse : IReturnValue
    {
        public required BenchmarkResult Response { get; set; }
        public required string ResultFile { get; set; }
        public required string ResultPdfFile { get; set; }
        public required string ResultMetaDataFile { get; set; }
        public required List<MemoryAnswer> MetaData { get; set; }
    }

    public class BenchmarkReportGenerationServiceExecutionResponse :  IReturnValue
    {
        public required string Response { get; set; }
        public required string ResultFile { get; set; }
        public required string ResultHtmlFile { get; set; }
        public required string ResultPdfFIle { get; set; }
        public required string ResultMetaDataFile { get; set; }
        public required List<MemoryAnswer> MetaData { get; set; }
    }
}
