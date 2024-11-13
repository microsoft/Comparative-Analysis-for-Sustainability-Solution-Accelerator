// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Diagnostics;
using Azure.Storage.Blobs;
using CFS.SK.Abstracts;
using CFS.SK.Abstracts.Model;
using CFS.SK.Sustainability.AI.Models;
using CFS.SK.Sustainability.AI.Utils;
using CFS.SK.Sustainability.AI.Storage.Benchmark;
using CFS.SK.Sustainability.AI.Storage.Benchmark.Entities;
using CFS.SK.Sustainability.AI.Storage.Document;
using CFS.SK.Sustainability.AI.Storage.Document.Entities;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using MongoDB.Driver.Core.Configuration;
using Azure.Identity;

namespace CFS.SK.Sustainability.AI
{
    
    public class BenchmarkJobManager : SemanticKernelLogicBase<BenchmarkJobManager>
    {
        const string FileDownLoadService = "/ESRS/Results";
        private readonly ILogger<BenchmarkJobManager> logger;
        private readonly BenchmarkJobRepository benchmarkjobRepo;
        private readonly DocumentRepository documentRepo;
        private readonly IConfiguration config;
        private readonly ESRSDisclosureRetriever esrsDisclosureRetriever;
        private readonly ESRSBenchmarkReportGenerator esrsBenchmarkReportGenerator;

        public BenchmarkJobManager(ApplicationContext appContext, ESRSBenchmarkReportGenerator esrsBenchmarkReportGenerator, ESRSDisclosureRetriever esrsDisclosureRetriever, BenchmarkJobRepository benchmarkjobRepo, DocumentRepository documentRepo, ILogger<BenchmarkJobManager> logger, IConfiguration config) : base(appContext, logger)
        {
            this.logger = logger;
            this.benchmarkjobRepo = benchmarkjobRepo;
            this.config = config;
            this.esrsDisclosureRetriever = esrsDisclosureRetriever;
            this.documentRepo = documentRepo;
            this.esrsBenchmarkReportGenerator = esrsBenchmarkReportGenerator;
        }

        public async Task<IEnumerable<BenchmarkJob>> GetBenchmarkJobs()
        {
            return await this.benchmarkjobRepo.GetAllDocuments();
        }

        public async Task<bool> DeleteBenchmarkJob(Guid id)
        {
            await this.benchmarkjobRepo.Delete(id);
            return true;
        }

