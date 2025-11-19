// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFS.SK.Sustainability.AI.Utils
{
    internal static class HtmlPdfConverter
    {
        public static bool Convert(string sourceHtmlFilePath, string targetPdfFilePath)
        {
            // Validate file paths to prevent command injection
            ValidateFilePath(sourceHtmlFilePath);
            ValidateFilePath(targetPdfFilePath);

            var escapedSourceHtmlFilePath = sourceHtmlFilePath.Replace("\"", "\\\"");
            var escapedTargetPdfFilePath = targetPdfFilePath.Replace("\"", "\\\"");
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = IsWindows() ? "wkhtmltopdf.exe" : "/usr/bin/wkhtmltopdf",
                    Arguments = $"--encoding UTF-8 -q \"{escapedSourceHtmlFilePath}\" \"{escapedTargetPdfFilePath}\"",// CodeQL [SM02383]  File paths are validated by ValidateFilePath() to prevent command injection
                    RedirectStandardInput = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.ErrorDataReceived += (process, data) =>
            {
                if (data.Data == null) return;
                Console.Error.WriteLine(data.Data);
            };

            process.OutputDataReceived += (process, data) =>
            {
                Console.WriteLine(data.Data);
            };

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit(new TimeSpan(0, 1, 0));

            return process.ExitCode == 0;
        }


        private static bool IsWindows()
        {
            return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        }

        // Simple validation to prevent command injection
        private static void ValidateFilePath(string path)
        {
            if (string.IsNullOrEmpty(path)) 
                throw new ArgumentException("Invalid path");
            
            // Block command injection characters
            if (path.Any(c => ";|&`$<>\n\r".Contains(c)))
                throw new ArgumentException("Path contains dangerous characters");
        }
    }
}
