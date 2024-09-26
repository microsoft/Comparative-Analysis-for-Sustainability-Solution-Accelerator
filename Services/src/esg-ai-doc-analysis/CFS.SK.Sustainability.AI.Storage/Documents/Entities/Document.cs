// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CFS.SK.Sustainability.AI.Storage.Components;

namespace CFS.SK.Sustainability.AI.Storage.Document.Entities
{
    public class Document : CosmosDBEntityBase
    {
        public string DocumentId { get; set; }
        public string FileDisplayName  { get; set; }
        public string FileLocation { get; set; }
        public List<KeyValuePair<string,string>>? Tags { get; set; }
        public DateTime UploadedTime { get; set; }
        public DateTime latestProcessedTime { get; set; }
        public string Status { get; set; }
    }
}
