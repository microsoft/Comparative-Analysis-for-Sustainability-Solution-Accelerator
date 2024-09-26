// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using CFS.SK.Sustainability.AI.Storage;
using CFS.SK.Sustainability.AI.Storage.Components;
using System.Collections.Specialized;
using CFS.SK.Sustainability.AI.Storage.GapAnalysis.Entities;

namespace CFS.SK.Sustainability.AI.Storage.GapAnalysis
{
    public class GapAnalysisJobRepository : MongoEntntyCollectionBase<GapAnalysisJob, Guid>
    {
        public GapAnalysisJobRepository(string DataConnectionString, string CollectionName) : base(DataConnectionString, CollectionName)
        {
        }

        public async Task<GapAnalysisJob> Register(GapAnalysisJob gapAnalysisJob)
        {
            return await this.EntityCollection.AddAsync(gapAnalysisJob);
        }

        public async Task<IEnumerable<GapAnalysisJob>> GetAllDocuments()
        {
            var results = await this.EntityCollection.GetAllAsync();
            //return collection after ordering by ProcessStartTime time in descending order
            return results.OrderByDescending(x => x.ProcessStartTime);
        }

        public async Task<GapAnalysisJob> Update(GapAnalysisJob gapAnalysisJob)
        {
            return await this.EntityCollection.SaveAsync(gapAnalysisJob);
        }

        public async Task Delete(Guid id)
        {
            await this.EntityCollection.DeleteAsync(id);
        }

        async public Task<GapAnalysisJob> Find(Guid Id)
        {
            return await this.EntityCollection.GetAsync(Id);
        }


        async public Task<GapAnalysisJob> FindByGapAnalysisJobId(Guid JobId)
        {
            return await this.EntityCollection.FindAsync(
                new GenericSpecification<GapAnalysisJob>(x => x.JobId == JobId));
        }

        async public Task<GapAnalysisJob> UpdateProcessStatus(Guid JobId, ProcessStatus JobStatus)
        {
            var gapAnalysisJob = await this.EntityCollection.FindAsync(
                new GenericSpecification<GapAnalysisJob>(x => x.JobId == JobId));
            gapAnalysisJob.ProcessStatus = JobStatus;
            gapAnalysisJob.ProcessedTime = DateTime.UtcNow;

            return await this.EntityCollection.SaveAsync(gapAnalysisJob);
        }
    }
}
