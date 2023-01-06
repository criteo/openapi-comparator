// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Criteo.OpenApi.Comparator.Comparators.Extensions;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators
{
    internal class ParameterComparator : ComponentComparator
    {
        private readonly SchemaComparator _schemaComparator;
        private readonly ContentComparator _contentComparator;

        private readonly LinkedList<OpenApiParameter> _visitedParameters;

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
            if (oldParameter == null)
                throw new ArgumentNullException(nameof(oldParameter));

            if (newParameter == null)
                throw new ArgumentNullException(nameof(newParameter));

            context.Direction = DataDirection.Request;

            base.Compare(context, oldParameter, newParameter);

            var areParametersReferenced = false;

            if (!string.IsNullOrWhiteSpace(oldParameter.Reference?.ReferenceV3))
            {
                oldParameter = FindReferencedParameter(oldParameter.Reference, context.OldOpenApiDocument.Components.Parameters);
                areParametersReferenced = true;
                if (oldParameter == null)
                    return;
            }
            if (!string.IsNullOrWhiteSpace(newParameter.Reference?.ReferenceV3))
            {
                newParameter = FindReferencedParameter(newParameter.Reference, context.NewOpenApiDocument.Components.Parameters);
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

            CompareConstantStatus(context, oldParameter, newParameter);

            CompareRequiredStatus(context, oldParameter, newParameter);

            CompareStyle(context, oldParameter, newParameter);

            CompareSchema(context, oldParameter.Schema, newParameter.Schema);

            _contentComparator.Compare(context, oldParameter.Content, newParameter.Content);

            context.Direction = DataDirection.None;
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
            if (parameters == null || reference == null || !reference.IsLocal)
                return null;

            var parts = reference.ReferenceV3.Split('/');
            var isPathToParameter = parts.Length == 4 && parts[1].Equals("components") && parts[2].Equals("parameters");
            if (!isPathToParameter)
                return null;

            if (parameters.TryGetValue(parts[3], out var parameter))
                return parameter;

            return null;
        }
    }
}
