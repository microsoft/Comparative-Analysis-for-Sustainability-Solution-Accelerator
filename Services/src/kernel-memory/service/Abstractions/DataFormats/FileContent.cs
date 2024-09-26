// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.KernelMemory.DataFormats;

public class FileContent
{
    [JsonPropertyOrder(0)]
    [JsonPropertyName("sections")]
    public List<FileSection> Sections { get; set; } = new();
}
