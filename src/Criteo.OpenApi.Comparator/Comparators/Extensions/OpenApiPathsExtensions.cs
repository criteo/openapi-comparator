// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace Criteo.OpenApi.Comparator.Comparators.Extensions
{
    internal static class OpenApiPathsExtensions
    {
        internal static OpenApiPaths ToOpenApiPaths(this JsonObject rawPaths, OpenApiDocument document)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(rawPaths.ToJsonString()));
            var paths = OpenApiModelFactory.Load<OpenApiPaths>(
                input: stream,
                version: OpenApiSpecVersion.OpenApi3_0,
                format: OpenApiConstants.Json,
                openApiDocument: document,
                diagnostic: out _);
            return paths ?? [];
        }
    }
}
