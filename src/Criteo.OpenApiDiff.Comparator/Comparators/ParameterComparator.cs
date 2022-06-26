// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Criteo.OpenApiDiff.Comparator.Comparators.Extensions;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApiDiff.Comparator.Comparators
{
    /// <summary>
    /// Describes a single operation parameter.
    /// https://github.com/wordnik/swagger-spec/blob/master/versions/2.0.md#parameterObject
    /// </summary>
    public class ParameterComparator : ComponentComparator
    {
        private readonly SchemaComparator _schemaComparator;
        private readonly ContentComparator _contentComparator;

        private readonly LinkedList<OpenApiParameter> _visitedParameters;

        public ParameterComparator(SchemaComparator schemaComparator, ContentComparator contentComparator)
        {
            _schemaComparator = schemaComparator;
            _contentComparator = contentComparator;
            _visitedParameters = new LinkedList<OpenApiParameter>();
        }

        /// <summary>
        /// Compare a modified document node (this) to a previous one and look for breaking as well as non-breaking changes.
        /// </summary>
        /// <param name="context">The modified document context.</param>
        /// <param name="oldParameter">The SwaggerParameter from the original document model.</param>
        /// <param name="newParameter">The SwaggerParameter from the original document model.</param>
        /// <returns>A list of messages from the comparison.</returns>
        public IEnumerable<ComparisonMessage> Compare(
            ComparisonContext<OpenApiDocument> context,
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
                    return context.Messages;
            }
            if (!string.IsNullOrWhiteSpace(newParameter.Reference?.ReferenceV3))
            {
                newParameter = FindReferencedParameter(newParameter.Reference, context.NewOpenApiDocument.Components.Parameters);
                areParametersReferenced = true;
                if (newParameter == null)
                    return context.Messages;
            }

            if (areParametersReferenced)
            {
                if (_visitedParameters.Contains(oldParameter))
                    return context.Messages;

                _visitedParameters.AddFirst(oldParameter);
            }

            CompareIn(context, oldParameter.In, newParameter.In);

            CompareConstantStatus(context, oldParameter, newParameter);

            CompareRequiredStatus(context, oldParameter, newParameter);

            CompareStyle(context, oldParameter, newParameter);

            CompareSchema(context, oldParameter.Schema, newParameter.Schema);

            _contentComparator.Compare(context, oldParameter.Content, newParameter.Content);

            context.Direction = DataDirection.None;

            return context.Messages;
        }

        private static void CompareIn(ComparisonContext<OpenApiDocument> context,
            ParameterLocation? oldIn,
            ParameterLocation? newIn)
        {
            if (oldIn != newIn)
            {
                context.PushProperty("in");
                context.LogBreakingChange(ComparisonMessages.ParameterInHasChanged,
                    oldIn.ToString().ToLower(),
                    newIn.ToString().ToLower()
                );
                context.Pop();
            }
        }

        private static void CompareConstantStatus(ComparisonContext<OpenApiDocument> context,
            OpenApiParameter oldParameter,
            OpenApiParameter newParameter)
        {
            if (newParameter.IsConstant() != oldParameter.IsConstant())
            {
                context.PushProperty("enum");
                context.LogBreakingChange(ComparisonMessages.ConstantStatusHasChanged);
                context.Pop();
            }
        }

        private static void CompareRequiredStatus(ComparisonContext<OpenApiDocument> context,
            OpenApiParameter oldParameter, OpenApiParameter newParameter)
        {
            if (oldParameter.IsRequired() == newParameter.IsRequired() || context.Direction == DataDirection.Response)
                return;

            context.PushProperty("required");
            if (newParameter.IsRequired())
            {
                context.LogBreakingChange(ComparisonMessages.RequiredStatusChange, false, true);
            }
            else
            {
                context.LogInfo(ComparisonMessages.RequiredStatusChange, true, false);
            }
            context.Pop();
        }

        private static void CompareStyle(ComparisonContext<OpenApiDocument> context,
            OpenApiParameter oldParameter,
            OpenApiParameter newParameter)
        {
            if (oldParameter.Style != newParameter.Style)
            {
                context.PushProperty("style");
                context.LogBreakingChange(ComparisonMessages.ParameterStyleChanged, oldParameter.Name);
                context.Pop();
            }
        }

        private void CompareSchema(ComparisonContext<OpenApiDocument> context,
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
