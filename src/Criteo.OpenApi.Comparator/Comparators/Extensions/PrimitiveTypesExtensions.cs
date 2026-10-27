// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;

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

        internal static bool DifferFrom(this JsonNode oldOpenApiAny, JsonNode newOpenApiAny)
        {
            if (oldOpenApiAny == null || newOpenApiAny == null)
                return true;

            return !JsonNode.DeepEquals(oldOpenApiAny, newOpenApiAny);
        }
    }
}
