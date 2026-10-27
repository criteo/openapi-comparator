// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Criteo.OpenApi.Comparator.Comparators.Extensions;
using Microsoft.OpenApi;

namespace Criteo.OpenApi.Comparator.Comparators
{
    internal class ResponseComparator
    {
        private readonly ContentComparator _contentComparator;

        internal ResponseComparator(ContentComparator contentComparator)
        {
            _contentComparator = contentComparator;
        }

        internal void Compare(ComparisonContext context,
            IOpenApiResponse oldResponse, IOpenApiResponse newResponse)
        {
            ComponentComparator<IOpenApiResponse>.Compare(context, oldResponse, newResponse);

            using (context.WithDirection(DataDirection.Response))
            {
                if (!string.IsNullOrWhiteSpace(oldResponse.GetReference()?.ReferenceV3))
                {
                    oldResponse = oldResponse.GetReference().Resolve(context.OldOpenApiDocument.Components.Responses);
                    if (oldResponse == null)
                        return;
                }

                if (!string.IsNullOrWhiteSpace(newResponse.GetReference()?.ReferenceV3))
                {
                    newResponse = newResponse.GetReference().Resolve(context.NewOpenApiDocument.Components.Responses);
                    if (newResponse == null)
                        return;
                }

                CompareHeaders(context, oldResponse.Headers, newResponse.Headers);

                _contentComparator.Compare(context, oldResponse.Content, newResponse.Content);
            }
        }

        private static void CompareHeaders(ComparisonContext context,
            IDictionary<string, IOpenApiHeader> oldHeaders,
            IDictionary<string, IOpenApiHeader> newHeaders)
        {
            newHeaders = newHeaders ?? new Dictionary<string, IOpenApiHeader>();
            oldHeaders = oldHeaders ?? new Dictionary<string, IOpenApiHeader>();

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
                    ComponentComparator<IOpenApiHeader>.Compare(context, oldHeader, header.Value);
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
