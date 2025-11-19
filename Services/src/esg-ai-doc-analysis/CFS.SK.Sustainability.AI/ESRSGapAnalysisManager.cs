// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Azure.Storage.Blobs;
using CFS.SK.Abstracts;
using CFS.SK.Abstracts.Model;
using CFS.SK.Sustainability.AI.Models;
using CFS.SK.Sustainability.AI.Utils;
using CFS.SK.Sustainability.AI.Storage.Document;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;
using CFS.SK.Sustainability.AI.Storage.GapAnalysis.Entities;
using CFS.SK.Sustainability.AI.Storage.GapAnalysis;
using Microsoft.Identity.Client;
using Azure.Identity;


namespace CFS.SK.Sustainability.AI
{
    public class ESRSGapAnalysisManager : SemanticKernelLogicBase<ESRSGapAnalysisManager>
    {
        const string FileDownLoadService = "/ESRS/Results";
        private readonly ESRSDisclosureRetriever _esrsDisclosureRetriever;
        private readonly DocumentRepository _documentRepository;
        private readonly GapAnalysisJobRepository _gapAnalysisJobRepository;
        private readonly IConfiguration _config;

        public ESRSGapAnalysisManager(ApplicationContext appContext,
                                        ILogger<ESRSGapAnalysisManager> logger,
                                        ESRSDisclosureRetriever esrsDisclosureRetriever,
                                        GapAnalysisJobRepository gapAnalysisJobRepo,
                                        DocumentRepository documentRepo,
                                        IConfiguration config) : base(appContext, logger)
        {
            this._esrsDisclosureRetriever = esrsDisclosureRetriever;
            this._logger = logger;
            this._documentRepository = documentRepo;
            this._gapAnalysisJobRepository = gapAnalysisJobRepo;
            this._config = config;
        }

        //TODO : HAVE TO CREATE MANAGEMENT FOR GAP ANALYSIS
        public async Task<GapAnalysisJob> GetAnalysisJobByJobId(Guid jobId)
        {
            return await this._gapAnalysisJobRepository.FindByGapAnalysisJobId(jobId);
        }

        public async Task<bool> DeleteGapAnalysisJob(Guid jobId)
        {
            var analysisJob = await this._gapAnalysisJobRepository.FindByGapAnalysisJobId(jobId);
            if (analysisJob != null)
            {
                await this._gapAnalysisJobRepository.Delete(analysisJob.id);
                return true;
            }
            else
                return false;
        }

        public async Task UpdateAnalysisJobProcessStatus(Guid jobId, ProcessStatus status)
        {
            var analysisJob = await this._gapAnalysisJobRepository.FindByGapAnalysisJobId(jobId);
            if (analysisJob != null)
            {
                analysisJob.ProcessStatus = status;
                await this._gapAnalysisJobRepository.Update(analysisJob);
            }
        }

        public async Task<GapAnalysisJob> RegisterJob(GapAnalysisServiceRequest jobRequest)
        {
            //check parameter is valid
            if (jobRequest == null)
                throw new ArgumentNullException("Job Request parameter is not valid");

            //check if the document exists then delete it
            var existingJob = await this._gapAnalysisJobRepository.FindByGapAnalysisJobId(jobRequest.JobId);
            if (existingJob != null)
            {
                await this._gapAnalysisJobRepository.Delete(existingJob.id);
                _logger.LogInformation($"Existing Job {jobRequest.JobId} is deleted");
            }

            //Register the job
            var newJob = new GapAnalysisJob()
            {
                JobId = jobRequest.JobId,
                DisclosureNumber = jobRequest.DisclosureNumber,
                DisclosureName = jobRequest.DisclosureName,
                DisclosureRequirement = jobRequest.DisclosureRequirement,
                DisclosureAnnex = jobRequest.DisclosureAnnex,
                Owner = jobRequest.JobOwner,
                ProcessStartTime = DateTime.UtcNow,
                ProcessStatus = ProcessStatus.InProgress
            };

            //Find Document Information
            var document = await this._documentRepository.FindByDocumentId(jobRequest.DocumentId);
            if (document != null)
            {
                newJob.Document = new GapAnalysisDocumentInformation()
                {
                    DocumentId = document.DocumentId,
                    DocumentName = document.FileDisplayName
                };
            }

            await this._gapAnalysisJobRepository.Register(newJob);

            return newJob;
        }


