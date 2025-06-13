// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators
{
    internal static class ComponentComparator<T> where T : IOpenApiReferenceable
    {
        internal static void Compare(ComparisonContext context, T oldComponent, T newComponent)
        {
            if (oldComponent == null)
                throw new ArgumentNullException(nameof(oldComponent));

            if (newComponent == null)
                throw new ArgumentNullException(nameof(newComponent));

            CompareReference(context, oldComponent.Reference, newComponent.Reference);
        }

        private static void CompareReference(ComparisonContext context,
            OpenApiReference oldReference,
            OpenApiReference newReference)
        {
            if (newReference?.ReferenceV3 != null && !newReference.ReferenceV3.Equals(oldReference?.ReferenceV3))
            {
                context.Log(ComparisonRules.ReferenceRedirection);
            }
        }
    }
}
