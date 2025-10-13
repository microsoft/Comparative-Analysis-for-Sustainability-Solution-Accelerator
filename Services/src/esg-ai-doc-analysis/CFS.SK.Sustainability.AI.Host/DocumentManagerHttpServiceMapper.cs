// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using CFS.SK.Sustainability.AI.Storage.Document.Entities;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Word;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.Json;
using CFS.SK.Sustainability.AI.Models;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Text;
using System;
using CFS.SK.Abstracts;

namespace CFS.SK.Sustainability.AI.Host
{
    public static class DocumentManagerHttpServiceMapper
    {
        public static void UseDocumentManagerEndpoint(this WebApplication app)
        {
            app.MapPost("/DocumentManager/RegisterDocument", async (HttpContext httpContext, 
                                                                    [FromServices] DocumentManager docManagerHandler,
                                                                    [FromServices] IConfiguration config,
                                                                    [FromServices] ApplicationContext appContext,
                                                                    [FromForm] IFormFile fileBinary) =>
            {

                if (docManagerHandler != null)
                {
                    List<KeyValuePair<string, string>> tags = null;
                    if (httpContext.Request.Form.ContainsKey("Tags"))
                    {
                        tags = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(httpContext.Request.Form["Tags"]);
                    }

                    var result = await docManagerHandler.RegisterDocument(new RegisterDocumentFileServiceRequest() { FileBinary = fileBinary, Tags = tags });

                    //get HTTP Request Object
                    var currentRequest = httpContext.Request;
                    var locationUrl = $"https://{currentRequest.Host.Value}/DocumentManager/GetDocumentProcessStatus/{result.DocumentId}";

                    

                    var paramJson = new { locationUrl = locationUrl, fileName = fileBinary.FileName, documentId = result.DocumentId };
                    var serializedParamJson = JsonConvert.SerializeObject(paramJson);
                    var content = new StringContent(serializedParamJson, Encoding.UTF8, "application/json");

                    //Add Log
                    appContext.SKLoggerFactory.CreateLogger("DocumentManager/RegisterDocument").LogInformation($"Document information to be passed :  {serializedParamJson}");

                    //Invoke Process Watcher
                    var response = await appContext.httpClient.PostAsync(config["DocumentPreprocessing:processwatcherUrl"], content); // CodeQL [SM03781] This repository is no longer actively maintained. Fixing this issue is not feasible as no further development is planned.

                    return Results.Accepted(locationUrl, result);
                }
                else
                {
                    return Results.BadRequest();
                }
            })
            .DisableAntiforgery()
            .Produces<Document>(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);


            app.MapGet("/DocumentManager/GetDocumentProcessStatus/{documentId}", async ([FromServices] DocumentManager docManagerHandler, string documentId) =>
            {
               return await docManagerHandler.GetDocumentProcessingStatus(documentId);
            })
            .DisableAntiforgery()
            .Produces<Document>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

            app.MapPost("/DocumentManager/RegisterDocumentWithFileLocation", async ([FromServices] DocumentManager docManagerHandler, [FromBody] RegisterDocumentFromBlobStorageServiceRequest serviceRequest) =>
            {
                if (docManagerHandler != null)
                {
                    var result = await docManagerHandler.RegisterDocumentFromBlobStorage(serviceRequest);
                    return result;
                }
                else
                {
                    throw new Exception("Document Manager handler not registered");
                }
            })
            .DisableAntiforgery()
            .Produces<Document>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);


            app.MapPost("/DocumentManager/RegisterDocumentWithFileURL", async ([FromServices] DocumentManager docManagerHandler, [FromBody] RegisterDocumentFromFileUrlServiceRequest serviceRequest) =>
            {
                if (docManagerHandler != null)
                {
                    var result = await docManagerHandler.RegisterDocumentFromFileUrl(serviceRequest);
                    return result;
                }
                else
                {
                    throw new Exception("Document Manager handler not registered");
                }
            })
            .DisableAntiforgery()
            .Produces<Document>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);



