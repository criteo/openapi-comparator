// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Criteo.OpenApi.Comparator.Comparators.Extensions;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators;

internal class ResponseComparator
{
    private readonly ContentComparator _contentComparator;

    internal ResponseComparator(ContentComparator contentComparator)
    {
        _contentComparator = contentComparator;
    }

    internal void Compare(ComparisonContext context,
        OpenApiResponse oldResponse, OpenApiResponse newResponse)
    {
        ComponentComparator<OpenApiResponse>.Compare(context, oldResponse, newResponse);

        using (context.WithDirection(DataDirection.Response))
        {
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

            _contentComparator.Compare(context, oldResponse.Content, newResponse.Content);
        }
    }

    private static void CompareHeaders(ComparisonContext context,
        IDictionary<string, OpenApiHeader> oldHeaders,
        IDictionary<string, OpenApiHeader> newHeaders)
    {
        newHeaders ??= new Dictionary<string, OpenApiHeader>();
        oldHeaders ??= new Dictionary<string, OpenApiHeader>();

        context.PushProperty("headers");
        foreach (var header in newHeaders)
        {
            context.PushProperty(header.Key);
            if (!oldHeaders.TryGetValue(header.Key, out var oldHeader))
            {
                if (header.Value.Required && context.Direction == DataDirection.Request)
                    context.Log(ComparisonRules.AddingRequiredHeader, header.Key);
                else
                    context.Log(ComparisonRules.AddingHeader, header.Key);
            }
            else
            {
                ComponentComparator<OpenApiHeader>.Compare(context, oldHeader, header.Value);
            }

            context.Pop();
        }

        foreach (var oldHeader in oldHeaders)
        {
            context.PushProperty(oldHeader.Key);
            if (!newHeaders.ContainsKey(oldHeader.Key))
            {
                if (context.Direction == DataDirection.Response)
                    context.Log(ComparisonRules.RemovingHeader, oldHeader.Key);
                else
                    context.Log(ComparisonRules.RemovingRequestHeader, oldHeader.Key);
            }

            context.Pop();
        }

        context.Pop();
    }
}