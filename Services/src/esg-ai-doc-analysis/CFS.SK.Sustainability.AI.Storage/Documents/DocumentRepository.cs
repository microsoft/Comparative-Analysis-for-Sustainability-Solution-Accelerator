// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using CFS.SK.Sustainability.AI.Storage.Document;
using CFS.SK.Sustainability.AI.Storage.Components;
using System.Collections.Specialized;

namespace CFS.SK.Sustainability.AI.Storage.Document
{
    public class DocumentRepository : MongoEntntyCollectionBase<Entities.Document, Guid>
    {
        public DocumentRepository(string DataConnectionString, string CollectionName) : base(DataConnectionString, CollectionName)
        {
        }

        public async Task<Entities.Document> Register(string DocumentId, string FileDisplayName, string FileLocation, List<KeyValuePair<string, string>> Tags)
        {
            return await this.EntityCollection.AddAsync(
                    new Entities.Document()
                    {
                        DocumentId = DocumentId,
                        FileDisplayName = FileDisplayName,
                        FileLocation = FileLocation,
                        Tags = Tags
                    }
                );
        }
        public async Task<Entities.Document> Register(Entities.Document document)
        {
            return await this.EntityCollection.AddAsync(document);
        }

        public async Task<IEnumerable<Entities.Document>> GetAllDocuments()
        {
            var results = await this.EntityCollection.GetAllAsync();
            //return collection after ordering by latest processed time in descending order
            return results.OrderByDescending(x => x.UploadedTime);
            
        }

        public async Task<Entities.Document> Update(Entities.Document document)
        {
            return await this.EntityCollection.SaveAsync(document);
        }

        public async Task Delete(Guid id)
        {
            await this.EntityCollection.DeleteAsync(id);
        }

        async public Task<IEnumerable<Entities.Document>> FindByTags(string[] tags)
        {
            return await this.EntityCollection.FindAllAsync(
                new GenericSpecification<Entities.Document>(x => x.Tags.Any(tag => tags.Contains(tag.Value))));
        }

        async public Task<Entities.Document> Find(Guid id)
        {
            return await this.EntityCollection.GetAsync(id);
        }


        async public Task<Entities.Document> FindByDocumentId(string documentId)
        {
            return await this.EntityCollection.FindAsync(
                new GenericSpecification<Entities.Document>(x => x.DocumentId == documentId));
        }

        async public Task<Entities.Document> UpdateProcessStatus(string documentId, string status)
        {
            var doc = await this.EntityCollection.FindAsync(
                new GenericSpecification<Entities.Document>(x => x.DocumentId == documentId));
            doc.Status = status;
            doc.latestProcessedTime = DateTime.UtcNow;

            return await this.EntityCollection.SaveAsync(doc);
        }
    }
}
