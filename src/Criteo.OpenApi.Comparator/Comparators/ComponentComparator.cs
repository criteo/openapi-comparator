// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System;
using Criteo.OpenApi.Comparator.Comparators.Extensions;
using Microsoft.OpenApi;

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

            CompareReference(context, oldComponent.GetReference(), newComponent.GetReference());
        }

        private static void CompareReference(ComparisonContext context,
            BaseOpenApiReference oldReference,
            BaseOpenApiReference newReference)
        {
            if (newReference?.ReferenceV3 != null && !newReference.ReferenceV3.Equals(oldReference?.ReferenceV3))
            {
                context.LogBreakingChange(ComparisonRules.ReferenceRedirection);
            }
        }
    }
}
