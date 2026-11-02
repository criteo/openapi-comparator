// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi;

namespace Criteo.OpenApi.Comparator.Comparators.Extensions
{
    internal static class OpenApiReferenceExtensions
    {
        internal static BaseOpenApiReference GetReference(this IOpenApiReferenceable component) =>
            component switch
            {
                OpenApiSchemaReference reference => reference.Reference,
                OpenApiParameterReference reference => reference.Reference,
                OpenApiResponseReference reference => reference.Reference,
                OpenApiRequestBodyReference reference => reference.Reference,
                OpenApiHeaderReference reference => reference.Reference,
                OpenApiPathItemReference reference => reference.Reference,
                _ => null
            };

        /// <summary>
        /// Retrieve a parameter from the components/parameters section.
        /// </summary>
        /// <param name="reference">A document-relative reference object -- #/components/parameters/XXX</param>
        /// <param name="parameters">The parameters dictionary to use</param>
        internal static IOpenApiParameter Resolve(this BaseOpenApiReference reference,
            IDictionary<string, IOpenApiParameter> parameters)
        {
            if (reference == null || parameters == null || !reference.IsLocal)
                return null;

            if (!reference.IsPathToParameter())
                return null;

            if (parameters.TryGetValue(reference.GetLastPathElement(), out var parameter))
                return parameter;

            return null;
        }

        /// <summary>
        /// Retrieve a schema from the components/schemas section.
        /// </summary>
        /// <param name="reference">A document-relative reference object -- #/components/schemas/XXX</param>
        /// <param name="schemas">The schemas dictionary to use</param>
        internal static IOpenApiSchema Resolve(this BaseOpenApiReference reference, IDictionary<string, IOpenApiSchema> schemas)
        {
            if (reference == null || schemas == null || !reference.IsLocal)
                return null;

            if (!reference.IsPathToSchema())
                return null;

            return schemas.TryGetValue(reference.GetLastPathElement(), out var schema) ? schema : null;
        }

        /// <summary>
        /// Retrieve a response from the components/responses section.
        /// </summary>
        /// <param name="reference">A document-relative reference object -- #/components/responses/XXX</param>
        /// <param name="responses">The responses dictionary to use</param>
        internal static IOpenApiResponse Resolve(this BaseOpenApiReference reference, IDictionary<string, IOpenApiResponse> responses)
        {
            if (reference == null || responses == null || !reference.IsLocal)
                return null;

            if (!reference.IsPathToResponse())
                return null;

            return responses.TryGetValue(reference.GetLastPathElement(), out var response) ? response : null;
        }

        /// <summary>
        /// Retrieve a requestBody from the components/requestBodies section.
        /// </summary>
        /// <param name="reference">A document-relative reference object -- #/components/requestBodies/XXX</param>
        /// <param name="requestBodies">The requestBodies dictionary to use</param>
        internal static IOpenApiRequestBody Resolve(this BaseOpenApiReference reference, IDictionary<string, IOpenApiRequestBody> requestBodies)
        {
            if (reference == null || requestBodies == null || !reference.IsLocal)
                return null;

            if (!reference.IsPathToRequestBody())
                return null;

            return requestBodies.TryGetValue(reference.GetLastPathElement(), out var requestBody) ? requestBody : null;
        }

        private static bool IsPathToParameter(this BaseOpenApiReference reference)
        {
            var parts = reference.ReferenceV3.Split('/');
            return parts.Length == 4 && parts[1].Equals("components") && parts[2].Equals("parameters");
        }

        private static bool IsPathToSchema(this BaseOpenApiReference reference)
        {
            var parts = reference.ReferenceV3.Split('/');
            return parts.Length == 4 && parts[1].Equals("components") && parts[2].Equals("schemas");
        }

        private static bool IsPathToResponse(this BaseOpenApiReference reference)
        {
            var parts = reference.ReferenceV3.Split('/');
            return parts.Length == 4 && parts[1].Equals("components") && parts[2].Equals("responses");
        }

        private static bool IsPathToRequestBody(this BaseOpenApiReference reference)
        {
            var parts = reference.ReferenceV3.Split('/');
            return parts.Length == 4 && parts[1].Equals("components") && parts[2].Equals("requestBodies");
        }

        private static string GetLastPathElement(this BaseOpenApiReference reference) =>
            reference.ReferenceV3.Split('/').Last();
    }
}
