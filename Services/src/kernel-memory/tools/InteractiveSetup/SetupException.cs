﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.KernelMemory.InteractiveSetup;

public class SetupException : Exception
{
    /// <inheritdoc />
    public SetupException()
    {
    }

    /// <inheritdoc />
    public SetupException(string? message) : base(message)
    {
    }

    /// <inheritdoc />
    public SetupException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
