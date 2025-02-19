// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using Criteo.OpenApi.Comparator.Comparators.Extensions;
using Microsoft.OpenApi.Any;
using NUnit.Framework;

namespace Criteo.OpenApi.Comparator.UTest
{
    [TestFixture]
    public class PrimitiveTypesExtensionTests
    {
        [Test]
        public void DifferFrom_ShouldReturn_false_When_SameString()
        {
            const string oldString = "string";
            const string newString = "string";
            Assert.That(oldString.DifferFrom(newString), Is.False);
        }

        [Test]
        public void DifferFrom_ShouldReturn_false_When_NullString()
        {
            const string oldString = null;
            const string newString = null;
            Assert.That(oldString.DifferFrom(newString), Is.False);
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_NullOldString()
        {
            const string oldString = null;
            const string newString = "string";
            Assert.That(oldString.DifferFrom(newString), Is.True);
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_NullNewString()
        {
            const string oldString = "string";
            const string newString = null;
            Assert.That(oldString.DifferFrom(newString), Is.True);
        }

        [Test]
        public void DifferFrom_ShouldReturn_false_When_SameInteger()
        {
            int? oldInteger = 8;
            int? newInteger = 8;
            Assert.That(oldInteger.DifferFrom(newInteger), Is.False);
        }

        [Test]
        public void DifferFrom_ShouldReturn_false_When_NullInteger()
        {
            int? oldInteger = null;
            int? newInteger = null;
            Assert.That(oldInteger.DifferFrom(newInteger), Is.False);
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_NullOldInteger()
        {
            int? oldInteger = null;
            int? newInteger = 8;
            Assert.That(oldInteger.DifferFrom(newInteger), Is.True);
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_NullNewInteger()
        {
            int? oldInteger = 8;
            Assert.That(oldInteger.DifferFrom(null), Is.True);
        }

        [Test]
        public void DifferFrom_ShouldReturn_false_When_SameOpenApiInteger()
        {
            var oldInteger = new OpenApiInteger(8);
            var newInteger = new OpenApiInteger(8);
            Assert.That(oldInteger.DifferFrom(newInteger), Is.False);
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_DifferentOpenApiInteger()
        {
            var oldInteger = new OpenApiInteger(8);
            var newInteger = new OpenApiInteger(1);
            Assert.That(oldInteger.DifferFrom(newInteger), Is.True);
        }

        [Test]
        public void DifferFrom_ShouldReturn_false_When_SameOpenApiBoolean()
        {
            var oldBoolean = new OpenApiBoolean(false);
            var newBoolean = new OpenApiBoolean(false);
            Assert.That(oldBoolean.DifferFrom(newBoolean), Is.False);
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_DifferentOpenApiBoolean()
        {
            var oldBoolean = new OpenApiBoolean(false);
            var newBoolean = new OpenApiBoolean(true);
            Assert.That(oldBoolean.DifferFrom(newBoolean), Is.True);
        }

        [Test]
        public void DifferFrom_ShouldReturn_false_When_SameOpenApiString()
        {
            var oldString = new OpenApiString("string");
            var newString = new OpenApiString("string");
            Assert.That(oldString.DifferFrom(newString), Is.False);
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_DifferentOpenApiString()
        {
            var oldString = new OpenApiString("oldString");
            var newString = new OpenApiString("newString");
            Assert.That(oldString.DifferFrom(newString), Is.True);
        }
    }
}
