// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.KernelMemory.FileSystem.DevTools;

public static class StringExtensions
{
    public static string RemoveBOM(this string x)
    {
        return x.TrimStart('\uFEFF', '\u200B');
    }
}
