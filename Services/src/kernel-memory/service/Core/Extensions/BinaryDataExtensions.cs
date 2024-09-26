// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Cryptography;

namespace Microsoft.KernelMemory.Extensions;

public static class BinaryDataExtensions
{
    public static string CalculateSHA256(this BinaryData binaryData)
    {
        byte[] byteArray = SHA256.HashData(binaryData.ToMemory().Span);
        return Convert.ToHexString(byteArray).ToLowerInvariant();
    }
}
