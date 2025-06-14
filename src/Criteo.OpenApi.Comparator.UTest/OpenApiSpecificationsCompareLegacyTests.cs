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
public class OpenApiSpecificationsCompareLegacyTests
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

        var differences = OpenApiComparator
            .Compare(File.ReadAllText(oldFileName), File.ReadAllText(newFileName), out _);

        var expectedDifferencesText = File.ReadAllText(diffFileName);
        var expectedDifferences = JsonSerializer
            .Deserialize<ComparisonMessageModel[]>(expectedDifferencesText, serializerOptions);

        Assert.That(differences,
            Is.EquivalentTo(expectedDifferences).Using<ComparisonMessage, ComparisonMessageModel>(Comparer),
            $"Expected differences:\n{expectedDifferencesText}\nActual differences:\n{JsonSerializer.Serialize(differences, serializerOptions)}");
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
        return Path.Combine(baseDirectory, "Resource");
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

internal class ComparisonMessageModel
{
    public MessageSeverity Severity { get; set; }
    public string Message { get; set; }
    public string OldJsonRef { get; set; }
    public string NewJsonRef { get; set; }
    public string OldJsonPath { get; set; }
    public string NewJsonPath { get; set; }
    public int Id { get; set; }
    public string Code { get; set; }
    public MessageType Mode { get; set; }
}
