// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.KernelMemory.MemoryDb.AzureAISearch;

public class AzureAISearchMemoryException : KernelMemoryException
{
    /// <inheritdoc />
    public AzureAISearchMemoryException()
    {
    }

    /// <inheritdoc />
    public AzureAISearchMemoryException(string? message) : base(message)
    {
    }

    /// <inheritdoc />
    public AzureAISearchMemoryException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
