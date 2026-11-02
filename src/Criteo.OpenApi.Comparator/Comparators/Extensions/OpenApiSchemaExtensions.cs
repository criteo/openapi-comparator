// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace Criteo.OpenApi.Comparator.Comparators.Extensions
{
    internal static class OpenApiSchemaExtensions
    {
        internal static bool IsPropertyRequired(this IOpenApiSchema schema, string propertyName) =>
            schema.Required != null && schema.Required.Contains(propertyName);

        internal static string StringValue(this JsonNode value) => value?.ToString() ?? "null";
    }
}
