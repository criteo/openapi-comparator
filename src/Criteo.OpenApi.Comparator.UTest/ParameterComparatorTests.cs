// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using Criteo.OpenApi.Comparator.Comparators;
using Criteo.OpenApi.Comparator.Parser;
using Microsoft.OpenApi.Models;
using NUnit.Framework;

namespace Criteo.OpenApi.Comparator.UTest
{
    [TestFixture]
    public class ParameterComparatorTests
    {
        private const string Spec =
@"openapi: 3.0.0
info:
  title: t
  version: ""1.0""
paths: {}
";

        [Test]
        public void Compare_DoesNotThrow_WhenComponentsIsNull()
        {
            var oldJson = OpenApiParser.Parse(Spec, out _);
            var newJson = OpenApiParser.Parse(Spec, out _);
            oldJson.Typed.Components = null;
            newJson.Typed.Components = null;
            var schemaComparator = new SchemaComparator();
            var contentComparator = new ContentComparator(schemaComparator);
            var comparator = new ParameterComparator(schemaComparator, contentComparator);
            var context = new ComparisonContext(oldJson, newJson);
            var reference = new OpenApiReference { Type = ReferenceType.Parameter, Id = "missing" };

            Assert.That(
                () => comparator.Compare(
                    context,
                    new OpenApiParameter { Reference = reference },
                    new OpenApiParameter { Reference = reference }),
                Throws.Nothing);
        }
    }
}
