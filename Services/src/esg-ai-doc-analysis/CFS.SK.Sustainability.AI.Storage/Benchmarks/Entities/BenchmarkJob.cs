// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using CFS.SK.Sustainability.AI.Storage.Components;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace CFS.SK.Sustainability.AI.Storage.Benchmark.Entities
{
    public class BenchmarkJob : CosmosDBEntityBase
    {
        public Guid JobId { get; set; }
        public string JobName { get; set; }
        public List<BenchmarkDocumentInformation> Documents { get; set; }

        public string Owner { get; set; }
        public DateTime ProcessStartTime { get; set; }
        public DateTime ProcessedTime { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public ProcessStatus ProcessStatus { get; set; }
        public string DisclosureNumber { get; set; }
        public string DisclosureName { get; set; }
        public string DisclosureRequirement { get; set; }
        public string DisclosureAnnex { get; set; }
        public string BechmarkResult { get; set; }
        public string BenchmarkMetaDataFileUrl { get; set; }
        public string BenchmarkResultFileUrl { get; set; }
        public string BenchmarkResultHtmlFileUrl { get; set; }
        public string BenchmarkResultPdfFileUrl { get; set; }
    }

    public class BenchmarkDocumentInformation
    {
        public string DocumentId { get; set; }
        public string DocumentName { get; set; }
    }

    public enum ProcessStatus
    {
        [Description("In Progress")]
        InProgress = 0,
        [Description("Completed")]
        Completed = 1
    }
}
