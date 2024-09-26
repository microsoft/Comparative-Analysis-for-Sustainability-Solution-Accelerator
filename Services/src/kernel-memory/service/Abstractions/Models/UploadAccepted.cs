// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.KernelMemory;

public class UploadAccepted
{
    [JsonPropertyName("index")]
    [JsonPropertyOrder(1)]
    public string Index { get; set; } = string.Empty;

    [JsonPropertyName("documentId")]
    [JsonPropertyOrder(2)]
    public string DocumentId { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    [JsonPropertyOrder(3)]
    public string Message { get; set; } = string.Empty;
}
