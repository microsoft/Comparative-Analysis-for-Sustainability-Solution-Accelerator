// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using CFS.SK.Sustainability.AI.Storage;
using CFS.SK.Sustainability.AI.Storage.Components;
using System.Collections.Specialized;
using CFS.SK.Sustainability.AI.Storage.Benchmark.Entities;

namespace CFS.SK.Sustainability.AI.Storage.Benchmark
{
    public class BenchmarkJobRepository : MongoEntntyCollectionBase<BenchmarkJob, Guid>
    {
        public BenchmarkJobRepository(string DataConnectionString, string CollectionName) : base(DataConnectionString, CollectionName)
        {
        }
        public async Task<BenchmarkJob> Register(BenchmarkJob benchmarkJob)
        {
            return await this.EntityCollection.AddAsync(benchmarkJob);
        }

        public async Task<IEnumerable<BenchmarkJob>> GetAllDocuments()
        {
            var results = await this.EntityCollection.GetAllAsync();
            //return collection after ordering by ProcessStartTime time in descending order
            return results.OrderByDescending(x => x.ProcessStartTime);
        }

        public async Task<BenchmarkJob> Update(BenchmarkJob document)
        {
            return await this.EntityCollection.SaveAsync(document);
        }

        public async Task Delete(Guid id)
        {
            await this.EntityCollection.DeleteAsync(id);
        }

        async public Task<IEnumerable<BenchmarkJob>> FindByJobName(string JobName)
        {
            return await this.EntityCollection.FindAllAsync(
                new GenericSpecification<BenchmarkJob>(x => x.JobName == JobName));
        }

        async public Task<BenchmarkJob> Find(Guid Id)
        {
            return await this.EntityCollection.GetAsync(Id);
        }


        async public Task<BenchmarkJob> FindByBenchmarkJobId(Guid JobId)
        {
            return await this.EntityCollection.FindAsync(
                new GenericSpecification<BenchmarkJob>(x => x.JobId == JobId));
        }

        async public Task<BenchmarkJob> UpdateProcessStatus(Guid JobId, ProcessStatus JobStatus)
        {
            var benchmarkJob = await this.EntityCollection.FindAsync(
                new GenericSpecification<BenchmarkJob>(x => x.JobId == JobId));
            benchmarkJob.ProcessStatus = JobStatus;
            benchmarkJob.ProcessedTime = DateTime.UtcNow;

            return await this.EntityCollection.SaveAsync(benchmarkJob);
        }
    }
}
