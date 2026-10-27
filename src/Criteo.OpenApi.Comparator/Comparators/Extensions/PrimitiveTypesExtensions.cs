// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Text.Json.Nodes;

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

        internal static bool DifferFrom(this JsonNode oldValue, JsonNode newValue) =>
            !JsonNode.DeepEquals(oldValue, newValue);
    }
}
