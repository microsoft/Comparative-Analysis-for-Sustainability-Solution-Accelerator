// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CFS.SK.Abstracts.Model;

namespace CFS.SK.Sustainability.AI.Models
{
    public class AskAboutDocumentServiceRequest : IParameter
    {
        public required string[] DocumentIds { get; set; }
        public required string Question { get; set; }
    }
}
