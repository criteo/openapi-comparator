// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Criteo.OpenApi.Comparator.Comparators.Extensions;
#if NET10_0
using Microsoft.OpenApi;
using OpenApiParameter = Microsoft.OpenApi.IOpenApiParameter;
using OpenApiReference = Microsoft.OpenApi.BaseOpenApiReference;
using OpenApiSchema = Microsoft.OpenApi.IOpenApiSchema;
#else
using Microsoft.OpenApi.Models;
#endif

namespace Criteo.OpenApi.Comparator.Comparators
{
    internal class ParameterComparator
    {
        private readonly SchemaComparator _schemaComparator;
        private readonly ContentComparator _contentComparator;

        private readonly LinkedList<OpenApiParameter> _visitedParameters;

        internal bool WasVisited(OpenApiParameter parameter) => _visitedParameters.Contains(parameter);

        internal ParameterComparator(SchemaComparator schemaComparator, ContentComparator contentComparator)
        {
            _schemaComparator = schemaComparator;
            _contentComparator = contentComparator;
            _visitedParameters = new LinkedList<OpenApiParameter>();
        }

        internal void Compare(
            ComparisonContext context,
            OpenApiParameter oldParameter,
            OpenApiParameter newParameter)
        {
            ComponentComparator<OpenApiParameter>.Compare(context, oldParameter, newParameter);

            using (context.WithDirection(DataDirection.Request))
            {
                var areParametersReferenced = false;

                var oldReference = oldParameter.GetReference();
                if (oldReference != null)
                {
                    oldParameter = FindReferencedParameter(oldReference, context.OldOpenApiDocument.Components.Parameters);
                    areParametersReferenced = true;
                    if (oldParameter == null)
                        return;
                }
                var newReference = newParameter.GetReference();
                if (newReference != null)
                {
                    newParameter = FindReferencedParameter(newReference, context.NewOpenApiDocument.Components.Parameters);
                    areParametersReferenced = true;
                    if (newParameter == null)
                        return;
                }

                if (areParametersReferenced)
                {
                    if (_visitedParameters.Contains(oldParameter))
                        return;

                    _visitedParameters.AddFirst(oldParameter);
                }

                CompareIn(context, oldParameter.In, newParameter.In);

                if (oldParameter.Schema?.GetReference() == null && newParameter.Schema?.GetReference() == null)
                    CompareConstantStatus(context, oldParameter, newParameter);

                CompareRequiredStatus(context, oldParameter, newParameter);

                CompareStyle(context, oldParameter, newParameter);

                CompareSchema(context, oldParameter.Schema, newParameter.Schema);

                _contentComparator.Compare(context, oldParameter.Content, newParameter.Content);
            }
        }

        private static void CompareIn(ComparisonContext context,
            ParameterLocation? oldIn,
            ParameterLocation? newIn)
        {
            if (oldIn != newIn)
            {
                context.PushProperty("in");
                context.LogBreakingChange(ComparisonRules.ParameterInHasChanged,
                    oldIn.ToString().ToLower(),
                    newIn.ToString().ToLower()
                );
                context.Pop();
            }
        }

        private static void CompareConstantStatus(ComparisonContext context,
            OpenApiParameter oldParameter,
            OpenApiParameter newParameter)
        {
            if (newParameter.IsConstant() != oldParameter.IsConstant())
            {
                context.PushProperty("enum");
                context.LogBreakingChange(ComparisonRules.ConstantStatusHasChanged);
                context.Pop();
            }
        }

        private static void CompareRequiredStatus(ComparisonContext context,
            OpenApiParameter oldParameter, OpenApiParameter newParameter)
        {
            if (oldParameter.IsRequired() == newParameter.IsRequired() || context.Direction == DataDirection.Response)
                return;

            context.PushProperty("required");
            if (newParameter.IsRequired())
            {
                context.LogBreakingChange(ComparisonRules.RequiredStatusChange, false, true);
            }
            else
            {
                context.LogInfo(ComparisonRules.RequiredStatusChange, true, false);
            }
            context.Pop();
        }

        private static void CompareStyle(ComparisonContext context,
            OpenApiParameter oldParameter,
            OpenApiParameter newParameter)
        {
            if (oldParameter.Style != newParameter.Style)
            {
                context.PushProperty("style");
                context.LogBreakingChange(ComparisonRules.ParameterStyleChanged, oldParameter.Name);
                context.Pop();
            }
        }

        private void CompareSchema(ComparisonContext context,
            OpenApiSchema oldSchema, OpenApiSchema newSchema)
        {
            if (oldSchema == null || newSchema == null)
                return;

            context.PushProperty("schema");
            _schemaComparator.Compare(context, oldSchema, newSchema);
            context.Pop();
        }

        private static OpenApiParameter FindReferencedParameter(
            OpenApiReference reference, IDictionary<string, OpenApiParameter> parameters
        )
        {
            if (parameters == null || reference == null)
                return null;

            var name = !string.IsNullOrWhiteSpace(reference.ReferenceV3)
                ? reference.ReferenceV3.Split('/').LastOrDefault()
                : reference.Id;
            if (name != null && parameters.TryGetValue(name, out var parameter))
                return parameter;

            return null;
        }
    }
}
