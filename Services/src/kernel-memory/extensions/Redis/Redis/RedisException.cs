// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.KernelMemory.MemoryDb.Redis;

public class RedisException : KernelMemoryException
{
    /// <inheritdoc />
    public RedisException() { }

    /// <inheritdoc />
    public RedisException(string message) : base(message) { }

    /// <inheritdoc />
    public RedisException(string message, Exception? innerException) : base(message, innerException) { }
}
