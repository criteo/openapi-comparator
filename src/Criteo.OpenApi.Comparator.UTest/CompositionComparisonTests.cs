// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using NUnit.Framework;

namespace Criteo.OpenApi.Comparator.UTest
{
    [TestFixture]
    public class CompositionComparisonTests
    {
        [Test]
        public void Compare_ReportsDifferentAllOf_WhenInlineAllOfIsAddedOrRemoved()
        {
            var omitted = ObjectSchemaSpec(null);
            var inlineAllOf = ObjectSchemaSpec(
@"                allOf:
                  - type: object
                    properties:
                      id:
                        type: integer
");

            var added = OpenApiComparator.Compare(omitted, inlineAllOf, out _);
            var removed = OpenApiComparator.Compare(inlineAllOf, omitted, out _);

            Assert.That(added, Has.Some.Matches<ComparisonMessage>(m => m.Code == nameof(ComparisonRules.DifferentAllOf)));
            Assert.That(removed, Has.Some.Matches<ComparisonMessage>(m => m.Code == nameof(ComparisonRules.DifferentAllOf)));
        }

        [Test]
        public void Compare_ReportsDifferentOneOf_WhenInlineOneOfIsAddedOrRemoved()
        {
            var omitted = ObjectSchemaSpec(null);
            var inlineOneOf = ObjectSchemaSpec(
@"                oneOf:
                  - type: string
                  - type: integer
");

            var added = OpenApiComparator.Compare(omitted, inlineOneOf, out _);
            var removed = OpenApiComparator.Compare(inlineOneOf, omitted, out _);

            Assert.That(added, Has.Some.Matches<ComparisonMessage>(m => m.Code == nameof(ComparisonRules.DifferentOneOf)));
            Assert.That(removed, Has.Some.Matches<ComparisonMessage>(m => m.Code == nameof(ComparisonRules.DifferentOneOf)));
        }

        [Test]
        public void Compare_DoesNotReportAllOfChange_WhenOmittedVersusEmptyArray()
        {
            var omitted = ObjectSchemaSpec(null);
            var empty = ObjectSchemaSpec("                allOf: []\n");

            var omittedToEmpty = OpenApiComparator.Compare(omitted, empty, out _);
            var emptyToOmitted = OpenApiComparator.Compare(empty, omitted, out _);

            Assert.That(omittedToEmpty, Has.None.Matches<ComparisonMessage>(m => m.Code == nameof(ComparisonRules.DifferentAllOf)));
            Assert.That(emptyToOmitted, Has.None.Matches<ComparisonMessage>(m => m.Code == nameof(ComparisonRules.DifferentAllOf)));
        }

        private static string ObjectSchemaSpec(string extraSchemaLines)
        {
            extraSchemaLines ??= string.Empty;
            return
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
          content:
            application/json:
              schema:
                type: object
" + extraSchemaLines;
        }
    }
}
