using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators.Extensions
{
    internal static class OpenApiSchemaExtensions
    {
        internal static bool IsPropertyRequired(this OpenApiSchema schema, string propertyName) =>
            schema.Required != null && schema.Required.Contains(propertyName);
    }
}
