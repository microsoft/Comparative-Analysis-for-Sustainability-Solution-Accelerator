// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using CFS.SK.Sustainability.AI.Storage.Components;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace CFS.SK.Sustainability.AI.Storage.GapAnalysis.Entities
{
    public class GapAnalysisJob : CosmosDBEntityBase
    {
        public Guid JobId { get; set; }
        public GapAnalysisDocumentInformation? Document { get; set; }
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
        public string Result { get; set; }
        public string MetaData { get; set; }
        public string MetaDataFileUrl { get; set; }
        public string ResultFileUrl { get; set; }
        public string ResultFileHtmlUrl { get; set; }
        public string ResultPdfFileUrl { get; set; }
    }

    public class GapAnalysisDocumentInformation
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
