using System.IO;
using System.Reflection;
using Criteo.OpenApi.Comparator.Core;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
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
            Assert.Throws<JsonReaderException>(() => OpenApiParser.Parse(documentAsString, fileName, new Settings()));
        }

        /// <summary>
        /// Verifies that a valid JsonDocument object is parsed when input is a valid OpenApi
        /// </summary>
        [Test]
        public void OpenApiParser_Should_Return_Valid_OpenApi_Document_Object()
        {
            const string fileName = "openapi_specification.json";
            var documentAsString = ReadOpenApiFile(fileName);
            var validOpenApiDocument = OpenApiParser.Parse(documentAsString, fileName, new Settings());
            Assert.IsInstanceOf<JsonDocument<OpenApiDocument>>(validOpenApiDocument);
        }
    }
}
