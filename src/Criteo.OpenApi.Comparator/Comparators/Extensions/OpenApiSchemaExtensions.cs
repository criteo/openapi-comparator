using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators.Extensions
{
    public static class OpenApiSchemaExtensions
    {
        public static bool IsPropertyRequired(this OpenApiSchema schema, string propertyName) =>
            schema.Required != null && schema.Required.Contains(propertyName);
    }
}
