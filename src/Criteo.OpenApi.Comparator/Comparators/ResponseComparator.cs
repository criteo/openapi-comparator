// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Criteo.OpenApi.Comparator.Comparators.Extensions;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators
{
    /// <summary>
    /// Describes a single response from an API Operation.
    /// </summary>
    public class ResponseComparator : ComponentComparator
    {
        private readonly ContentComparator _contentComparator;

        public ResponseComparator(ContentComparator contentComparator)
        {
            _contentComparator = contentComparator;
        }
        /// <summary>
        /// Compare a modified document node (this) to a previous one and look for breaking as well as non-breaking changes.
        /// </summary>
        /// <param name="context">The modified document context.</param>
        /// <param name="oldResponse">The original response model.</param>
        /// <param name="newResponse">The original response model.</param>
        /// <returns>A list of messages from the comparison.</returns>
        public IEnumerable<ComparisonMessage> Compare(ComparisonContext<OpenApiDocument> context,
            OpenApiResponse oldResponse, OpenApiResponse newResponse)
        {
            if (oldResponse == null)
                throw new ArgumentNullException(nameof(oldResponse));

            if (newResponse == null)
                throw new ArgumentNullException(nameof(newResponse));

            context.Direction = DataDirection.Response;

            if (!string.IsNullOrWhiteSpace(oldResponse.Reference?.ReferenceV3))
            {
                oldResponse = oldResponse.Reference.Resolve(context.OldOpenApiDocument.Components.Responses);
                if (oldResponse == null)
                    return context.Messages;
            }

            if (!string.IsNullOrWhiteSpace(newResponse.Reference?.ReferenceV3))
            {
                newResponse = newResponse.Reference.Resolve(context.NewOpenApiDocument.Components.Responses);
                if (newResponse == null)
                    return context.Messages;
            }

            CompareHeaders(context, oldResponse.Headers, newResponse.Headers);

            base.Compare(context, oldResponse, newResponse);

            _contentComparator.Compare(context, oldResponse.Content, newResponse.Content);

            context.Direction = DataDirection.None;

            return context.Messages;
        }

        private void CompareHeaders(ComparisonContext<OpenApiDocument> context,
            IDictionary<string, OpenApiHeader> oldHeaders,
            IDictionary<string, OpenApiHeader> newHeaders)
        {
            newHeaders = newHeaders ?? new Dictionary<string, OpenApiHeader>();
            oldHeaders = oldHeaders ?? new Dictionary<string, OpenApiHeader>();

            context.PushProperty("headers");
            foreach (var header in newHeaders)
            {
                context.PushProperty(header.Key);
                if (!oldHeaders.TryGetValue(header.Key, out var oldHeader))
                {
                    context.LogInfo(ComparisonMessages.AddingHeader, header.Key);
                }
                else
                {
                    base.Compare(context, oldHeader, header.Value);
                }
                context.Pop();
            }

            foreach (var oldHeader in oldHeaders)
            {
                context.PushProperty(oldHeader.Key);
                if (!newHeaders.ContainsKey(oldHeader.Key))
                {
                    context.LogBreakingChange(ComparisonMessages.RemovingHeader, oldHeader.Key);
                }
                context.Pop();
            }

            context.Pop();
        }
    }
}
