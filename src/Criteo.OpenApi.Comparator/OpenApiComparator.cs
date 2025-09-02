// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Criteo.OpenApi.Comparator.Comparators;
using Criteo.OpenApi.Comparator.Logging;
using Criteo.OpenApi.Comparator.Parser;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator;

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
    /// <param name="comparisonMessages">Result Details</param>
    /// <returns>The severity of the changes.</returns>
    public static ChangeLevel Compare(
        out List<ComparisonMessage> comparisonMessages,
        string oldOpenApiSpec,
        string newOpenApiSpec)
    {
        var oldOpenApiDocument = OpenApiParser.Parse(oldOpenApiSpec, out var oldSpecDiagnostic);
        var newOpenApiDocument = OpenApiParser.Parse(newOpenApiSpec, out var newSpecDiagnostic);
        var context = new ComparisonContext(oldOpenApiDocument, newOpenApiDocument);

        var comparator = new OpenApiDocumentComparator();
        comparisonMessages = comparator.Compare(context, oldOpenApiDocument.Typed, newOpenApiDocument.Typed);
        comparisonMessages.AddRange(oldSpecDiagnostic.Errors.Select(item => new ComparisonMessage(item)));
        comparisonMessages.AddRange(newSpecDiagnostic.Errors.Select(item => new ComparisonMessage(item)));

        if (!comparisonMessages.Any())
            return ChangeLevel.None;

        var maxSeverity = comparisonMessages.Max(s => s.Severity);

        return maxSeverity switch
        {
            MessageSeverity.Error => ChangeLevel.Error,
            MessageSeverity.Breaking => ChangeLevel.Error,
            MessageSeverity.Warning => ChangeLevel.Warning,
            MessageSeverity.Info => ChangeLevel.Info,
            _ => ChangeLevel.Warning
        };
    }

    /// <summary>
    /// Compares two OpenAPI specification.
    /// </summary>
    /// <param name="oldOpenApiSpec">The content of the old OpenAPI Specification</param>
    /// <param name="newOpenApiSpec">The content of the new OpenAPI Specification</param>
    /// <param name="parsingErrors">Parsing errors</param>
    /// <param name="strict">If true, then breaking changes are errors instead of warnings.</param>
    [Obsolete(
        "Use new compare which returns severity and parsing errors as a comparison message. Also strict is true by default")]
    public static IEnumerable<ComparisonMessage> Compare(
        string oldOpenApiSpec,
        string newOpenApiSpec,
        out IEnumerable<ParsingError> parsingErrors,
        bool strict = false)
    {
        var oldOpenApiDocument = OpenApiParser.Parse(oldOpenApiSpec, out var oldSpecDiagnostic);
        var newOpenApiDocument = OpenApiParser.Parse(newOpenApiSpec, out var newSpecDiagnostic);

        parsingErrors = oldSpecDiagnostic.Errors
            .Select(e => new ParsingError("old", e))
            .Concat(newSpecDiagnostic.Errors.Select(e => new ParsingError("new", e)));

        var context = new ComparisonContext(oldOpenApiDocument, newOpenApiDocument);

        var comparator = new OpenApiDocumentComparator();
        var comparisonMessages = comparator.Compare(context, oldOpenApiDocument.Typed, newOpenApiDocument.Typed);

        if (comparisonMessages.Any(c =>
                c.Code == ComparisonRules.NoVersionChange.Code))
            strict = true;

        // If it was a patch version run in strict
        if (comparisonMessages.All(c =>
                c.Code != ComparisonRules.MajorVersionChange.Code &&
                c.Code != ComparisonRules.MinorVersionChange.Code
                ))
            strict = true;

        comparisonMessages.RemoveAll(c =>
            c.Code == ComparisonRules.MajorVersionChange.Code ||
            c.Code == ComparisonRules.MinorVersionChange.Code
        );


        comparisonMessages.ForEach(c =>
        {
            if (c.Severity == MessageSeverity.Breaking)
                c.Severity = strict ? MessageSeverity.Error : MessageSeverity.Warning;
        });

        return comparisonMessages;
    }
}

/// <summary>
/// Represents an error that occurred while parsing an OpenAPI document.
/// </summary>
[Obsolete]
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
    public override string ToString()
    {
        return $"[{_documentName}] {_error}";
    }
}