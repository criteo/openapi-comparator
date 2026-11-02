// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using Microsoft.OpenApi;

namespace Criteo.OpenApi.Comparator.Comparators.Extensions
{
    internal static class OpenApiParameterExtension
    {
        internal static bool IsConstant(this IOpenApiParameter parameter) =>
            parameter.IsRequired() && parameter.HasEnumWithSingleValue();

        internal static bool IsRequired(this IOpenApiParameter parameter) =>
            parameter.Required || parameter.In == ParameterLocation.Path;

        private static bool HasEnumWithSingleValue(this IOpenApiParameter parameter) =>
            parameter.Schema is OpenApiSchema { Enum.Count: 1 };
    }
}
