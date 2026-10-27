// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Globalization;
using System.IO;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Newtonsoft.Json.Linq;

namespace Criteo.OpenApi.Comparator.Parser
{
    /// <summary>
    /// Converts a swagger into a C# object
    /// </summary>
    internal static class OpenApiParser
    {
        /// <param name="openApiDocumentAsString">Swagger as string</param>
        /// <param name="diagnostic"></param>
        internal static JsonDocument<OpenApiDocument> Parse(string openApiDocumentAsString, out OpenApiDiagnostic diagnostic)
        {
            var settings = new OpenApiReaderSettings();
            settings.AddYamlReader();
            var result = OpenApiDocument.Parse(openApiDocumentAsString, settings: settings);
            diagnostic = result.Diagnostic ?? new OpenApiDiagnostic();
            var openApiDocument = result.Document
                ?? throw new OpenApiException("Unable to parse the OpenAPI document: " +
                    string.Join("; ", diagnostic.Errors));

            using var textWriter = new StringWriter(CultureInfo.InvariantCulture);
            var openApiWriter = new OpenApiJsonWriter(textWriter);
            openApiDocument.SerializeAsV3(openApiWriter);
            var openApiDocumentAsJson = textWriter.ToString();
            var parsedJson = JToken.Parse(openApiDocumentAsJson);

            return parsedJson.ToJsonDocument(openApiDocument);
        }
    }
}