        public async Task<bool> DeleteBenchmarkJobWithBenchmarkId(Guid JobId)
        {
            var benchmarkJob = await this.benchmarkjobRepo.FindByBenchmarkJobId(JobId);
            if (benchmarkJob != null)
            {
                await DeleteBenchmarkJob(benchmarkJob.id);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<BenchmarkJob> GetBenchmarkJobByJobId(Guid benchmarkJobId)
        {
            return await this.benchmarkjobRepo.FindByBenchmarkJobId(benchmarkJobId);
        }

        public async Task<IEnumerable<BenchmarkJob>> GetBenchmarkJobByJobName(string jobName)
        {
            return await this.benchmarkjobRepo.FindByJobName(jobName);
        }

        public override async Task<IReturnValue?> ExecuteAndReturnResult(IParameter benchmarkServiceRequest)
        {
            if (benchmarkServiceRequest == null) { throw new ArgumentNullException(nameof(BenchmarkServiceRequest)); }
            if (this.esrsDisclosureRetriever == null) { throw new Exception("ESRSDiclosureRetriever Service should be registered"); }
            if (this.esrsBenchmarkReportGenerator == null) { throw new Exception("ESRSBenchmark Reporting Generator Service should be registered"); }

            var param_benchmarkServiceRequest = (BenchmarkServiceRequest)benchmarkServiceRequest;

            //Register Job into Cosmos DB
            var registeredJob = await RegisterJob(param_benchmarkServiceRequest);
            //Get JobId
            var jobId = registeredJob.JobId;

            //Get Disclosures from four Documents
            var disclosureAnswers = new List<MemoryAnswer>();
            var disclosures = new List<string>();

            //Get Multiple disclosure answer from each documents
            foreach (var documentId in param_benchmarkServiceRequest.DocumentIds)
            {
                var result = await this.esrsDisclosureRetriever.ExecuteAndReturnResultAsMemoryAnswer(new ESRSDisclosureRetrieverServiceRequest()
                {
                    DocumentId = documentId,
                    DisclosureName = param_benchmarkServiceRequest.DisclosureName,
                    DisclosureNumber = param_benchmarkServiceRequest.DisclosureNumber,
                    DisclosureRequirement = param_benchmarkServiceRequest.DisclosureRequirement,
                    DisclosureRequirementDetail = param_benchmarkServiceRequest.DisclosureRequirementDetail,
                    AnnexDescription = param_benchmarkServiceRequest.DisclosureAnnex,
                    SizeOfChars = 8000 //2500
                });

                if (result != null)
                {
                    disclosureAnswers.Add(result);
                    disclosures.Add(result.Result);
                }
            }

            //parepare disclosures array list as a string
            StringBuilder sb = new StringBuilder();
            foreach (var item in disclosures)
            {
                sb.Append(item.ToString());
                sb.Append('\n');
                sb.Append('\n');
            }

            //Prepare Benchmark Service invocation.
            var benchmarkServiceExecutionRequest = new BenchmarkServiceExecutionRequest()
            {
                DisclosureNumber = param_benchmarkServiceRequest.DisclosureNumber,
                DisclosureName = param_benchmarkServiceRequest.DisclosureName,
                DisclosureRequirement = param_benchmarkServiceRequest.DisclosureRequirement,
                DisclosureRequirementDetail = param_benchmarkServiceRequest.DisclosureRequirementDetail,
                DisclosureAnnex = param_benchmarkServiceRequest.DisclosureAnnex,
                Disclosures = sb.ToString() //disclosures.ToArray<string>()
            };

            var benchmarkResult = await this.esrsBenchmarkReportGenerator.ExecuteAndReturnString(benchmarkServiceExecutionRequest);

            if (benchmarkResult == null)
                throw new Exception("Benchmark Report Generation Failed");

            //Save result as a files
            //Save Response as a [BenchmarkName]-[JobId].md file
            //Save MetaData as a [BenchmarkName]-[JobId]-meta.json file

            //Get BlobConnectionString
            var blobConnectionString = this.config["ConnectionStrings:BlobStorage"];
            //Get BlobServiceClient from ConnectionString
            var blobServiceClient = StorageAccessUtil.GetBlobClientFromConnectionString(blobConnectionString);
            //Create ContainerClient
            var blobContainerClient = blobServiceClient.GetBlobContainerClient("results");
            await blobContainerClient.CreateIfNotExistsAsync();

            //Make File Name
            var fileName = $"{param_benchmarkServiceRequest.JobName}-{param_benchmarkServiceRequest.DisclosureNumber}-{jobId}";

            //Create BlobClient
            var mdFileName = $"{fileName}.md";
            var blobClient = blobContainerClient.GetBlobClient(mdFileName);
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(benchmarkResult)))
            {
                await blobClient.UploadAsync(stream, true);
            };

            //Client SAS Token Url for the file
            //var sasUri_resultFile = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddYears(100));
            var sasUri_resultFile = $"{param_benchmarkServiceRequest.ServiceUrl}{FileDownLoadService}/{mdFileName}";

            //Create BlobClient
            var htmlFileName = $"{fileName}.html";
            blobClient = blobContainerClient.GetBlobClient(htmlFileName);

            byte[] byteArray_html = MarkdownHtmlConverter.Convert(benchmarkResult);
            using (Stream stream = new MemoryStream(byteArray_html))
            {
                await blobClient.UploadAsync(stream, true);
            };

            //Client SAS Token Url for the file
            //var sasUri_resultHtml = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddYears(100));
            var sasUri_resultHtml = $"{param_benchmarkServiceRequest.ServiceUrl}{FileDownLoadService}/{htmlFileName}";

            //Get Current Path for Save Converted Html
            var currentPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //Get Only Path
            currentPath = System.IO.Path.GetDirectoryName(currentPath);

            var htmlFilePath = System.IO.Path.Combine(currentPath, htmlFileName);
            //Save Converted Html
            await System.IO.File.WriteAllBytesAsync(htmlFilePath, byteArray_html);

            //Convert HTML to PDF
            //Need to extra work for convert html to pdf
            var pdfFileName = $"{fileName}.pdf";
            var pdfFilePath = System.IO.Path.Combine(currentPath, pdfFileName);

            HtmlPdfConverter.Convert(htmlFilePath, pdfFilePath);

            //if Pdf file is exist, upload it to the blob storage
            string sasUri_resultPdf;

            if (System.IO.File.Exists(pdfFilePath))
            {
                //Create BlobClient
                blobClient = blobContainerClient.GetBlobClient(pdfFileName);
                using (Stream stream = new MemoryStream(System.IO.File.ReadAllBytes(pdfFilePath)))
                {
                    await blobClient.UploadAsync(stream, true);
                };
                //Client SAS Token Url for the file
                //sasUri_resultPdf = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddYears(100));
                sasUri_resultPdf = $"{param_benchmarkServiceRequest.ServiceUrl}{FileDownLoadService}/{pdfFileName}";

                //Delete pdf file
                File.Delete(pdfFilePath);
                //Delete html file
                File.Delete(htmlFilePath);
            }
            else
            {
                //Delete html file
                File.Delete(htmlFileName);
                throw new Exception("PDF File is not converted");
            }

            //Create BlobClient
            blobClient = blobContainerClient.GetBlobClient($"{param_benchmarkServiceRequest.JobName}-{param_benchmarkServiceRequest.DisclosureNumber}-{jobId}-meta.json");
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(disclosureAnswers))))
            {
                await blobClient.UploadAsync(stream, true);
            };

            //Client SAS Token Url for the file
            //var sasUri_resultMetaFile = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddYears(100));
            var sasUri_resultMetaFile = $"{param_benchmarkServiceRequest.ServiceUrl}{FileDownLoadService}/{param_benchmarkServiceRequest.JobName}-{param_benchmarkServiceRequest.DisclosureNumber}-{jobId}-meta.json";
            var response = new BenchmarkReportGenerationServiceExecutionResponse()
            {
                Response = benchmarkResult,
                ResultFile = sasUri_resultFile,
                ResultHtmlFile = sasUri_resultHtml,
                ResultMetaDataFile = sasUri_resultMetaFile,
                ResultPdfFIle = sasUri_resultPdf,
                MetaData = disclosureAnswers,
            };

            //Update Job Information with Results
            registeredJob.BechmarkResult = benchmarkResult;
            registeredJob.BenchmarkMetaDataFileUrl = sasUri_resultMetaFile;
            registeredJob.BenchmarkResultFileUrl = sasUri_resultFile;
            registeredJob.BenchmarkResultHtmlFileUrl = sasUri_resultHtml;
            registeredJob.ProcessedTime = DateTime.UtcNow;
            registeredJob.ProcessStatus = ProcessStatus.Completed;
            registeredJob.BenchmarkResultPdfFileUrl = sasUri_resultPdf;
            await this.benchmarkjobRepo.Update(registeredJob);

            return response;
        }

        //define private methods for make string to well formatted json with identation.
        private string MakeWellFormedJsonString(string jsonString)
        {
            var token = Newtonsoft.Json.Linq.JToken.Parse(jsonString);
            return token.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        private async Task UpdateBenchmarkJobProcessStatus(Guid jobId, ProcessStatus processStatus)
        {
            await this.benchmarkjobRepo.UpdateProcessStatus(jobId, processStatus);
        }

        private async Task<BenchmarkJob> RegisterJob(BenchmarkServiceRequest jobRequest)
        {
            if (jobRequest == null) { throw new ArgumentNullException(nameof(jobRequest)); }

            //just in case if Job is already registered, delete it.
            var existingJob = await this.benchmarkjobRepo.FindByBenchmarkJobId(jobRequest.JobId);
            if (existingJob != null)
            {
                await this.benchmarkjobRepo.Delete(existingJob.id);
                logger.LogInformation($"Existing Job is deleted with JobId: {jobRequest.JobId}");
            }

            //Register Job
            var newBenchmarkJob = new BenchmarkJob()
            {
                //if JobId is not provided, generate new one or JobId should be provided by jobRequest
                JobId = jobRequest.JobId == Guid.Empty ? Guid.NewGuid() : jobRequest.JobId,
                JobName = jobRequest.JobName,
                Owner = jobRequest.JobOwner,
                Documents = new List<BenchmarkDocumentInformation>(),
                DisclosureNumber = jobRequest.DisclosureNumber,
                DisclosureName = jobRequest.DisclosureName,
                DisclosureRequirement = jobRequest.DisclosureRequirement,
                DisclosureAnnex = jobRequest.DisclosureAnnex,
                ProcessStartTime = DateTime.UtcNow,
                ProcessStatus = ProcessStatus.InProgress
            };

            var documents = await jobRequest.DocumentIds.ToAsyncEnumerable().SelectAwait<string, BenchmarkDocumentInformation>(async documentId =>
            {
                var document = await this.documentRepo.FindByDocumentId(documentId);
                if (document != null)
                {
                    return new BenchmarkDocumentInformation()
                    {
                        DocumentId = documentId,
                        DocumentName = document.FileDisplayName
                    };
                }
                else
                {
                    return null;
                }
            }).ToArrayAsync();

            if (documents != null)
                newBenchmarkJob.Documents = documents.Where(x => x != null).Select(x => x).ToList();

            await this.benchmarkjobRepo.Register(newBenchmarkJob);

            return newBenchmarkJob;
        }
    }
}

