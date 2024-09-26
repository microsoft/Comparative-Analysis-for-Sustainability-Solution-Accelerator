// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CFS.SK.Abstracts.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFS.SK.Sustainability.AI.Models
{
    public class RegisterDocumentFileServiceRequest : RegisterDocumentParameter
    {
        public required IFormFile FileBinary { get; set; }
    }
}
