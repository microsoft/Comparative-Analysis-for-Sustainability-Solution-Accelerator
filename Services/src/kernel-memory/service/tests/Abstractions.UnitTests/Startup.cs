﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

/* IMPORTANT: the Startup class must be at the root of the namespace and
 * the namespace must match exactly (required by Xunit.DependencyInjection) */

using Microsoft.Extensions.Hosting;

namespace Abstractions.UnitTests;

public class Startup
{
    // ReSharper disable once UnusedMember.Global
    public void ConfigureHost(IHostBuilder hostBuilder)
    {
    }
}
