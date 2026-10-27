// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace Criteo.OpenApi.Comparator.Comparators.Extensions
{
    internal static class OpenApiV2Extensions
    {
        internal static BaseOpenApiReference GetReference(this object referenceable)
        {
            return referenceable switch
            {
                OpenApiSchemaReference reference => reference.Reference,
                OpenApiParameterReference reference => reference.Reference,
                OpenApiRequestBodyReference reference => reference.Reference,
                OpenApiResponseReference reference => reference.Reference,
                OpenApiHeaderReference reference => reference.Reference,
                OpenApiPathItemReference reference => reference.Reference,
                _ => null,
            };
        }

        internal static IOpenApiSchema ResolveTarget(this IOpenApiSchema schema, System.Collections.Generic.IDictionary<string, IOpenApiSchema> schemas)
        {
            if (schema is OpenApiSchemaReference reference)
            {
                return reference.RecursiveTarget ?? reference.Target ?? reference.GetReference().Resolve(schemas);
            }

            return schema;
        }

        internal static IOpenApiParameter ResolveTarget(this IOpenApiParameter parameter, System.Collections.Generic.IDictionary<string, IOpenApiParameter> parameters)
        {
            if (parameter is OpenApiParameterReference reference)
            {
                return reference.RecursiveTarget ?? reference.Target ?? reference.GetReference().Resolve(parameters);
            }

            return parameter;
        }

        internal static IOpenApiResponse ResolveTarget(this IOpenApiResponse response, System.Collections.Generic.IDictionary<string, IOpenApiResponse> responses)
        {
            if (response is OpenApiResponseReference reference)
            {
                return reference.RecursiveTarget ?? reference.Target ?? reference.GetReference().Resolve(responses);
            }

            return response;
        }

        internal static IOpenApiRequestBody ResolveTarget(this IOpenApiRequestBody requestBody, System.Collections.Generic.IDictionary<string, IOpenApiRequestBody> requestBodies)
        {
            if (requestBody is OpenApiRequestBodyReference reference)
            {
                return reference.RecursiveTarget ?? reference.Target ?? reference.GetReference().Resolve(requestBodies);
            }

            return requestBody;
        }

        internal static JsonNode AsJsonNode(this IOpenApiExtension extension)
        {
            return extension is JsonNodeExtension jsonNodeExtension ? jsonNodeExtension.Node : null;
        }

        internal static string JsonString(this JsonNode node)
        {
            return node is JsonValue value && value.TryGetValue<string>(out var result) ? result : null;
        }

        internal static bool? JsonBoolean(this JsonNode node)
        {
            return node is JsonValue value && value.TryGetValue<bool>(out var result) ? result : null;
        }
    }
}
