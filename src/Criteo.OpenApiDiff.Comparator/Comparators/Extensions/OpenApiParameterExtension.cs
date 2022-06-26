using Microsoft.OpenApi.Models;

namespace Criteo.OpenApiDiff.Comparator.Comparators.Extensions
{
    public static class OpenApiParameterExtension
    {
        public static bool IsConstant(this OpenApiParameter parameter) =>
            parameter.IsRequired() && parameter.HasEnumWithSingleValue();

        public static bool IsRequired(this OpenApiParameter parameter) =>
            parameter.Required || parameter.In == ParameterLocation.Path;

        private static bool HasEnumWithSingleValue(this OpenApiParameter parameter) =>
            parameter.Schema?.Enum != null && parameter.Schema.Enum.Count == 1;
    }
}
