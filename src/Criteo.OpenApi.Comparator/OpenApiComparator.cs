// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Criteo.OpenApi.Comparator.Comparators;
using Criteo.OpenApi.Comparator.Parser;

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
        /// <param name="strict">If true, then breaking changes are errors instead of warnings.</param>
        public static IEnumerable<ComparisonMessage> Compare(string oldOpenApiSpec, string newOpenApiSpec, bool strict = false)
        {
            var oldOpenApiDocument = OpenApiParser.Parse(oldOpenApiSpec);
            var newOpenApiDocument = OpenApiParser.Parse(newOpenApiSpec);

            var context = new ComparisonContext(oldOpenApiDocument, newOpenApiDocument) { Strict = strict };

            var comparator = new OpenApiDocumentComparator();
            var comparisonMessages = comparator.Compare(context, oldOpenApiDocument.Typed, newOpenApiDocument.Typed);

            return comparisonMessages;
        }
    }
}
