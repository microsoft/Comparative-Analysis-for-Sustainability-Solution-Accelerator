// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.KernelMemory.Pipeline;

public class OrchestrationException : KernelMemoryException
{
    /// <inheritdoc />
    public OrchestrationException() { }

    /// <inheritdoc />
    public OrchestrationException(string message) : base(message) { }

    /// <inheritdoc />
    public OrchestrationException(string message, Exception? innerException) : base(message, innerException) { }
}
