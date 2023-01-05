using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators.Extensions
{
    internal static class OpenApiParameterExtension
    {
        internal static bool IsConstant(this OpenApiParameter parameter) =>
            parameter.IsRequired() && parameter.HasEnumWithSingleValue();

        internal static bool IsRequired(this OpenApiParameter parameter) =>
            parameter.Required || parameter.In == ParameterLocation.Path;

        private static bool HasEnumWithSingleValue(this OpenApiParameter parameter) =>
            parameter.Schema?.Enum != null && parameter.Schema.Enum.Count == 1;
    }
}
