using Newtonsoft.Json.Linq;

namespace Criteo.OpenApi.Comparator.Parser
{
    internal interface IJsonDocument
    {
        /// JSON object
        JToken Token { get; }

        /// JSON file name
        string FileName { get; }
    }

    internal sealed class JsonDocument<T> : IJsonDocument
    {
        /// <summary>
        /// Untyped raw parsed JSON. The Token also includes information about
        /// file location of each item.
        /// </summary>
        public JToken Token { get; }

        /// <summary>
        /// Representation of JSON as `T` type.
        /// </summary>
        public T Typed { get; }

        /// <summary>
        /// JSON file name.
        /// </summary>
        public string FileName { get; }

        public JsonDocument(JToken token, T typed, string fileName)
        {
            Token = token;
            Typed = typed;
            FileName = fileName;
        }
    }

    internal static class JsonDocument
    {
        /// <summary>
        /// Creates a `JsonDocument` object. It's a syntax sugar for `new JsonDocument`.
        /// </summary>
        public static JsonDocument<T> ToJsonDocument<T>(this JToken token, T typed, string fileName) =>
            new JsonDocument<T>(token, typed, fileName);
    }
}
