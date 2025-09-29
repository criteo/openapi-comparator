// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.IO;
using NUnit.Framework;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Criteo.OpenApi.Comparator.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;

namespace Criteo.OpenApi.Comparator.UTest;

/// <summary>
/// This class contains tests for the logic comparing two swagger specifications,
/// an older version against newer version.
///
/// For all but the tests that verify that version checks are done properly, the
/// old and new specifications have the same version number, which should force
/// the comparison logic to produce errors rather than warnings for each breaking
/// change.
///
/// Non-breaking changes are always presented as informational messages, regardless
/// of whether the version has changed or not.
/// </summary>
[TestFixture]
public class OpenApiSpecificationsCompareTests
{
    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    [TestCaseSource(nameof(TestCases))]
    public void CompareOAS(string testcase)
    {
        var resourceDirectory = GetResourceDirectory();
        var oldFileName = Path.Combine(resourceDirectory, testcase, "old.yaml");
        var newFileName = Path.Combine(resourceDirectory, testcase, "new.yaml");
        var diffFileName = Path.Combine(resourceDirectory, testcase, "diff.json");

        var result = OpenApiComparator
            .Compare(out var differences, File.ReadAllText(oldFileName), File.ReadAllText(newFileName));

        var expectedDifferencesText = File.ReadAllText(diffFileName);
        var expectedDifferences = JsonSerializer
            .Deserialize<TestResult>(expectedDifferencesText, serializerOptions);

        Assert.That(differences,
            Is.EquivalentTo(expectedDifferences.Messages).Using<ComparisonMessage, ComparisonMessageModel>(Comparer),
            $"Expected differences:\n{JsonSerializer.Serialize(expectedDifferences.Messages, serializerOptions)}\nActual differences:\n{JsonSerializer.Serialize(differences, serializerOptions)}");
        Assert.That(result, Is.EqualTo(expectedDifferences.Result));

    }

    private static IEnumerable<string> TestCases()
    {
        var resourceDirectory = GetResourceDirectory();
        return Directory
            .GetDirectories(resourceDirectory)
            .Where(directory => File.Exists(Path.Combine(directory, "diff.json")))
            .Select(Path.GetFileName);
    }

    private static string GetResourceDirectory()
    {
        var assemblyLocation = typeof(OpenApiSpecificationsCompareTests).GetTypeInfo().Assembly.Location;
        var baseDirectory = Directory.GetParent(assemblyLocation).ToString();
        return Path.Combine(baseDirectory, "Resource1.0");
    }

    private static bool Comparer(ComparisonMessage message, ComparisonMessageModel model)
    {
        return message.Severity == model.Severity
            && message.Message == model.Message
            && message.OldJsonRef == model.OldJsonRef
            && message.NewJsonRef == model.NewJsonRef
            && message.OldJsonPath == model.OldJsonPath
            && message.NewJsonPath == model.NewJsonPath
            && message.Id == model.Id
            && message.Code == model.Code
            && message.Mode == model.Mode;
    }
}

internal class TestResult
{
    public ChangeLevel Result { get; set; }
    public List<ComparisonMessageModel> Messages { get; set; }
}
