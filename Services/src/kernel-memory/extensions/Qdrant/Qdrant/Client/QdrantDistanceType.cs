﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.KernelMemory.MemoryDb.Qdrant.Client;

/// <summary>
/// The vector distance type used by Qdrant.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
internal enum QdrantDistanceType
{
    /// <summary>
    /// Cosine of the angle between vectors, aka dot product scaled by magnitude. Cares only about angle difference.
    /// </summary>
    Cosine,

    /// <summary>
    /// Consider angle and distance (magnitude) of vectors.
    /// </summary>
    DotProduct,

    /// <summary>
    /// Pythagorean(theorem) distance of 2 multidimensional points.
    /// </summary>
    Euclidean,

    /// <summary>
    /// Sum of absolute differences between points across all the dimensions.
    /// </summary>
    Manhattan,

    /// <summary>
    /// Assuming only the most significant dimension is relevant.
    /// </summary>
    Chebyshev,

    /// <summary>
    /// Generalization of Euclidean and Manhattan.
    /// </summary>
    Minkowski,
}
