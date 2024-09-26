// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig;
using Microsoft.KernelMemory.DataFormats;
using SelectPdf;
namespace CFS.SK.Sustainability.AI.Utils
{
    public class MarkdownHtmlConverter
    {
        public static Byte[] Convert(string markdownString)
        {
            var pipeline = new MarkdownPipelineBuilder()
                                        .UseAdvancedExtensions()
                                        .UseBootstrap()
                                        .UseCitations()
                                        .UseCustomContainers()
                                        .UseDefinitionLists()
                                        .UseDiagrams()
                                        .UseEmojiAndSmiley()
                                        .UseFigures()
                                        .UseFootnotes()
                                        .UseGridTables()
                                        .UseMathematics()
                                        .UseMediaLinks()
                                        .UsePipeTables()
                                        .UseListExtras()
                                        .UseTaskLists()
                                        .UseYamlFrontMatter()
                                        .Build();

            // Convert to HTML
            var html = Markdown.ToHtml(markdownString, pipeline);
            // Get the location where Template is stored. It should be in the same folder as the executable
            var baseFolder = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var relativeFilePath = "Templates/outputHtmlTemplate.txt";
            // Make a real file path with base folder and relative file path
            var filePath = System.IO.Path.Combine(Path.GetDirectoryName(baseFolder), relativeFilePath);
            var template = File.ReadAllText(filePath);
            // Replace the {{content}} placeholder with the generated HTML
            var outputHtml = template.Replace("{{content}}", html);

            //convert outputHtml to Byte[]
            return Encoding.UTF8.GetBytes(outputHtml);


            // Convert HTML to PDF then Save
            //var converter = new HtmlToPdf();

            //// Set Page Margin like Markdown file
            //converter.Options.MarginTop = 60;
            //converter.Options.MarginBottom = 60;
            //converter.Options.MarginLeft = 60;
            //converter.Options.MarginRight = 60;

            //var output = converter.ConvertHtmlString(outputHtml);
            //return output.Save();
        }
    }
}
