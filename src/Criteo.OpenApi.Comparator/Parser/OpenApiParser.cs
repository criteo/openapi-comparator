// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using Newtonsoft.Json.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;
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
            var openApiReaderSettings = new OpenApiReaderSettings
            {
                ReferenceResolution = ReferenceResolutionSetting.DoNotResolveReferences
            };
            var openApiReader = new OpenApiStringReader(openApiReaderSettings);
            var openApiDocument = openApiReader.Read(openApiDocumentAsString, out diagnostic);

            var textWriter = new StringWriter();
            var openApiWriter = new OpenApiJsonWriter(textWriter);
            openApiDocument.SerializeAsV3(openApiWriter);
            var openApiDocumentAsJson = textWriter.ToString();
            var parsedJson = JToken.Parse(openApiDocumentAsJson);

            return parsedJson.ToJsonDocument(openApiDocument);
        }
    }
}
