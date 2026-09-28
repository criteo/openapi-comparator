// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using Criteo.OpenApi.Comparator.Comparators.Extensions;
#if NET10_0
using System.Text.Json.Nodes;
#else
using Microsoft.OpenApi.Any;
#endif
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
#if NET10_0
            var oldInteger = JsonValue.Create(8);
            var newInteger = JsonValue.Create(8);
#else
            var oldInteger = new OpenApiInteger(8);
            var newInteger = new OpenApiInteger(8);
#endif
            Assert.That(oldInteger.DifferFrom(newInteger), Is.False);
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_DifferentOpenApiInteger()
        {
#if NET10_0
            var oldInteger = JsonValue.Create(8);
            var newInteger = JsonValue.Create(1);
#else
            var oldInteger = new OpenApiInteger(8);
            var newInteger = new OpenApiInteger(1);
#endif
            Assert.That(oldInteger.DifferFrom(newInteger), Is.True);
        }

        [Test]
        public void DifferFrom_ShouldReturn_false_When_SameOpenApiBoolean()
        {
#if NET10_0
            var oldBoolean = JsonValue.Create(false);
            var newBoolean = JsonValue.Create(false);
#else
            var oldBoolean = new OpenApiBoolean(false);
            var newBoolean = new OpenApiBoolean(false);
#endif
            Assert.That(oldBoolean.DifferFrom(newBoolean), Is.False);
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_DifferentOpenApiBoolean()
        {
#if NET10_0
            var oldBoolean = JsonValue.Create(false);
            var newBoolean = JsonValue.Create(true);
#else
            var oldBoolean = new OpenApiBoolean(false);
            var newBoolean = new OpenApiBoolean(true);
#endif
            Assert.That(oldBoolean.DifferFrom(newBoolean), Is.True);
        }

        [Test]
        public void DifferFrom_ShouldReturn_false_When_SameOpenApiString()
        {
#if NET10_0
            var oldString = JsonValue.Create("string");
            var newString = JsonValue.Create("string");
#else
            var oldString = new OpenApiString("string");
            var newString = new OpenApiString("string");
#endif
            Assert.That(oldString.DifferFrom(newString), Is.False);
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_DifferentOpenApiString()
        {
#if NET10_0
            var oldString = JsonValue.Create("oldString");
            var newString = JsonValue.Create("newString");
#else
            var oldString = new OpenApiString("oldString");
            var newString = new OpenApiString("newString");
#endif
            Assert.That(oldString.DifferFrom(newString), Is.True);
        }
    }
}
