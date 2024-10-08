// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;

namespace Postgres.UnitTests;

public class PostgresConfigTests
{
    [Fact]
    [Trait("Category", "UnitTest")]
    [Trait("Category", "Postgres")]
    public void ItRequiresOnlyAConnStringToBeValid()
    {
        // Arrange
        var config1 = new PostgresConfig();
        var config2 = new PostgresConfig { ConnectionString = "test string" };

        // Act - Assert exception occurs
        Assert.Throws<ConfigurationException>(() => config1.Validate());

        // Act - Assert no exception occurs
        config2.Validate();
    }
}
