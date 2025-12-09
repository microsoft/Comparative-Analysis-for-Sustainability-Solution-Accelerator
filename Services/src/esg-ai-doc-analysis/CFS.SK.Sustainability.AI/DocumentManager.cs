// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using CFS.SK.Abstracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.KernelMemory;
using Microsoft.Extensions.Configuration;
using CFS.SK.Sustainability.AI.Storage.Document;
using CFS.SK.Sustainability.AI.Storage.Document.Entities;
using System.Collections.Specialized;
using Document = CFS.SK.Sustainability.AI.Storage.Document.Entities.Document;
using System.Text.Json;
using System.IO;
using Azure.Storage;
using DocumentFormat.OpenXml.Office2010.Word;
using CFS.SK.Sustainability.AI.Models;
using CFS.SK.Abstracts.Model;
using Azure.Identity;
using CFS.SK.Sustainability.AI.Utils;

namespace CFS.SK.Sustainability.AI
{
    public class DocumentManager : SemanticKernelLogicBase<DocumentManager>
    {
        private IKernelMemory memoryWebClient { get; set; }
        private readonly ILogger<DocumentManager> logger;
        private readonly DocumentRepository docRepo;
        private readonly IConfiguration config;
        private readonly HttpClient httpClient;

        public DocumentManager(ApplicationContext appContext, DocumentRepository docRepo, IKernelMemory memoryClient ,HttpClient httpClient, ILogger<DocumentManager> logger, IConfiguration config) : base(appContext, logger)
        {
            memoryWebClient = memoryClient;
            this.logger = logger;
            this.docRepo = docRepo;
            this.config = config;
            this.httpClient = httpClient;
        }

        async public Task<DocumentServiceResult> RegisterDocument(IParameter serviceRequest)
        {
            RegisterDocumentFileServiceRequest param_serviceRequest = (RegisterDocumentFileServiceRequest)serviceRequest;

            using (var fileStream = param_serviceRequest.FileBinary.OpenReadStream())
            {
                fileStream.Position = 0;

                var fileName = param_serviceRequest.FileBinary.FileName;
                var documentId = Guid.NewGuid().ToString();
                var fileLocation = $"default/{documentId}/{fileName}";
                var status = "Processing";
                var doc = new Document()
                {
                    FileDisplayName = fileName,
                    FileLocation = fileLocation,
                    Tags = param_serviceRequest.Tags,
                    Status = status,
                    DocumentId = documentId,
                    UploadedTime = DateTime.UtcNow
                };

                //Add Code to invoke Kernel Memory Service

                TagCollection? tagCollection = null;

                if (param_serviceRequest.Tags != null)
                {
                    tagCollection = new TagCollection();
                    param_serviceRequest.Tags.ForEach(tag => tagCollection.Add(tag.Key, tag.Value));
                }

                await this.memoryWebClient.ImportDocumentAsync(content: fileStream, fileName: fileName, documentId: doc.DocumentId, tags: tagCollection,
                                                                steps: new[]
                                                                {
                                                                    Constants.PipelineStepsExtract,
                                                                    //Constants.PipelineStepsSummarize, //it take a time. good to have for synthesize but not necessary. 
                                                                    Constants.PipelineStepsPartition,
                                                                    Constants.PipelineStepsGenEmbeddings,
                                                                    Constants.PipelineStepsSaveRecords,
                                                                });
                var result = await this.docRepo.Register(doc);

                //mapping process between Document and DocumentServiceResult
                return DocumentServiceResult.MapDataEntities(result);
            }       
                    
        }

        async public Task<DataPipelineStatus?> GetDocumentProcessingStatus(string documentId)
        {
            return await this.memoryWebClient.GetDocumentStatusAsync(documentId: documentId, index: "default");
        }

        async public Task<DocumentServiceResult> RegisterDocument(Document doc)
        {
            var result = await this.docRepo.Register(doc);
            return DocumentServiceResult.MapDataEntities(result);
        }

