// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.IO;
using System.Linq;
using System.Reflection;
using Criteo.OpenApi.Comparator.Logging;
using NUnit.Framework;

namespace Criteo.OpenApi.Comparator.UTest
{
    [TestFixture]
    public class OpenApiComparatorTests
    {

        private static string ReadOpenApiFile(string fileName)
        {
            var baseDir = Directory.GetParent(typeof(OpenApiParserTests).GetTypeInfo().Assembly.Location)
                .ToString();
            var filePath = Path.Combine(baseDir, "Resource", fileName);
            return File.ReadAllText(filePath);
        }


        /// <summary>
        ///     Info NoVersionChange message should not be coming back
        /// </summary>
        [Test]
        public void OpenApiComparator_WithStrict_ShouldNotHaveVersionWarning()
        {
            var oldYaml = ReadOpenApiFile("added_required_parameter/old.yaml");
            var newYaml = ReadOpenApiFile("added_required_parameter/new.yaml");

            var results = OpenApiComparator.Compare(oldYaml, newYaml, out _, true);

            Assert.That(results.All(r => r.Id != ComparisonRules.NoVersionChange.Id),
                "NoVersionChange warning (Id 1001) should not be present in the results.");

        }
    }
}
