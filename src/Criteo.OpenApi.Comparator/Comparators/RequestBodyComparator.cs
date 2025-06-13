// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using Criteo.OpenApi.Comparator.Comparators.Extensions;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators
{
    internal class RequestBodyComparator
    {
        private readonly ContentComparator _contentComparator;

        internal RequestBodyComparator(ContentComparator contentComparator)
        {
            _contentComparator = contentComparator;
        }

        internal void Compare(ComparisonContext context,
            OpenApiRequestBody oldRequestBody, OpenApiRequestBody newRequestBody)
        {
            using (context.WithDirection(DataDirection.Request))
            {

                if (oldRequestBody == null && newRequestBody == null)
                    return;

                if (oldRequestBody == null)
                {
                    context.Log(ComparisonRules.AddedRequestBody);
                    return;
                }

                if (newRequestBody == null)
                {
                    context.Log(ComparisonRules.RemovedRequestBody);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(oldRequestBody.Reference?.ReferenceV3))
                {
                    oldRequestBody = oldRequestBody.Reference.Resolve(context.OldOpenApiDocument.Components.RequestBodies);
                    if (oldRequestBody == null)
                        return;
                }

                if (!string.IsNullOrWhiteSpace(newRequestBody.Reference?.ReferenceV3))
                {
                    newRequestBody = newRequestBody.Reference.Resolve(context.NewOpenApiDocument.Components.RequestBodies);
                    if (newRequestBody == null)
                        return;
                }

                CompareRequired(context, oldRequestBody.Required, newRequestBody.Required);

                _contentComparator.Compare(context, oldRequestBody.Content, newRequestBody.Content);
            }
        }

        private static void CompareRequired(ComparisonContext context,
            bool oldRequired, bool newRequired)
        {
            if (oldRequired != newRequired)
            {
                context.PushProperty("required");
                if (newRequired)
                {
                    context.Log(ComparisonRules.RequiredStatusAdded, oldRequired, newRequired);
                }
                else
                {
                    context.Log(ComparisonRules.RequiredStatusRemoved, oldRequired, newRequired);
                }
                context.Pop();
            }
        }
    }
}
