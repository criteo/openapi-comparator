// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Criteo.OpenApi.Comparator.Comparators;
using Criteo.OpenApi.Comparator.Parser;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator
{
    /// <summary>
    /// OpenAPI Comparator base class
    /// </summary>
    public static class OpenApiComparator
    {
        /// <summary>
        /// Compares two OpenAPI specification.
        /// </summary>
        /// <param name="oldOpenApiSpec">The content of the old OpenAPI Specification</param>
        /// <param name="newOpenApiSpec">The content of the new OpenAPI Specification</param>
        public static IEnumerable<ComparisonMessage> Compare(string oldOpenApiSpec, string newOpenApiSpec)
        {
            var oldOpenApiDocument = OpenApiParser.Parse(oldOpenApiSpec);
            var newOpenApiDocument = OpenApiParser.Parse(newOpenApiSpec);

            var context = new ComparisonContext<OpenApiDocument>(oldOpenApiDocument, newOpenApiDocument);

            var comparator = new OpenApiDocumentComparator();
            var comparisonMessages = comparator.Compare(context, oldOpenApiDocument.Typed, newOpenApiDocument.Typed);

            return comparisonMessages;
        }
    }
}
