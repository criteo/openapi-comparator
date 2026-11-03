// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using Criteo.OpenApi.Comparator.Comparators;
using Criteo.OpenApi.Comparator.Parser;
using Microsoft.OpenApi.Models;
using NUnit.Framework;

namespace Criteo.OpenApi.Comparator.UTest
{
    [TestFixture]
    public class OperationComparatorTests
    {
        private const string Spec =
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
";

        [Test]
        public void Compare_DoesNotThrow_WhenOperationParametersAreNull()
        {
            var oldJson = OpenApiParser.Parse(Spec, out _);
            var newJson = OpenApiParser.Parse(Spec, out _);
            var context = new ComparisonContext(oldJson, newJson);
            var oldOperation = oldJson.Typed.Paths["/pets"].Operations[OperationType.Get];
            var newOperation = newJson.Typed.Paths["/pets"].Operations[OperationType.Get];
            oldOperation.Parameters = null;
            newOperation.Parameters = null;
            context.PushProperty("paths");
            context.PushPathProperty("/pets", false);
            context.PushProperty("get");

            Assert.That(() => CreateComparator().Compare(context, oldOperation, newOperation), Throws.Nothing);
        }

        private static OperationComparator CreateComparator()
        {
            var schemaComparator = new SchemaComparator();
            var contentComparator = new ContentComparator(schemaComparator);
            var parameterComparator = new ParameterComparator(schemaComparator, contentComparator);
            var requestBodyComparator = new RequestBodyComparator(contentComparator);
            var responseComparator = new ResponseComparator(contentComparator);
            return new OperationComparator(parameterComparator, requestBodyComparator, responseComparator);
        }
    }
}
