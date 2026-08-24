// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System;
using System.IO;
using NUnit.Framework;

namespace Criteo.OpenApi.Comparator.Cli.UTest
{
    [TestFixture]
    public class ProgramTests
    {
        private NonClosingStringWriter _stderr;
        private TextWriter _originalError;

        [SetUp]
        public void CaptureConsole()
        {
            _stderr = new NonClosingStringWriter();
            _originalError = Console.Error;
            Console.SetError(_stderr);
        }

        [TearDown]
        public void RestoreConsole()
        {
            Console.SetError(_originalError);
            _stderr.Dispose();
        }

        [Test]
        public void Main_Returns1_WhenComparisonThrowsBeforeParsingErrorsAreAssigned()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt");
            File.WriteAllText(path, "{\n  \"invalid_json\"\n}");
            try
            {
                var exitCode = Program.Main(new[] { "-o", path, "-n", path });

                Assert.That(exitCode, Is.EqualTo(1));
                Assert.That(_stderr.ToString(), Does.Contain("An error occurred while comparing the OpenAPI specifications"));
            }
            finally
            {
                File.Delete(path);
            }
        }

        private sealed class NonClosingStringWriter : StringWriter
        {
            protected override void Dispose(bool disposing) { }
        }
    }
}
