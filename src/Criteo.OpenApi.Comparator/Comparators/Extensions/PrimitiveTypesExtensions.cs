// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System;
#if NET10_0
using Microsoft.OpenApi;
using System.Text.Json.Nodes;
using IOpenApiAny = System.Text.Json.Nodes.JsonNode;
#else
using Microsoft.OpenApi.Any;
#endif

namespace Criteo.OpenApi.Comparator.Comparators.Extensions
{
    internal static class PrimitiveTypesExtensions
    {
        internal static bool DifferFrom(this string oldString, string newString) =>
            oldString == null && newString != null || oldString != null && !oldString.Equals(newString);

        internal static bool DifferFrom(this decimal? oldDecimal, decimal? newDecimal) =>
            oldDecimal == null && newDecimal != null || oldDecimal != null && !oldDecimal.Equals(newDecimal);

        internal static bool DifferFrom(this int? oldDecimal, int? newDecimal) =>
            oldDecimal == null && newDecimal != null || oldDecimal != null && !oldDecimal.Equals(newDecimal);

        private static bool DifferFrom(this bool oldBoolean, bool newBoolean) => oldBoolean != newBoolean;

        private static bool DifferFrom(this float oldFloat, float newFloat) => Math.Abs(oldFloat - newFloat) > float.MinValue;

        private static bool DifferFrom(this DateTime? oldTime, DateTime? newTime) =>
            oldTime == null && newTime != null || oldTime != null && oldTime.Equals(newTime);

#if NET10_0
        internal static bool DifferFrom(this JsonNode oldOpenApiAny, JsonNode newOpenApiAny) =>
            !JsonNode.DeepEquals(oldOpenApiAny, newOpenApiAny);
#else
        internal static bool DifferFrom(this IOpenApiAny oldOpenApiAny, IOpenApiAny newOpenApiAny)
        {
            if (oldOpenApiAny == null || newOpenApiAny == null)
                return true;

            if (oldOpenApiAny.GetType() != newOpenApiAny.GetType())
                return true;

            switch (oldOpenApiAny)
            {
                case OpenApiString oldString:
                    return oldString.Value.DifferFrom((newOpenApiAny as OpenApiString)?.Value);
                case OpenApiInteger oldInteger:
                    return ((int?)oldInteger.Value).DifferFrom((newOpenApiAny as OpenApiInteger)?.Value);
                case OpenApiFloat oldFloat:
                    return (oldFloat.Value).DifferFrom(((OpenApiFloat) newOpenApiAny).Value);
                case OpenApiDate oldDate:
                    return ((DateTime?) oldDate.Value).DifferFrom(((OpenApiDate) newOpenApiAny).Value);
                case OpenApiBoolean oldBoolean:
                    return oldBoolean.Value.DifferFrom(((OpenApiBoolean) newOpenApiAny).Value);
                default:
                    return false;
            }
        }
#endif
    }
}
