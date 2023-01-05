// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Criteo.OpenApi.Comparator.Comparators.Extensions;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators
{
    internal class ResponseComparator : ComponentComparator
    {
        private readonly ContentComparator _contentComparator;

        internal ResponseComparator(ContentComparator contentComparator)
        {
            _contentComparator = contentComparator;
        }

        internal void Compare(ComparisonContext context,
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
                    return;
            }

            if (!string.IsNullOrWhiteSpace(newResponse.Reference?.ReferenceV3))
            {
                newResponse = newResponse.Reference.Resolve(context.NewOpenApiDocument.Components.Responses);
                if (newResponse == null)
                    return;
            }

            CompareHeaders(context, oldResponse.Headers, newResponse.Headers);

            base.Compare(context, oldResponse, newResponse);

            _contentComparator.Compare(context, oldResponse.Content, newResponse.Content);

            context.Direction = DataDirection.None;
        }

        private void CompareHeaders(ComparisonContext context,
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
                    context.LogInfo(ComparisonRules.AddingHeader, header.Key);
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
                    context.LogBreakingChange(ComparisonRules.RemovingHeader, oldHeader.Key);
                }
                context.Pop();
            }

            context.Pop();
        }
    }
}
