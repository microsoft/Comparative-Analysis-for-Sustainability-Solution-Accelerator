// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Swashbuckle.AspNetCore.SwaggerGen;

namespace CFS.SK.Sustainability.AI.Host
{
    public static class SwaggerExtensions
    {
        public static void UseCustomSchemaIds(this SwaggerGenOptions options)
        {
            options.CustomSchemaIds(type => type.FullName);
        }
    }
}
