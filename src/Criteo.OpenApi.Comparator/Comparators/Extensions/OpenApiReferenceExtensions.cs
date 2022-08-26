using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators.Extensions
{
    public static class OpenApiReferenceExtensions
    {
        /// <summary>
        /// Retrieve a parameter from the components/parameters section.
        /// </summary>
        /// <param name="reference">A document-relative reference object -- #/components/parameters/XXX</param>
        /// <param name="parameters">The parameters dictionary to use</param>
        public static OpenApiParameter Resolve(this OpenApiReference reference,
            IDictionary<string, OpenApiParameter> parameters)
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
        public static OpenApiSchema Resolve(this OpenApiReference reference, IDictionary<string, OpenApiSchema> schemas)
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
        public static OpenApiResponse Resolve(this OpenApiReference reference, IDictionary<string, OpenApiResponse> responses)
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
        public static OpenApiRequestBody Resolve(this OpenApiReference reference, IDictionary<string, OpenApiRequestBody> requestBodies)
        {
            if (reference == null || requestBodies == null || !reference.IsLocal)
                return null;

            if (!reference.IsPathToRequestBody())
                return null;

            return requestBodies.TryGetValue(reference.GetLastPathElement(), out var requestBody) ? requestBody : null;
        }

        private static bool IsPathToParameter(this OpenApiReference reference)
        {
            var parts = reference.ReferenceV3.Split('/');
            return parts.Length == 4 && parts[1].Equals("components") && parts[2].Equals("parameters");
        }

        private static bool IsPathToSchema(this OpenApiReference reference)
        {
            var parts = reference.ReferenceV3.Split('/');
            return parts.Length == 4 && parts[1].Equals("components") && parts[2].Equals("schemas");
        }

        private static bool IsPathToResponse(this OpenApiReference reference)
        {
            var parts = reference.ReferenceV3.Split('/');
            return parts.Length == 4 && parts[1].Equals("components") && parts[2].Equals("responses");
        }

        private static bool IsPathToRequestBody(this OpenApiReference reference)
        {
            var parts = reference.ReferenceV3.Split('/');
            return parts.Length == 4 && parts[1].Equals("components") && parts[2].Equals("requestBodies");
        }

        private static string GetLastPathElement(this OpenApiReference reference) =>
            reference.ReferenceV3.Split('/').Last();
    }
}
