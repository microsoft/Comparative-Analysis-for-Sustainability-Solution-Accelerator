// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Microsoft.KernelMemory;
using Microsoft.KernelMemory.ContentStorage;
using Microsoft.KernelMemory.ContentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.Extensions.DependencyInjection;

public static partial class KernelMemoryBuilderExtensions
{
    public static IKernelMemoryBuilder WithSimpleFileStorage(this IKernelMemoryBuilder builder, SimpleFileStorageConfig? config = null)
    {
        builder.Services.AddSimpleFileStorageAsContentStorage(config ?? new SimpleFileStorageConfig());
        return builder;
    }

    public static IKernelMemoryBuilder WithSimpleFileStorage(this IKernelMemoryBuilder builder, string directory)
    {
        builder.Services.AddSimpleFileStorageAsContentStorage(directory);
        return builder;
    }
}

public static partial class DependencyInjection
{
    public static IServiceCollection AddSimpleFileStorageAsContentStorage(this IServiceCollection services, SimpleFileStorageConfig config)
    {
        return services
            .AddSingleton<SimpleFileStorageConfig>(config)
            .AddSingleton<IContentStorage, SimpleFileStorage>();
    }

    public static IServiceCollection AddSimpleFileStorageAsContentStorage(this IServiceCollection services, string directory)
    {
        var config = new SimpleFileStorageConfig { StorageType = FileSystemTypes.Disk, Directory = directory };
        return services.AddSimpleFileStorageAsContentStorage(config);
    }
}
