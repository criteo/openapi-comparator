// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

#if NET10_0
using Microsoft.OpenApi;
using IOpenApiAny = System.Text.Json.Nodes.JsonNode;
using OpenApiSchema = Microsoft.OpenApi.IOpenApiSchema;
#else
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
#endif

namespace Criteo.OpenApi.Comparator.Comparators.Extensions
{
    internal static class OpenApiSchemaExtensions
    {
        internal static bool IsPropertyRequired(this OpenApiSchema schema, string propertyName) =>
            schema.Required != null && schema.Required.Contains(propertyName);

        internal static string StringValue(this IOpenApiAny openApiAny) =>
#if NET10_0
            openApiAny?.ToString();
#else
            openApiAny switch
            {
                OpenApiString openApiString => openApiString.Value,
                _ => openApiAny.ToString(),
            };
#endif
    }
}