        async public Task<DocumentServiceResult> RegisterDocumentFromBlobStorage(RegisterDocumentFromBlobStorageServiceRequest serviceRequest)
        {
            if (serviceRequest == null) { throw new ArgumentNullException(nameof(serviceRequest)); }

            //Get BlobConnectionString
            var blobConnectionString = this.config["ConnectionStrings:BlobStorage"];
            //Get BlobServiceClient from ConnectionString
            var blobServiceClient = StorageAccessUtil.GetBlobClientFromConnectionString(blobConnectionString);

            //Create ContainerClient
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(serviceRequest.ContainerName);
            //Create BlobClient
            var blobClient = blobContainerClient.GetBlobClient(serviceRequest.FileLocation);

            //Prepare Document Information
            var fileName = Path.GetFileName(serviceRequest.FileLocation);
            var fileLocation = serviceRequest.FileLocation;

            //Add Code to invoke Kernel Memory Service
            TagCollection? tagCollection = null;

            if (serviceRequest.Tags != null)
            {
                tagCollection = new TagCollection();
                serviceRequest.Tags.ForEach(tag => tagCollection.Add(tag.Key, tag.Value));
            }

            //Random Doc ID
            var _docId = Guid.NewGuid().ToString();
            var doc = new Document()
            {
                DocumentId = _docId,
                FileDisplayName = fileName,
                FileLocation = $"default/{_docId}/{fileName}",
                Tags = serviceRequest.Tags,
                Status = "Processing",
                UploadedTime = DateTime.UtcNow
            };
            //Read the file
            using (var fileStream = new StreamReader(blobClient.OpenRead()))
            {
                await this.memoryWebClient.ImportDocumentAsync(content: fileStream.BaseStream, 
                                                              fileName: fileName, 
                                                              documentId: doc.DocumentId, tags: tagCollection,
                                                              steps: new[]
                                                                {
                                                                    Constants.PipelineStepsExtract,
                                                                    Constants.PipelineStepsPartition,
                                                                    Constants.PipelineStepsGenEmbeddings,
                                                                    Constants.PipelineStepsSaveRecords
                                                                });
            }

            //Invoke Logic App to follow up with the processing
            //http client call to Logic App
            var content = new StringContent(JsonSerializer.Serialize(new { DocumentId = doc.DocumentId }),
                                            Encoding.UTF8, "application/json");
            await this.httpClient.PostAsync(this.config["DocumentPreprocessing:processwatcherUrl"], content); // CodeQL [SM03781] We are reading this value from appsettings.json, this is not an user input

            var result = await this.docRepo.Register(doc);

            //mapping process between Document and DocumentServiceResult
            return DocumentServiceResult.MapDataEntities(result);
        }

        async public Task<DocumentServiceResult> RegisterDocumentFromFileUrl(RegisterDocumentFromFileUrlServiceRequest serviceRequest)
        {
            //Validate URL to prevent SSRF attack
            if (AntiSsrfValidation(serviceRequest.FileUrl))
            {
                throw new Exception("AntiSSRF validation failed - Invalid or unauthorized URL provided");
            }
            //Download file from URL then take only fileName from URL
            //the file location URL in the document will be mixed with SAS token to get it.
            //sample url - https://microsoft.seismic.com/app?ContentId=d6e9f9bb-70d4-4845-a2ad-dd25ecc343d6#/doccenter/a5266a70-9230-4c1e-a553-c5bddcb7a896/doc/%252Fdde0caec0e-9236-f21b-2991-5868e63d3984%252FdfYTZjNDRiZDMtMzEwZS1kNWZkLTNjOGEtNjliYWJjMjhmMmUw%252CPT0%253D%252CUGl0Y2ggRGVjaw%253D%253D%252Flffb13c1f1-d960-4bbe-8685-000afbf5a67f//?mode=view&parentPath=sessionStorage
            serviceRequest.FileUrl = FileNameValidator.ValidateAndReturnSafeFileName(serviceRequest.FileUrl);
            var fileBytes = await this.httpClient.GetByteArrayAsync(serviceRequest.FileUrl);
            //write ByteArray to Stream
            MemoryStream downloadedFileMemoryStream = new MemoryStream(fileBytes);
            downloadedFileMemoryStream.Seek(0, SeekOrigin.Begin);

            if (downloadedFileMemoryStream.Length == 0)
            {
                throw new FileNotFoundException("The file url is not valid. there is no file in this url");
            }

            //Add Code to invoke Kernel Memory Service
            TagCollection? tagCollection = null;

            if (serviceRequest.Tags != null)
            {
                tagCollection = new TagCollection();
                serviceRequest.Tags.ForEach(tag => tagCollection.Add(tag.Key, tag.Value));
            }

            //Random Doc ID
            var _docId = Guid.NewGuid().ToString();

            var doc = new Document()
            {
                FileDisplayName = serviceRequest.FileName,
                FileLocation = $"default/{_docId}/{serviceRequest.FileName}",
                Tags = serviceRequest.Tags,
                Status = "Processing",
                DocumentId = Guid.NewGuid().ToString(),
                UploadedTime = DateTime.UtcNow
            };

            await this.memoryWebClient.ImportDocumentAsync(content: downloadedFileMemoryStream,
                                                          fileName: serviceRequest.FileName,
                                                          documentId: doc.DocumentId, tags: tagCollection,
                                                          steps: new[]
                                                            {
                                                                    Constants.PipelineStepsExtract,
                                                                    Constants.PipelineStepsPartition,
                                                                    Constants.PipelineStepsGenEmbeddings,
                                                                    Constants.PipelineStepsSaveRecords
                                                            });

            downloadedFileMemoryStream?.Dispose();

            //Invoke Logic App to follow up with the processing
            //http client call to Logic App
            var content = new StringContent(JsonSerializer.Serialize(new { DocumentId = doc.DocumentId }),
                                            Encoding.UTF8, "application/json");
            await this.httpClient.PostAsync(this.config["DocumentPreprocessing:processwatcherUrl"], content); // CodeQL [SM03781] We are reading this value from appsettings.json, this is not an user input

            var result = await this.docRepo.Register(doc);

            //mapping process between Document and DocumentServiceResult
            return DocumentServiceResult.MapDataEntities(result);
        }


