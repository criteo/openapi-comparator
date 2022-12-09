// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Criteo.OpenApi.Comparator.Parser
{
    /// <summary>
    /// Converts a swagger into a C# object
    /// </summary>
    internal static class OpenApiParser
    {
        /// <param name="openApiDocumentAsString">Swagger as string</param>
        /// <param name="fileName">Name of the swagger file</param>
        internal static JsonDocument<OpenApiDocument> Parse(string openApiDocumentAsString, string fileName)
        {
            var settings = new JsonSerializerSettings
            {
                MaxDepth = 128,
                TypeNameHandling = TypeNameHandling.None,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore
            };
            settings.Converters.Add(new PathLevelParameterConverter(openApiDocumentAsString));

            var raw = JToken.Parse(openApiDocumentAsString);

            var openApiReaderSettings = new OpenApiReaderSettings
            {
                ReferenceResolution = ReferenceResolutionSetting.DoNotResolveReferences
            };
            var openApiReader = new OpenApiStringReader(openApiReaderSettings);
            var openApiDocument = openApiReader.Read(openApiDocumentAsString, out _);

            return raw.ToJsonDocument(openApiDocument, fileName);
        }
    }
}
