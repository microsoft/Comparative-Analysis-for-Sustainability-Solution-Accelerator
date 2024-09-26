// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CFS.SK.Abstracts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFS.SK.Sustainability.AI.Models
{
    public class ESRSDisclosureRetrieverServiceRequest : IParameter
    {
        public required string DisclosureName {  get; set; }
        public required string DisclosureNumber { get; set; }
        public required string DisclosureRequirement { get; set; }
        public required string DisclosureRequirementDetail { get; set; }
        public required string AnnexDescription { get; set; }
        public required string DocumentId { get; set; }
        public int SizeOfChars { get; set; }

    }
}