        public override async Task<IReturnValue?> ExecuteAndReturnResult(IParameter gapAnalysisServiceRequest)
        {
            var param_gapAnalysisServiceRequest = (GapAnalysisServiceRequest)gapAnalysisServiceRequest;

            //Register Job
            var registeredJob = await RegisterJob(param_gapAnalysisServiceRequest);

            //Get Disclosure
            var disclosureDescription = await this._esrsDisclosureRetriever.ExecuteAndReturnResultAsMemoryAnswer(
                                                                                                            new ESRSDisclosureRetrieverServiceRequest
                                                                                                            {
                                                                                                                DisclosureNumber = param_gapAnalysisServiceRequest.DisclosureNumber,
                                                                                                                DisclosureRequirement = param_gapAnalysisServiceRequest.DisclosureRequirement,
                                                                                                                DisclosureRequirementDetail = param_gapAnalysisServiceRequest.DisclosureRequirementDetail,
                                                                                                                DocumentId = param_gapAnalysisServiceRequest.DocumentId,
                                                                                                                DisclosureName = param_gapAnalysisServiceRequest.DisclosureName,
                                                                                                                AnnexDescription = param_gapAnalysisServiceRequest.DisclosureAnnex,
                                                                                                                SizeOfChars = 10000
                                                                                                            });



            string? disclosure_number = param_gapAnalysisServiceRequest.DisclosureNumber;
            string? disclosure_requirement = param_gapAnalysisServiceRequest.DisclosureRequirement;
            string? disclosure_requirement_detail = param_gapAnalysisServiceRequest.DisclosureRequirementDetail;
            string? statement = disclosureDescription.Result;
            string? annex = param_gapAnalysisServiceRequest?.DisclosureAnnex;
            string? disclosure_name = param_gapAnalysisServiceRequest?.DisclosureName;

            disclosure_requirement = disclosure_requirement.TrimEnd(new char[] { '\r', '\n' });

            var pluginDirectoryPath = Utils.Plugin.GetPluginDirectoryPath("plugins", "CSRDPlugin");

            if (this._appContext.Kernel.Plugins.Where(x => x.Name == "CSRDPlugin").SingleOrDefault() == null)
            {
                this._appContext.Kernel.ImportPluginFromPromptDirectory(pluginDirectoryPath);
            }
            var csrdSampleGenPlugin = this._appContext.Kernel.Plugins["CSRDPlugin"];

            var result_stream = this._appContext.Kernel.InvokeStreamingAsync<string>(csrdSampleGenPlugin["GAPAnalyzeReportGenerator"],
                                                        new KernelArguments
                                                        {
                                                            ["disclosure_number"] = disclosure_number,
                                                            ["disclosure_name"] = disclosure_name,
                                                            ["disclosure_requirement"] = disclosure_requirement,
                                                            ["disclosure_details"] = disclosure_requirement_detail,
                                                            ["annex"] = annex,
                                                            ["statement"] = statement
                                                        });

            //Getting the result from the stream and generate string
            var sb_Result = new StringBuilder();
            await foreach (var item in result_stream)
            {
                this._logger.LogInformation(item);
                sb_Result.Append(item);
            }

            var analysis_resultString = sb_Result.ToString();

            //Get BlobConnectionString
            var blobConnectionString = this._config["ConnectionStrings:BlobStorage"];

            //Create BlobServiceClient with DefaultAzure Credential
            //Define Storage Blob Uri from ConnectionString
            //Get BlobServiceClient from ConnectionString
            var blobServiceClient = StorageAccessUtil.GetBlobClientFromConnectionString(blobConnectionString);

            //Create ContainerClient
            var blobContainerClient = blobServiceClient.GetBlobContainerClient("results");
            await blobContainerClient.CreateIfNotExistsAsync();

            //Create BlobClient
            //Get Current YYYYMMDDHHMMSS
            var jobId = DateTime.Now.ToString("yyyyMMddHHmmss");
            var fileName = $"GAPAnalysisReport-{disclosure_number}-{jobId}";
            //validate file name
            EnsureSafeSimpleFileName(fileName);
            var blobClient = blobContainerClient.GetBlobClient($"{fileName}.md");
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(analysis_resultString)))
            {
                await blobClient.UploadAsync(stream, true);
            };

            //Client SAS Token Url for the file
            //var sasUri_resultFile = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddYears(100));
            var sasUri_resultFile = $"{param_gapAnalysisServiceRequest.ServiceUrl}{FileDownLoadService}/{fileName}";

