// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.KernelMemory.FileSystem.DevTools;

/// <summary>
/// The type of storage to use.
/// </summary>
public enum FileSystemTypes
{
    /// <summary>
    /// Save data to disk.
    /// </summary>
    Disk,

    /// <summary>
    /// Save data to memory.
    /// </summary>
    Volatile,
}
