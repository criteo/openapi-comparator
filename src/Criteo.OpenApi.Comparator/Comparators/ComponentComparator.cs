// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System;
using Criteo.OpenApi.Comparator.Comparators.Extensions;
#if NET10_0
using Microsoft.OpenApi;
using OpenApiReference = Microsoft.OpenApi.BaseOpenApiReference;
#else
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
#endif

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

#if NET10_0
            CompareReference(context, GetReference(oldComponent), GetReference(newComponent));
#else
            CompareReference(context, oldComponent.Reference, newComponent.Reference);
#endif
        }

#if NET10_0
        private static OpenApiReference GetReference(T component) => component switch
        {
            IOpenApiSchema schema => schema.GetReference(),
            IOpenApiParameter parameter => parameter.GetReference(),
            IOpenApiResponse response => response.GetReference(),
            IOpenApiRequestBody requestBody => requestBody.GetReference(),
            _ => null
        };
#endif

        private static void CompareReference(ComparisonContext context,
            OpenApiReference oldReference,
            OpenApiReference newReference)
        {
            if (newReference?.ReferenceV3 != null && !newReference.ReferenceV3.Equals(oldReference?.ReferenceV3))
            {
                context.LogBreakingChange(ComparisonRules.ReferenceRedirection);
            }
        }
    }
}
