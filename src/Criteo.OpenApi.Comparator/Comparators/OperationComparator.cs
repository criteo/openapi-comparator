// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using Criteo.OpenApi.Comparator.Comparators.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators
{
    /// <summary>
    /// Describes a single API operation on a path.
    /// </summary>
    internal class OperationComparator
    {
        private readonly ParameterComparator _parameter;
        private readonly RequestBodyComparator _requestBody;
        private readonly ResponseComparator _response;

        internal OperationComparator(ParameterComparator parameter,
            RequestBodyComparator requestBody,
            ResponseComparator response)
        {
            _parameter = parameter;
            _requestBody = requestBody;
            _response = response;
        }
        /// <summary>
        /// Compare a modified document node (this) to a previous one and look for breaking as well as non-breaking changes.
        /// </summary>
        /// <param name="context">The modified document context.</param>
        /// <param name="oldOperation">The original operation.</param>
        /// <param name="newOperation">The new operation.</param>
        /// <returns>A list of messages from the comparison.</returns>
        internal void Compare(ComparisonContext context,
            OpenApiOperation oldOperation,
            OpenApiOperation newOperation)
        {
            if (oldOperation == null)
                throw new ArgumentException(nameof(oldOperation));

            if (newOperation == null)
                throw new ArgumentException(nameof(newOperation));

            if (newOperation.OperationId != oldOperation.OperationId)
            {
                context.PushProperty("operationId");
                context.Log(ComparisonRules.ModifiedOperationId, oldOperation.OperationId, newOperation.OperationId);
                context.Pop();
            }

            CompareParameters(context, oldOperation.Parameters, newOperation.Parameters);

            CompareResponses(context, oldOperation.Responses, newOperation.Responses);

            CompareRequestBody(context, oldOperation.RequestBody, newOperation.RequestBody);

            CompareExtensions(context, oldOperation.Extensions, newOperation.Extensions);
        }

        /// <summary>
        /// Check that no parameters were removed or reordered, and compare them if it's not the case
        /// </summary>
        /// <param name="context">Comparision Context</param>
        /// <param name="oldParameters">Old Operation's parameters</param>
        /// <param name="newParameters">New Operation's parameters</param>
        internal void CompareParameters(ComparisonContext context,
            IList<OpenApiParameter> oldParameters,
            IList<OpenApiParameter> newParameters)
        {
            var oldDocument = context.OldOpenApiDocument;
            var newDocument = context.NewOpenApiDocument;

            context.PushProperty("parameters");

            oldParameters = oldParameters.Select(oldParameter =>
                string.IsNullOrWhiteSpace(oldParameter.Reference?.ReferenceV3)
                    ? oldParameter
                    : oldParameter.Reference.Resolve(oldDocument.Components.Parameters)
            ).ToList();

            newParameters = newParameters.Select(newParameter =>
                string.IsNullOrWhiteSpace(newParameter.Reference?.ReferenceV3)
                    ? newParameter
                    : newParameter.Reference.Resolve(newDocument.Components.Parameters)
            ).ToList();

            CompareParametersOrder(context, oldParameters, newParameters);

            CheckRequiredParametersRemoval(context, oldParameters, newParameters);

            CheckRequiredParametersAddition(context, oldParameters, newParameters);
        }
        private static void CompareParametersOrder(ComparisonContext context,
            IList<OpenApiParameter> oldParameters,
            IList<OpenApiParameter> newParameters)
        {
            newParameters = newParameters.Select(parameter =>
                string.IsNullOrWhiteSpace(parameter.Reference?.ReferenceV3)
                    ? parameter
                    : parameter.Reference.Resolve(context.NewOpenApiDocument.Components.Parameters)
                ).ToList();

            for (var i = 0; i < newParameters.Count(); i++)
            {
                var newParameter = newParameters.ElementAt(i);

                if (newParameter.In == ParameterLocation.Path) continue;

                var priorIndex = FindParameterIndex(newParameter, oldParameters);

                if (OrderHasChanged(priorIndex, i))
                {
                    context.Log(ComparisonRules.ChangedParameterOrder, newParameter.Name);
                }
            }
        }

        private static bool OrderHasChanged(int priorIndex, int newIndex) => priorIndex != -1 && priorIndex != newIndex;


        private void CheckRequiredParametersRemoval(ComparisonContext context,
            IEnumerable<OpenApiParameter> oldParameters,
            IList<OpenApiParameter> newParameters)
        {
            foreach (var oldParameter in oldParameters)
            {
                var newParameter = FindParameter(oldParameter, newParameters, context.NewOpenApiDocument.Components?.Parameters);

                // we should use PushItemByName instead of PushProperty because Swagger `parameters` is
                // an array of parameters.
                context.PushParameterByName(oldParameter.Name);

                if (newParameter != null)
                {
                    _parameter.Compare(context, oldParameter, newParameter);
                }
                else if (oldParameter.Required || oldParameter.In == ParameterLocation.Path)
                {
                    // Removed required parameter
                    context.Log(ComparisonRules.RemovedRequiredParameter, oldParameter.Name);
                }

                context.Pop();
            }
        }

        private static void CheckRequiredParametersAddition(ComparisonContext context,
            IList<OpenApiParameter> oldParameters,
            IList<OpenApiParameter> newParameters)
        {
            // Check that no parameters were added.
            newParameters = newParameters
                .Select(parameter => string.IsNullOrWhiteSpace(parameter.Reference?.ReferenceV3)
                        ? parameter
                        : parameter.Reference.Resolve(context.NewOpenApiDocument.Components.Parameters)
                )
                .Where(parameter => parameter != null)
                .ToList();

            foreach (var newParameter in newParameters)
            {
                OpenApiParameter oldParameter = FindParameter(
                    newParameter,
                    oldParameters,
                    context.OldOpenApiDocument.Components?.Parameters
                );

                if (oldParameter == null)
                {
                    // Did not find required parameter in the old swagger i.e required parameter is added
                    context.PushParameterByName(newParameter.Name);
                    context.Log(
                        newParameter.Required || newParameter.In == ParameterLocation.Path
                            ? ComparisonRules.AddingRequiredParameter
                            : ComparisonRules.AddingOptionalParameter, newParameter.Name);
                    context.Pop();
                }
            }
            context.Pop();
        }

        private void CompareResponses(ComparisonContext context,
            OpenApiResponses oldResponses,
            OpenApiResponses newResponses)
        {
            if (newResponses == null || oldResponses == null)
                return;

            context.PushProperty("responses");
            var addedResponseCodes = newResponses.Keys.Where(statusCode => !oldResponses.ContainsKey(statusCode));
            foreach (var statusCode in addedResponseCodes)
            {
                context.PushProperty(statusCode);
                context.Log(ComparisonRules.AddingResponseCode, statusCode);
                context.Pop();
            }

            var removedResponseCodes = oldResponses.Keys.Where(statusCode => !newResponses.ContainsKey(statusCode));
            foreach (var statusCode in removedResponseCodes)
            {
                context.PushProperty(statusCode);
                context.Log(ComparisonRules.RemovedResponseCode, statusCode);
                context.Pop();
            }

            var commonResponses = oldResponses.Where(response => newResponses.ContainsKey(response.Key));
            foreach (var response in commonResponses)
            {
                context.PushProperty(response.Key);
                _response.Compare(context, response.Value, newResponses[response.Key]);
                context.Pop();
            }
            context.Pop();
        }

        private void CompareRequestBody(ComparisonContext context,
            OpenApiRequestBody oldRequestBody, OpenApiRequestBody newRequestBody)
        {
            context.PushProperty("requestBody");
            _requestBody.Compare(context, oldRequestBody, newRequestBody);
            context.Pop();
        }

        private static void CompareExtensions(ComparisonContext context,
            IDictionary<string, IOpenApiExtension> oldExtensions,
            IDictionary<string, IOpenApiExtension> newExtensions)
        {
            const string longRunningOperationExtension = "x-ms-long-running-operation";
            oldExtensions.TryGetValue(longRunningOperationExtension, out var oldLongRunningOperationValue);
            newExtensions.TryGetValue(longRunningOperationExtension, out var newLongRunningOperationValue);

            if (oldLongRunningOperationValue == newLongRunningOperationValue)
                return;

            context.PushProperty(longRunningOperationExtension);
            context.Log(ComparisonRules.LongRunningOperationExtensionChanged);
            context.Pop();
        }

        private static int FindParameterIndex(OpenApiParameter parameter, IList<OpenApiParameter> operationParameters)
        {
            for (var index = 0; index < operationParameters.Count(); index++)
            {
                if (operationParameters.ElementAt(index).Name == parameter.Name
                    && operationParameters.ElementAt(index).In == parameter.In)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Finds parameter name in the list of operation parameters or global parameters
        /// </summary>
        /// <param name="key">The parameter used as a key. It is expected that this parameter comes from the opposite
        /// file. The parameter that has the same name and is most similar to this parameter will be returned</param>
        /// <param name="operationParameters">list of operation parameters to search</param>
        /// <param name="documentParameters">Dictionary of global parameters to search</param>
        /// <returns>Swagger Parameter if found; otherwise null</returns>
        private static OpenApiParameter FindParameter(
            OpenApiParameter key,
            IEnumerable<OpenApiParameter> operationParameters,
            IDictionary<string, OpenApiParameter> documentParameters)
        {
            string name = key.Name;
            if (name == null || operationParameters == null)
                return null;

            var candidateParameters = new List<OpenApiParameter>();
            foreach (var parameter in operationParameters)
            {
                if (name.Equals(parameter.Name))
                    candidateParameters.Add(parameter);
                else
                {
                    var referencedParameter = parameter.Reference.Resolve(documentParameters);

                    if (referencedParameter != null && name.Equals(referencedParameter.Name))
                        candidateParameters.Add(referencedParameter);
                }
            }

            if (!candidateParameters.Any())
                return null;

            if(candidateParameters.Count == 1)
                return candidateParameters[0];

            return GetClosestFunctionally(key, candidateParameters);
        }

        /// <summary>
        /// Determines the most similar parameter to the key parameter based on functional similarity.
        /// </summary>
        /// <param name="key">The parameter to score against for functional similarity</param>
        /// <param name="candidateParameters">The candidates. One of these will be selected as the most similar</param>
        /// <returns>The most similar parameter functionally to the key parameter</returns>
        private static OpenApiParameter GetClosestFunctionally(OpenApiParameter key, List<OpenApiParameter> candidateParameters)
        {
            OpenApiParameter bestMatch = null;
            var bestMatchScore = 0;

            foreach (var candidate in candidateParameters)
            {
                var score = 0;

                score += key.Reference == candidate.Reference ? 1 : 0;
                score += key.In == candidate.In ? 1 : 0;
                score += key.Required == candidate.Required ? 1 : 0;
                score += key.Deprecated == candidate.Deprecated ? 1 : 0;
                score += key.AllowEmptyValue == candidate.AllowEmptyValue ? 1 : 0;
                score += key.Style == candidate.Style ? 1 : 0;
                score += key.Explode == candidate.Explode ? 1 : 0;
                score += key.AllowReserved == candidate.AllowReserved ? 1 : 0;

                if (score <= bestMatchScore)
                    continue;
                
                bestMatch = candidate;
                bestMatchScore = score;
            }

            return bestMatch;
        }
    }
}
