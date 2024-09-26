// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using CFS.SK.Abstracts.Model;

namespace CFS.SK.Sustainability.AI.Models
{
    public class BenchmarkResult : IReturnValue
    {
        public required string DisclosureNumber { get; set; }
        public required string DisclosureName { get; set; }
        public required string DisclosureRequirement { get; set; }
        public required string[] Disclosures { get; set; }
        public required string Similarities { get; set; }
        public required string Differences { get; set; }
        public required string Opportunities { get; set; }
        public required string ActionPlan { get; set; }
    }
}