            app.MapPost("/DocumentManager/DeleteDocumentByDocumentId", async (HttpContext context, [FromServices] DocumentManager docManagerHandler, [FromForm] string documentId) =>
            {
                if (docManagerHandler != null)
                {
                    await docManagerHandler.UnRegisterDocumentByDocId(documentId);
                    return Results.Ok(new { DocumentId = documentId, Result = "deleted" });
                }
                else
                {
                    throw new Exception("Document Manager handler not registered");
                }
            })
            .DisableAntiforgery()
            .Produces<Document>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

            app.MapPost("/DocumentManager/DeleteDocumentById", async ([FromServices] DocumentManager docManagerHandler, [FromForm] string Id) =>
            {
                if (docManagerHandler != null)
                {
                    await docManagerHandler.UnRegisterDocumentById(Id);
                    return Results.Ok(new { Id = Id, Result = "deleted" });
                }
                else
                {
                    throw new Exception("Document Manager handler not registered");
                }
            })
            .DisableAntiforgery()
            .Produces<Document>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);


            app.MapGet("/DocumentManager/GetAllDocuments", async ([FromServices] DocumentManager docManagerHandler) =>
            {
                if (docManagerHandler != null)
                {
                    return await docManagerHandler.GetAllDocuments();
                }
                else
                {
                    throw new Exception("Document Manager handler not registered");
                }
            })
            .DisableAntiforgery()
            .Produces<IEnumerable<Document>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

            app.MapPost("/DocumentManager/GetDocumentsByTags", async ([FromServices] DocumentManager docManagerHandler, string[] tags) =>
            {
                if (docManagerHandler != null)
                {
                    return await docManagerHandler.GetDocumentsByTags(tags);
                }
                else
                {
                    throw new Exception("Document Manager handler not registered");
                }
            })
            .DisableAntiforgery()
            .Produces<IEnumerable<Document>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

            app.MapPost("/DocumentManager/GetDocumentById", async ([FromServices] DocumentManager docManagerHandler, [FromForm] string id) =>
            {
                if (docManagerHandler != null)
                {
                    return await docManagerHandler.GetDocumentById(id);
                }
                else
                {
                    throw new Exception("Document Manager handler not registered");
                }
            })
            .DisableAntiforgery()
            .Produces<Document>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

            app.MapPost("/DocumentManager/GetDocumentByDocumentId", async ([FromServices] DocumentManager docManagerHandler, [FromForm] string documentId) =>
            {
                if (docManagerHandler != null)
                {
                    return await docManagerHandler.GetDocumentByDocumentId(documentId);
                }
                else
                {
                    throw new Exception("Document Manager handler not registered");
                }
            })
            .DisableAntiforgery()
            .Produces<Document>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);


            app.MapPut("/DocumentManager/UpdateDocumentProcessStatus", async ([FromServices] DocumentManager docManagerHandler, string documentId, string status) =>
            {
                if (docManagerHandler != null)
                {
                    return await docManagerHandler.UpdateDocumentStatus(documentId, status);
                }
                else
                {
                    throw new Exception("Document Manager handler not registered");
                }
            })
            .DisableAntiforgery()
            .Produces<Document>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

            app.MapPost("/DocumentManager/GetDocumentSummary", async ([FromServices] DocumentManager docManagerHandler, [FromForm] string documentId) =>
            {
                if (docManagerHandler != null)
                {
                    return await docManagerHandler.AskAboutDocumentSummary(documentId, "Show me the summary for this document");
                }
                else
                {
                    throw new Exception("Document Manager handler not registered");
                }
            })
            .DisableAntiforgery()
            .Produces<Microsoft.KernelMemory.MemoryAnswer>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);


            app.MapPost("/DocumentManager/AskAgainstDocument", async ([FromServices] DocumentManager docManagerHandler, [FromBody] AskAboutDocumentServiceRequest serviceRequest) =>
            {
                if (docManagerHandler != null)
                {
                    return await docManagerHandler.AskAboutDocuments(serviceRequest);
                }
                else
                {
                    throw new Exception("Document Manager handler not registered");
                }
            })
            .DisableAntiforgery()
            .Produces<Microsoft.KernelMemory.MemoryAnswer>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        }
    }
}
