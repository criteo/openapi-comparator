// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.IO;
using System.Reflection;
using Criteo.OpenApi.Comparator.Parser;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers.Exceptions;
using NUnit.Framework;

namespace Criteo.OpenApi.Comparator.UTest
{
    [TestFixture]
    public class OpenApiParserTests
    {
        private static string ReadOpenApiFile(string fileName)
        {
            var baseDir = Directory.GetParent(typeof(OpenApiParserTests).GetTypeInfo().Assembly.Location)
                .ToString();
            var filePath = Path.Combine(baseDir, "Resource", fileName);
            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// Verifies that the Parser throws an Exception when an in valid json in given
        /// </summary>
        [Test]
        public void OpenApiParser_Should_Throw_Exception_When_Invalid_Json()
        {
            const string fileName = "invalid_json_file.txt";
            var documentAsString = ReadOpenApiFile(fileName);
            Assert.Throws<OpenApiUnsupportedSpecVersionException>(() => OpenApiParser.Parse(documentAsString, out _));
        }

        /// <summary>
        /// Verifies that a valid JsonDocument object is parsed when input is a valid OpenApi
        /// </summary>
        [TestCase("openapi_specification.json")]
        [TestCase("openapi_specification.yaml")]
        public void OpenApiParser_Should_Return_Valid_OpenApi_Document_Object(string fileName)
        {
            var documentAsString = ReadOpenApiFile(fileName);
            var validOpenApiDocument = OpenApiParser.Parse(documentAsString, out _);
            Assert.IsInstanceOf<JsonDocument<OpenApiDocument>>(validOpenApiDocument);
        }
    }
}
