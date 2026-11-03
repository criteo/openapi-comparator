// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using Newtonsoft.Json.Linq;
#if NET10_0
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.YamlReader;
#else
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;
#endif
using System.IO;

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
#if NET10_0
            var openApiReaderSettings = new OpenApiReaderSettings();
            openApiReaderSettings.AddJsonReader();
            openApiReaderSettings.AddYamlReader();
            var format = openApiDocumentAsString.TrimStart().StartsWith("{") ? "json" : "yaml";
            var readResult = OpenApiDocument.Parse(openApiDocumentAsString, format, openApiReaderSettings);
            diagnostic = readResult.Diagnostic;
            var openApiDocument = readResult.Document;
            if (openApiDocument == null)
                throw new OpenApiException("The OpenAPI document could not be parsed.");
#else
            var openApiReaderSettings = new OpenApiReaderSettings
            {
                ReferenceResolution = ReferenceResolutionSetting.DoNotResolveReferences
            };
            var openApiReader = new OpenApiStringReader(openApiReaderSettings);
            var openApiDocument = openApiReader.Read(openApiDocumentAsString, out diagnostic);
#endif

            var textWriter = new StringWriter();
            var openApiWriter = new OpenApiJsonWriter(textWriter);
            openApiDocument.SerializeAsV3(openApiWriter);
            var openApiDocumentAsJson = textWriter.ToString();
            var parsedJson = JToken.Parse(openApiDocumentAsJson);

            return parsedJson.ToJsonDocument(openApiDocument);
        }
    }
}
