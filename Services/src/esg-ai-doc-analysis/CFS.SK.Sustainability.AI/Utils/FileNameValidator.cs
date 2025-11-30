// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.RegularExpressions;

namespace CFS.SK.Sustainability.AI.Utils
{
    public static class FileNameValidator
    {
        // Regex to validate file names (only allows letters, digits, dot, dash, underscore)
        private static readonly Regex ValidFileNameRegex = 
            new Regex(@"^[A-Za-z0-9._\-]+$", RegexOptions.Compiled);

        /// <summary>
        /// Validates a simple file name to prevent path traversal attacks.
        /// Returns the validated filename to help static analysis tools track sanitization.
        /// </summary>
        /// <param name="name">The file name to validate</param>
        /// <returns>The validated file name</returns>
        /// <exception cref="ArgumentException">Thrown when the file name is invalid</exception>
        public static string ValidateAndReturnSafeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("File name is empty or null.");

            if (name.Contains("..") || name.Contains("/") || name.Contains("\\"))
                throw new ArgumentException("Invalid file name (contains path components or traversal).");
            
            // Whitelist chars: letters, digits, dash, underscore, dot
            if (!ValidFileNameRegex.IsMatch(name))
                throw new ArgumentException("Invalid file name.");

            return name;
        }
    }
}
