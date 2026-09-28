#if NET10_0
using System.IO;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace Criteo.OpenApi.Comparator.Comparators.Extensions
{
    internal static class OpenApiExtensionExtensions
    {
        internal static JsonNode ToJsonNode(this IOpenApiExtension extension)
        {
            using var textWriter = new StringWriter();
            extension.Write(new OpenApiJsonWriter(textWriter), OpenApiSpecVersion.OpenApi3_0);
            return JsonNode.Parse(textWriter.ToString());
        }
    }
}
#endif
