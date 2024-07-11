// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Criteo.OpenApi.Comparator.Logging;
using NUnit.Framework;

namespace Criteo.OpenApi.Comparator.UTest;

[TestFixture]
public class OpenApiEnumDirectionTests
{
    [TestCase("old_without_refs", "path_add", "#/paths/~1order~1{path}/post/parameters/0/schema/enum")]
    [TestCase("old_without_refs", "query_add", "#/paths/~1order~1{path}/post/parameters/1/schema/enum")]
    [TestCase("old_without_refs", "body_add", "#/paths/~1order~1{path}/post/requestBody/content/application~1json/schema/properties/foo/enum")]
    [TestCase("old_with_refs", "request_ref_add", "#/paths/~1order~1{path}/post/parameters/0/schema/enum")]
    public void CompareOAS_ShouldReturn_NonBreaking_AddedEnumValueChange(string oldName, string newName, string jsonRef)
    {
        var differences = CompareSpecifications(oldName, newName);
        differences.AssertContains(new ExpectedDifference
        {
            Rule = ComparisonRules.AddedEnumValue,
            Severity = Severity.Warning,
            NewJsonRef = jsonRef
        });
    }


    [TestCase("old_without_refs", "response_add", "#/paths/~1order~1{path}/post/responses/200/content/application~1json/schema/properties/bar/enum")]
    [TestCase("old_with_refs", "response_ref_add", "#/paths/~1order~1{path}/post/responses/200/content/application~1json/schema/properties/bar/enum")]
    [TestCase("old_with_refs", "both_ref_add", "#/paths/~1order~1{path}/post/responses/200/content/application~1json/schema/properties/foo/enum")]
    public void CompareOAS_ShouldReturn_Breaking_AddedEnumValueChange(string oldName, string newName, string jsonRef)
    {
        var differences = CompareSpecifications(oldName, newName);
        differences.AssertContains(new ExpectedDifference
        {
            Rule = ComparisonRules.AddedEnumValue,
            Severity = Severity.Error,
            NewJsonRef = jsonRef
        });
    }

    [TestCase("old_without_refs", "response_remove", "#/paths/~1order~1{path}/post/responses/200/content/application~1json/schema/properties/bar/enum")]
    [TestCase("old_with_refs", "response_ref_remove", "#/paths/~1order~1{path}/post/responses/200/content/application~1json/schema/properties/bar/enum")]
    public void CompareOAS_ShouldReturn_NonBreaking_RemovedEnumValueChange(string oldName, string newName, string jsonRef)
    {
        var differences = CompareSpecifications(oldName, newName);
        differences.AssertContains(new ExpectedDifference
        {
            Rule = ComparisonRules.RemovedEnumValue,
            Severity = Severity.Warning,
            NewJsonRef = jsonRef
        });
    }

    [TestCase("old_without_refs", "path_remove", "#/paths/~1order~1{path}/post/parameters/0/schema/enum")]
    [TestCase("old_without_refs", "query_remove", "#/paths/~1order~1{path}/post/parameters/1/schema/enum")]
    [TestCase("old_without_refs", "body_remove", "#/paths/~1order~1{path}/post/requestBody/content/application~1json/schema/properties/foo/enum")]
    [TestCase("old_with_refs", "request_ref_remove", "#/paths/~1order~1{path}/post/parameters/0/schema/enum")]
    [TestCase("old_with_refs", "both_ref_remove", "#/paths/~1order~1{path}/post/requestBody/content/application~1json/schema/properties/foo/enum")]
    public void CompareOAS_ShouldReturn_Breaking_RemovedEnumValueChange(string oldName, string newName, string jsonRef)
    {
        var differences = CompareSpecifications(oldName, newName);
        differences.AssertContains(new ExpectedDifference
        {
            Rule = ComparisonRules.RemovedEnumValue,
            Severity = Severity.Error,
            NewJsonRef = jsonRef,
        });
    }

    private static IList<ComparisonMessage> CompareSpecifications(string oldName, string newName)
    {
        var baseDirectory = Directory
            .GetParent(typeof(OpenApiSpecificationsCompareTests).GetTypeInfo().Assembly.Location)
            .ToString();

        var oldFileName = Path.Combine(baseDirectory, "Resource", "enum_direction", oldName + ".json");
        var newFileName = Path.Combine(baseDirectory, "Resource", "enum_direction", newName + ".yaml");

        var differences = OpenApiComparator
            .Compare(File.ReadAllText(oldFileName), File.ReadAllText(newFileName), strict: true)
            .ToList();

        OpenApiSpecificationsCompareTests.ValidateDifferences(differences);

        return differences;
    }
}
