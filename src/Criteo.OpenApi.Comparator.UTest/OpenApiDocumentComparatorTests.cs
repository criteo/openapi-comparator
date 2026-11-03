// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using Criteo.OpenApi.Comparator.Comparators;
using Criteo.OpenApi.Comparator.Parser;
using Microsoft.OpenApi.Models;
using NUnit.Framework;

namespace Criteo.OpenApi.Comparator.UTest
{
    [TestFixture]
    public class OpenApiDocumentComparatorTests
    {
        private const string SpecWithPathAndComponents =
@"openapi: 3.0.0
info:
  title: t
  version: ""1.0""
paths:
  /pets:
    get:
      responses:
        ""200"":
          description: ok
components:
  parameters: {}
  responses: {}
";

        [Test]
        public void Compare_DoesNotThrow_WhenPathParametersAreNull()
        {
            var (context, oldDoc, newDoc) = CreateDocuments();
            oldDoc.Paths["/pets"].Parameters = null;
            newDoc.Paths["/pets"].Parameters = null;

            Assert.That(() => CreateComparator().Compare(context, oldDoc, newDoc), Throws.Nothing);
        }

        [Test]
        public void Compare_DoesNotThrow_WhenPathOperationsAreNull()
        {
            var (context, oldDoc, newDoc) = CreateDocuments();
            oldDoc.Paths["/pets"].Operations = null;
            newDoc.Paths["/pets"].Operations = null;

            Assert.That(() => CreateComparator().Compare(context, oldDoc, newDoc), Throws.Nothing);
        }

        [Test]
        public void Compare_DoesNotThrow_WhenComponentSchemasAreNull()
        {
            var (context, oldDoc, newDoc) = CreateDocuments();
            oldDoc.Components.Schemas = null;
            newDoc.Components.Schemas = null;

            Assert.That(() => CreateComparator().Compare(context, oldDoc, newDoc), Throws.Nothing);
        }

        private static OpenApiDocumentComparator CreateComparator() => new OpenApiDocumentComparator();

        private static (ComparisonContext Context, OpenApiDocument Old, OpenApiDocument New) CreateDocuments()
        {
            var oldJson = OpenApiParser.Parse(SpecWithPathAndComponents, out _);
            var newJson = OpenApiParser.Parse(SpecWithPathAndComponents, out _);
            return (new ComparisonContext(oldJson, newJson), oldJson.Typed, newJson.Typed);
        }
    }
}
