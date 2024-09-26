// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CFS.SK.Abstracts.Model;
using CFS.SK.Sustainability.AI.Storage.Document.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFS.SK.Sustainability.AI.Models
{
    public class DocumentServiceResult : IReturnValue
    {
        public string? DocumentId { get; set; }
        public string? FileDisplayName { get; set; }
        public string? FileLocation { get; set; }
        public string? Title { get; set; }
        public string? Year { get; set; }
        public string? Type { get; set; }
        public string? Classification { get; set; }
        public DateTime UploadedTime { get; set; }
        public DateTime latestProcessedTime { get; set; }
        public required string Status { get; set; }
    
        static public DocumentServiceResult MapDataEntities(Document document)
        {
            return  new DocumentServiceResult
            {
                DocumentId = document.DocumentId,
                FileDisplayName = document.FileDisplayName,
                FileLocation = document.FileLocation,
                Title = document.Tags == null ? "" : GetTagValue(document.Tags, "Title"),
                Year = document.Tags == null? "" : GetTagValue(document.Tags, "Year"),
                Type = document.Tags == null ? "" : GetTagValue(document.Tags, "Type"),
                Classification = document.Tags == null ? "" : GetTagValue(document.Tags, "Classification"),
                UploadedTime = document.UploadedTime,
                latestProcessedTime = document.latestProcessedTime,
                Status = document.Status
            };
        }

        static private string GetTagValue(List<KeyValuePair<string, string>> tags, string key)
        {
            if (tags == null)
            {
                return "";
            }
            var tag = tags.FirstOrDefault(t => t.Key == key);
            //if tag is not found, return empty string
            if (tag.Key == null)
            {
                return "";
            }
                    
            return tag.Value;
        }
    }


}
