using System.Collections.Generic;
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

        internal IEnumerable<ComparisonMessage> Compare(ComparisonContext context,
            OpenApiRequestBody oldRequestBody, OpenApiRequestBody newRequestBody)
        {
            context.Direction = DataDirection.Request;

            if (oldRequestBody == null && newRequestBody == null)
                return context.Messages;

            if (oldRequestBody == null)
            {
                context.LogBreakingChange(ComparisonRules.AddedRequestBody);
                return context.Messages;
            }

            if (newRequestBody == null)
            {
                context.LogBreakingChange(ComparisonRules.RemovedRequestBody);
                return context.Messages;
            }

            if (!string.IsNullOrWhiteSpace(oldRequestBody.Reference?.ReferenceV3))
            {
                oldRequestBody = oldRequestBody.Reference.Resolve(context.OldOpenApiDocument.Components.RequestBodies);
                if (oldRequestBody == null)
                    return context.Messages;
            }

            if (!string.IsNullOrWhiteSpace(newRequestBody.Reference?.ReferenceV3))
            {
                newRequestBody = newRequestBody.Reference.Resolve(context.NewOpenApiDocument.Components.RequestBodies);
                if (newRequestBody == null)
                    return context.Messages;
            }

            CompareRequired(context, oldRequestBody.Required, newRequestBody.Required);

            _contentComparator.Compare(context, oldRequestBody.Content, newRequestBody.Content);

            return context.Messages;
        }

        private static IEnumerable<ComparisonMessage> CompareRequired(ComparisonContext context,
            bool oldRequired, bool newRequired)
        {
            if (oldRequired != newRequired)
            {
                context.PushProperty("required");
                if (newRequired)
                {
                    context.LogBreakingChange(ComparisonRules.RequiredStatusChange, oldRequired, newRequired);
                }
                else
                {
                    context.LogInfo(ComparisonRules.RequiredStatusChange, oldRequired, newRequired);
                }
                context.Pop();
            }

            return context.Messages;
        }
    }
}
