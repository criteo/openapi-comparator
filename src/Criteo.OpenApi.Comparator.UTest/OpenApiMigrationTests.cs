// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Linq;
using NUnit.Framework;

namespace Criteo.OpenApi.Comparator.UTest;

[TestFixture]
public class OpenApiMigrationTests
{
    private static string Document(string schema) => $$"""
        {
          "openapi": "3.0.3",
          "info": { "title": "Migration", "version": "1.0" },
          "paths": {},
          "components": { "schemas": { "Value": {{schema}} } }
        }
        """;

    [Test]
    public void MissingCollectionsCanBeCompared()
    {
        const string document = """
            { "openapi": "3.0.3", "info": { "title": "Migration", "version": "1.0" },
              "paths": { "/values": { "get": { "responses": { "204": { "description": "Empty" } } } } } }
            """;
        var differences = OpenApiComparator.Compare(document, document, out var errors).ToList();
        Assert.That(errors, Is.Empty);
        Assert.That(differences.Select(message => message.Code), Is.EquivalentTo(new[] { "NoVersionChange" }));
    }

    [TestCase("{\"type\":\"number\",\"maximum\":10,\"exclusiveMaximum\":true}",
              "{\"type\":\"number\",\"maximum\":5,\"exclusiveMaximum\":true}")]
    [TestCase("{\"type\":\"number\",\"minimum\":5,\"exclusiveMinimum\":true}",
              "{\"type\":\"number\",\"minimum\":10,\"exclusiveMinimum\":true}")]
    public void ExclusiveBoundValueChangesAreCompared(string oldSchema, string newSchema)
    {
        var differences = OpenApiComparator.Compare(Document(oldSchema), Document(newSchema), out _).ToList();
        Assert.That(differences.Count(message => message.Code == "ConstraintIsStronger"), Is.EqualTo(1));
    }

    [TestCase("{\"a\":[1,true]}", "{\"a\":[1,false]}", true)]
    [TestCase("{\"a\":1,\"b\":2}", "{\"b\":2,\"a\":1}", false)]
    [TestCase("null", "null", false)]
    [TestCase("1.5", "2.5", true)]
    [SetCulture("fr-FR")]
    public void JsonDefaultsAreComparedByValue(string oldDefault, string newDefault, bool changed)
    {
        var oldSchema = "{\"default\":" + oldDefault + "}";
        var newSchema = "{\"default\":" + newDefault + "}";
        var differences = OpenApiComparator.Compare(Document(oldSchema), Document(newSchema), out _).ToList();
        Assert.That(differences.Any(message => message.Code == "DefaultValueChanged"), Is.EqualTo(changed));
    }
}
