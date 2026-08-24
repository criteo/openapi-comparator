// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using NUnit.Framework;

namespace Criteo.OpenApi.Comparator.UTest
{
    [TestFixture]
    public class UniqueItemsComparisonTests
    {
        [Test]
        public void Compare_DoesNotReportUniqueItemsChange_WhenOmittedVersusExplicitFalse()
        {
            var omitted = ArraySchemaSpec(uniqueItems: null);
            var explicitFalse = ArraySchemaSpec(uniqueItems: false);

            var omittedToFalse = OpenApiComparator.Compare(omitted, explicitFalse, out _);
            var falseToOmitted = OpenApiComparator.Compare(explicitFalse, omitted, out _);

            Assert.That(omittedToFalse, Has.None.Matches<ComparisonMessage>(IsUniqueItemsConstraintChanged));
            Assert.That(falseToOmitted, Has.None.Matches<ComparisonMessage>(IsUniqueItemsConstraintChanged));
        }

        [Test]
        public void Compare_ReportsUniqueItemsChange_WhenOmittedVersusTrue()
        {
            var omitted = ArraySchemaSpec(uniqueItems: null);
            var explicitTrue = ArraySchemaSpec(uniqueItems: true);

            var differences = OpenApiComparator.Compare(omitted, explicitTrue, out _);

            Assert.That(differences, Has.Some.Matches<ComparisonMessage>(IsUniqueItemsConstraintChanged));
        }

        private static bool IsUniqueItemsConstraintChanged(ComparisonMessage message) =>
            message.Code == nameof(ComparisonRules.ConstraintChanged)
            && message.Message.Contains("uniqueItems");

        private static string ArraySchemaSpec(bool? uniqueItems)
        {
            var uniqueItemsLine = uniqueItems.HasValue
                ? "              uniqueItems: " + (uniqueItems.Value ? "true" : "false") + "\n"
                : string.Empty;

            return
@"openapi: 3.0.0
info:
  title: t
  version: ""1.0""
paths:
  /test:
    post:
      requestBody:
        content:
          application/json:
            schema:
              type: array
" + uniqueItemsLine +
@"              items:
                type: string
      responses:
        ""200"":
          description: ok
";
        }
    }
}
