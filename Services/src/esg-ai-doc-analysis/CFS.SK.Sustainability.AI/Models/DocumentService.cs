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
    public class RegisterDocumentParameter : IParameter
    {
        public RegisterDocumentParameter()
        {
        }
        public required List<KeyValuePair<string, string>> Tags { get; set; }
    }

    public class RegisterDocumentFromBlobStorageServiceRequest : RegisterDocumentParameter
    {
        public RegisterDocumentFromBlobStorageServiceRequest() : base() { }

        public required string ContainerName { get; set; }
        public required string FileLocation { get; set; }
    }

    public class RegisterDocumentFromFileUrlServiceRequest : RegisterDocumentParameter
    {
        public RegisterDocumentFromFileUrlServiceRequest() : base() { }

        public required string FileUrl { get; set; }
        public required string FileName { get; set; }
    }
}
