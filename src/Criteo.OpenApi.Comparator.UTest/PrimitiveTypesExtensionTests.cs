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
            Assert.IsFalse(oldString.DifferFrom(newString));
        }

        [Test]
        public void DifferFrom_ShouldReturn_false_When_NullString()
        {
            const string oldString = null;
            const string newString = null;
            Assert.IsFalse(oldString.DifferFrom(newString));
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_NullOldString()
        {
            const string oldString = null;
            const string newString = "string";
            Assert.IsTrue(oldString.DifferFrom(newString));
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_NullNewString()
        {
            const string oldString = "string";
            const string newString = null;
            Assert.IsTrue(oldString.DifferFrom(newString));
        }

        [Test]
        public void DifferFrom_ShouldReturn_false_When_SameInteger()
        {
            int? oldInteger = 8;
            int? newInteger = 8;
            Assert.IsFalse(oldInteger.DifferFrom(newInteger));
        }

        [Test]
        public void DifferFrom_ShouldReturn_false_When_NullInteger()
        {
            int? oldInteger = null;
            int? newInteger = null;
            Assert.IsFalse(oldInteger.DifferFrom(newInteger));
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_NullOldInteger()
        {
            int? oldInteger = null;
            int? newInteger = 8;
            Assert.IsTrue(oldInteger.DifferFrom(newInteger));
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_NullNewInteger()
        {
            int? oldInteger = 8;
            Assert.IsTrue(oldInteger.DifferFrom(null));
        }

        [Test]
        public void DifferFrom_ShouldReturn_false_When_SameOpenApiInteger()
        {
            var oldInteger = new OpenApiInteger(8);
            var newInteger = new OpenApiInteger(8);
            Assert.IsFalse(oldInteger.DifferFrom(newInteger));
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_DifferentOpenApiInteger()
        {
            var oldInteger = new OpenApiInteger(8);
            var newInteger = new OpenApiInteger(1);
            Assert.IsTrue(oldInteger.DifferFrom(newInteger));
        }

        [Test]
        public void DifferFrom_ShouldReturn_false_When_SameOpenApiBoolean()
        {
            var oldBoolean = new OpenApiBoolean(false);
            var newBoolean = new OpenApiBoolean(false);
            Assert.IsFalse(oldBoolean.DifferFrom(newBoolean));
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_DifferentOpenApiBoolean()
        {
            var oldBoolean = new OpenApiBoolean(false);
            var newBoolean = new OpenApiBoolean(true);
            Assert.IsTrue(oldBoolean.DifferFrom(newBoolean));
        }

        [Test]
        public void DifferFrom_ShouldReturn_false_When_SameOpenApiString()
        {
            var oldString = new OpenApiString("string");
            var newString = new OpenApiString("string");
            Assert.IsFalse(oldString.DifferFrom(newString));
        }

        [Test]
        public void DifferFrom_ShouldReturn_true_When_DifferentOpenApiString()
        {
            var oldString = new OpenApiString("oldString");
            var newString = new OpenApiString("newString");
            Assert.IsTrue(oldString.DifferFrom(newString));
        }
    }
}
