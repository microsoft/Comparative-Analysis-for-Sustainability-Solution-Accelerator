// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFS.SK.Sustainability.AI.plugins.CSRDPlugin
{
    public sealed class DisclosureRetrieverPlugin
    {
        [KernelFunction]
        [Description("Get relevant content by disclosure and annex from Document")]
        public async Task<MemoryAnswer> GetRelevantContent(
            [Description("Kernel Memory Client")] IKernelMemory memory,
            [Description("DocumentId")] string documentId,
            [Description("Disclosure Number")] string disclosureNo,
            [Description("Disclosure Name")] string disclosureName,
            [Description("Disclosure Requirement")] string disclosure_requirement,
            [Description("Disclosure Requirement Detail")] string disclosure_requirement_detail,
            [Description("Disclosure Annex")] string annex,
            [Description("Return Characters")] int sizeOfChars)
        {

            var inputParam = $"""
                Find Relevant Information to meet {disclosureName} disclosure requirement.
                The Requirement is {disclosure_requirement} and Requirement Detail is {disclosure_requirement_detail}.
                And The relevant Information should contains similar meaning like this annex description : {annex}.
                The response MUST contains quantitative / Numeric information. If it contains key indicators, YOU MUST SHOW THE INDICATOR's VALUES IN TABLE FORMAT.
                To render the table, you should add Table format after adding "\n\n".
                The response length shouldn't be over {sizeOfChars} characters at maximum.
                Every response MUST INCLUDE CITATION FOR MARKDOWN FORMAT DON'T ADD RELEVANCE SCORE BUT ONLY FILENAME : [filename](filename)[PageNumber].
                CITATION should be described with its statement.
                Just in case you can't find any relevant information, please Show company name in [Company Name] section and write "No information in this document" in [Information] section.
                The output MUST to be in the following Markdown(md) format:
                ## [Company Name placeholder]
                [You should show the relevant Information in here]
                """;

            var response = await memory.AskAsync(inputParam, filter: MemoryFilters.ByDocument(documentId), minRelevance: 10);
            response.Result = response.Result.Replace("\n\n", "    \n").Replace("\n", "  \n");
            return response;
        }
    }

}