            //Create BlobClient
            var htmlFileName = $"{fileName}.html";
            //validate html file name
            EnsureSafeSimpleFileName(htmlFileName);
            blobClient = blobContainerClient.GetBlobClient(htmlFileName);
            byte[] byteArray_html = MarkdownHtmlConverter.Convert(analysis_resultString);

            using (Stream stream = new MemoryStream(byteArray_html))
            {
                await blobClient.UploadAsync(stream, true);
            };

            //Client SAS Token Url for the file
            //var sasUri_resultHtml = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddYears(100));
            var sasUri_resultHtml = $"{param_gapAnalysisServiceRequest.ServiceUrl}{FileDownLoadService}/{htmlFileName}";
            //Save Converted Html
            await System.IO.File.WriteAllBytesAsync(htmlFileName, byteArray_html);

            //Convert HTML to PDF
            //Need to extra work for convert html to pdf
            var pdfFileName = $"{fileName}.pdf";
            HtmlPdfConverter.Convert(htmlFileName, pdfFileName);

            //if Pdf file is exist then upload to the blob
            string sasUri_resultPdf;

            if (System.IO.File.Exists(pdfFileName))
            {
                blobClient = blobContainerClient.GetBlobClient(pdfFileName);
                using (Stream stream = new FileStream(pdfFileName, FileMode.Open))
                {
                    await blobClient.UploadAsync(stream, true);
                };

                //Client SAS Token Url for the file
                //sasUri_resultPdf = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddYears(100));
                sasUri_resultPdf = $"{param_gapAnalysisServiceRequest.ServiceUrl}{FileDownLoadService}/{pdfFileName}";
                //Delete pdf file
                System.IO.File.Delete(pdfFileName);
                //Delete html file
                System.IO.File.Delete(htmlFileName);// CodeQL [SM00414] htmlFileName validated by EnsureSafeSimpleFileName.
            }
            else
            {
                //Delete html file
                System.IO.File.Delete(htmlFileName); // CodeQL [SM00414] This variable is not based on user input, so no need to handle the Code QL issue.
                throw new Exception("PDF File is not converted");
            }

            var metaDataFileName = $"GAPAnalysisReport-{disclosure_number}-{jobId}-meta.json";
            blobClient = blobContainerClient.GetBlobClient(metaDataFileName);

            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(disclosureDescription))))
            {
                await blobClient.UploadAsync(stream, true);
            };

            //Client SAS Token Url for the file
            //*var sasUri_resultMetaFile* = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddYears(100));
            var sasUri_resultMetaFile = $"{param_gapAnalysisServiceRequest.ServiceUrl}{FileDownLoadService}/{metaDataFileName}";

            var gapAnalysis_response = new GapAnalysisReportGenerationServiceExecutionResponse()
            {
                Response = analysis_resultString,
                MetaData = disclosureDescription,
                ResultFile = sasUri_resultFile,
                ResultPdfFile = sasUri_resultPdf,
                ResultHtmlFile = sasUri_resultHtml,
                ResultMetaDataFile = sasUri_resultMetaFile
            };


            //Update GapAnalysisJob
            registeredJob.ProcessedTime = DateTime.UtcNow;
            registeredJob.ProcessStatus = ProcessStatus.Completed;
            registeredJob.Result = analysis_resultString;
            registeredJob.MetaData = disclosureDescription.Result;
            registeredJob.ResultFileUrl = sasUri_resultFile;
            registeredJob.ResultFileHtmlUrl = sasUri_resultHtml;
            registeredJob.MetaDataFileUrl = sasUri_resultMetaFile;
            registeredJob.ResultPdfFileUrl = sasUri_resultPdf;

            //Update the job
            await this._gapAnalysisJobRepository.Update(registeredJob);

            return gapAnalysis_response;
            //return result.GetValue<string>();
        }
       
        // Validate simple file name to prevent path traversal attacks
        private static void EnsureSafeSimpleFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("File name is empty or null.");

            if (name.Contains("..") || name.Contains("/") || name.Contains("\\"))
                throw new ArgumentException("Invalid file name (contains path components or traversal).");
            
            // Whitelist chars: letters, digits, dash, underscore, dot
            foreach (char c in name)
            {
                if (!(char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.'))
                    throw new ArgumentException("Invalid character in file name.");
            }

        }
    }
}
