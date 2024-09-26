// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.KernelMemory.Prompts;

public interface IPromptProvider
{
    /// <summary>
    /// Return a prompt content
    /// </summary>
    /// <param name="promptName">Prompt name</param>
    /// <returns>Prompt string</returns>
    public string ReadPrompt(string promptName);
}
