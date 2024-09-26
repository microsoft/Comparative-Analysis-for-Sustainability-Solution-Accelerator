// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.KernelMemory;

public class KernelMemoryWebException : KernelMemoryException
{
    /// <inheritdoc />
    public KernelMemoryWebException()
    {
    }

    /// <inheritdoc />
    public KernelMemoryWebException(string? message) : base(message)
    {
    }

    /// <inheritdoc />
    public KernelMemoryWebException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
