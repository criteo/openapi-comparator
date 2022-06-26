// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Criteo.OpenApiDiff.Comparator.Comparators;
using Microsoft.OpenApi.Models;
using Criteo.OpenApiDiff.Core;

namespace Criteo.OpenApiDiff.Comparator
{
    /// <summary>
    /// Comparator base class
    /// </summary>
    public class OpenApiComparator
    {
        /// <summary>
        /// Compares two versions of the same service specification.
        /// </summary>
        /// <param name="oldFileName">a file name of the old swagger document</param>
        /// <param name="oldOpenApiSpec">a content of the old swagger document</param>
        /// <param name="newFileName">a file name of the new swagger document</param>
        /// <param name="newOpenApiSpec">a content of the new swagger document</param>
        /// <param name="settings">options retrieved from command line</param>
        public IEnumerable<ComparisonMessage> Compare(
            string oldFileName,
            string oldOpenApiSpec,
            string newFileName,
            string newOpenApiSpec,
            Settings settings = null)
        {
            var oldOpenApiDocument = OpenApiParser.Parse(oldOpenApiSpec, oldFileName, settings);
            var newOpenApiDocument = OpenApiParser.Parse(newOpenApiSpec, newFileName, settings);

            var context = new ComparisonContext<OpenApiDocument>(oldOpenApiDocument, newOpenApiDocument, settings);

            var comparator = new OpenApiSpecComparator();
            var comparisonMessages = comparator.Compare(context, oldOpenApiDocument.Typed, newOpenApiDocument.Typed);

            return comparisonMessages;
        }
    }
}
