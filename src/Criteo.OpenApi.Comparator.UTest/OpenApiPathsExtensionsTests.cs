// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using Criteo.OpenApi.Comparator.Comparators.Extensions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NUnit.Framework;

namespace Criteo.OpenApi.Comparator.UTest
{
    [TestFixture]
    public class OpenApiPathsExtensionsTests
    {
        [Test]
        public void ToOpenApiPaths_ConvertsSchemaRequiredArray()
        {
            var rawPaths = new OpenApiObject
            {
                ["/pets"] = new OpenApiObject
                {
                    ["get"] = new OpenApiObject
                    {
                        ["parameters"] = new OpenApiArray
                        {
                            new OpenApiObject
                            {
                                ["name"] = new OpenApiString("filter"),
                                ["in"] = new OpenApiString("query"),
                                ["schema"] = new OpenApiObject
                                {
                                    ["type"] = new OpenApiString("object"),
                                    ["properties"] = new OpenApiObject
                                    {
                                        ["name"] = new OpenApiObject { ["type"] = new OpenApiString("string") },
                                    },
                                    ["required"] = new OpenApiArray { new OpenApiString("name") },
                                },
                            },
                        },
                    },
                },
            };

            var paths = rawPaths.ToOpenApiPaths();
            var schema = paths["/pets"].Operations[OperationType.Get].Parameters[0].Schema;

            Assert.That(schema.Required, Is.EquivalentTo(new[] { "name" }));
        }
    }
}
