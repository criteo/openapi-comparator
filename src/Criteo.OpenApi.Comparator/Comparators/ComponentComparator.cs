// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators
{
    /// <summary>
    /// Compare the fields that components have in common (e.g. the $ref field)
    /// https://github.com/OAI/OpenAPI-Specification/blob/main/versions/3.0.0.md#componentsObject
    /// </summary>
    internal abstract class ComponentComparator
    {
        /// <summary>
        /// Compare a modified parameter to an old one and look for breaking as well as non-breaking changes.
        /// </summary>
        /// <param name="context">The modified document context.</param>
        /// <param name="oldParameter">The original parameter to compare.</param>
        /// <param name="newParameter">The new parameter to compare.</param>
        /// <returns>A list of messages from the comparison.</returns>
        protected void Compare(ComparisonContext context, OpenApiParameter oldParameter, OpenApiParameter newParameter)
        {
            if (oldParameter == null)
                throw new ArgumentNullException(nameof(oldParameter));

            if (newParameter == null)
                throw new ArgumentNullException(nameof(newParameter));

            CompareReference(context, oldParameter.Reference, newParameter.Reference);
        }

        /// <summary>
        /// Compare a modified response to an old one and look for breaking as well as non-breaking changes.
        /// </summary>
        /// <param name="context">The modified document context.</param>
        /// <param name="oldResponse">The original response to compare.</param>
        /// <param name="newResponse">The new response to compare.</param>
        /// <returns>A list of messages from the comparison.</returns>
        protected void Compare(ComparisonContext context, OpenApiResponse oldResponse, OpenApiResponse newResponse)
        {
            if (oldResponse == null)
                throw new ArgumentNullException(nameof(oldResponse));

            if (newResponse == null)
                throw new ArgumentNullException(nameof(newResponse));

            CompareReference(context, oldResponse.Reference, newResponse.Reference);
        }

        /// <summary>
        /// Compare a modified header to an old one and look for breaking as well as non-breaking changes.
        /// </summary>
        /// <param name="context">The modified document context.</param>
        /// <param name="oldHeader">The original header to compare.</param>
        /// <param name="newHeader">The new header to compare.</param>
        /// <returns>A list of messages from the comparison.</returns>
        protected void Compare(ComparisonContext context, OpenApiHeader oldHeader, OpenApiHeader newHeader)
        {
            if (oldHeader == null)
                throw new ArgumentNullException(nameof(oldHeader));

            if (newHeader == null)
                throw new ArgumentNullException(nameof(newHeader));

            CompareReference(context, oldHeader.Reference, newHeader.Reference);
        }

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
