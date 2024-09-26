// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.KernelMemory.MongoDbAtlas;

public class MongoDbAtlasException : KernelMemoryException
{
    /// <inheritdoc />
    public MongoDbAtlasException() { }

    /// <inheritdoc />
    public MongoDbAtlasException(string message) : base(message) { }

    /// <inheritdoc />
    public MongoDbAtlasException(string message, Exception? innerException) : base(message, innerException) { }
}
