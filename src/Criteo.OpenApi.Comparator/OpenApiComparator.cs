// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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
        /// <param name="parsingErrors">Parsing errors</param>
        /// <param name="strict">If true, then breaking changes are errors instead of warnings. If null it will auto detect based on versions. Major and Minor version changes will not default to strict.</param>
        public static IEnumerable<ComparisonMessage> Compare(
            string oldOpenApiSpec,
            string newOpenApiSpec,
            out IEnumerable<ParsingError> parsingErrors,
            bool? strict = null)
        {
            var oldOpenApiDocument = OpenApiParser.Parse(oldOpenApiSpec, out var oldSpecDiagnostic);
            var newOpenApiDocument = OpenApiParser.Parse(newOpenApiSpec, out var newSpecDiagnostic);

            parsingErrors = oldSpecDiagnostic.Errors
                .Select(e => new ParsingError("old", e))
                .Concat(newSpecDiagnostic.Errors.Select(e => new ParsingError("new", e)));

            var context = new ComparisonContext(oldOpenApiDocument, newOpenApiDocument) { Strict = strict };

            var comparator = new OpenApiDocumentComparator();
            var comparisonMessages = comparator.Compare(context, oldOpenApiDocument.Typed, newOpenApiDocument.Typed);

            return comparisonMessages;
        }
    }

    /// <summary>
    /// Represents an error that occurred while parsing an OpenAPI document.
    /// </summary>
    public class ParsingError
    {
        private readonly string _documentName;
        private readonly OpenApiError _error;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingError"/> class.
        /// </summary>
        /// <param name="documentName"></param>
        /// <param name="error"></param>
        public ParsingError(string documentName, OpenApiError error)
        {
            _documentName = documentName;
            _error = error;
        }

        /// <inheritdoc/>
        public override string ToString() => $"[{_documentName}] {_error}";
    }
}