        async public Task UnRegisterDocumentByDocId(string DocumentId)
        {
            //Add Code to invoke Kernel Memory Service
            await this.memoryWebClient.DeleteDocumentAsync(DocumentId);

            var targetDoc = await this.docRepo.FindByDocumentId(DocumentId);
            if (targetDoc == null) return;

            await this.docRepo.Delete(targetDoc.id);
        }

        async public Task UnRegisterDocumentById(string Id)
        {
            await this.docRepo.Delete(Guid.Parse(Id));
        }

        async public Task<IEnumerable<DocumentServiceResult>> GetAllDocuments()
        {
            var results = await this.docRepo.GetAllDocuments();

            //mapping process between IEnumerable<Document> and IEnumerable<DocumentServiceResult>
            return results.Select(doc => DocumentServiceResult.MapDataEntities(doc));

        }

        async public Task<IEnumerable<DocumentServiceResult>> GetDocumentsByTags(string[] tags)
        {
            var results = await this.docRepo.FindByTags(tags);
            //mapping process between IEnumerable<Document> and IEnumerable<DocumentServiceResult>
            return results.Select(doc => DocumentServiceResult.MapDataEntities(doc));
        }

        async public Task<DocumentServiceResult> GetDocumentById(string Id)
        {
            var result = await this.docRepo.Find(Guid.Parse(Id));
            return DocumentServiceResult.MapDataEntities(result);
        }

        async public Task<DocumentServiceResult> GetDocumentByDocumentId(string DocumentId)
        {
            var result = await this.docRepo.FindByDocumentId(DocumentId);
            return DocumentServiceResult.MapDataEntities(result);
        }

        async public Task<DocumentServiceResult> UpdateDocumentStatus(string DocumentId, string Status)
        {
            var result = await this.docRepo.UpdateProcessStatus(DocumentId, Status);
            return DocumentServiceResult.MapDataEntities(result);
        }

        async public Task<MemoryAnswer> AskAboutDocuments(AskAboutDocumentServiceRequest serviceRequest)
        {
            //generating filter with multiple document ids
            var documentFilter = new MemoryFilter();
            foreach (var docId in serviceRequest.DocumentIds)
            {
                documentFilter.ByDocument(docId);
            }
            
            return await this.memoryWebClient.AskAsync(serviceRequest.Question, filter: documentFilter);
        }

        async public Task<MemoryAnswer> AskAboutDocumentSummary(string DocumentId, string Question, KeyValuePair<string,string>? tag = null)
        {
            if (tag.HasValue)
            {
                return await this.memoryWebClient.AskAsync(Question, filter: new MemoryFilter().ByDocument(DocumentId).ByTag(tag.Value.Key, tag.Value.Value));
            }else
            {
                return await this.memoryWebClient.AskAsync(Question, filter: new MemoryFilter().ByDocument(DocumentId));
            }
        }
        
        // Anti-SSRF validation method
        private bool AntiSsrfValidation(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return true; // Invalid URL
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                return true; // Invalid URL format
            }

            //the parameter flows to the validation method
            bool isInvalidUri = !IsInAllowedDomain(uri) || !IsHttpsScheme(uri);

            //validation method call flows to boolean return statement
            return isInvalidUri;
        }

        //Domain validation helper
        private bool IsInAllowedDomain(Uri uri)
        {
            var allowedDomainsConfig = this.config["AntiSSRF:AllowedDomains"];
            if (string.IsNullOrWhiteSpace(allowedDomainsConfig))
            {
                throw new InvalidOperationException("AllowedDomains configuration is missing");
            }
            // Add this line to parse the config string into an array
            var allowedDomains = allowedDomainsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(d => d.Trim())
                                                    .ToArray();
            
            return allowedDomains.Any(domain => 
                uri.Host.Equals(domain, StringComparison.OrdinalIgnoreCase) ||
                uri.Host.EndsWith($".{domain}", StringComparison.OrdinalIgnoreCase));
        }

        // HTTPS validation helper
        private bool IsHttpsScheme(Uri uri)
        {
            return uri.Scheme == Uri.UriSchemeHttps;
        }
    }
}
