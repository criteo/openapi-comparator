// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators.Extensions
{
    internal static class OpenApiSchemaExtensions
    {
        internal static bool IsPropertyRequired(this OpenApiSchema schema, string propertyName) =>
            schema.Required != null && schema.Required.Contains(propertyName);
    }
}
