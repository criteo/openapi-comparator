// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Criteo.OpenApi.Comparator.Parser;

namespace Criteo.OpenApi.Comparator.Logging
{
    internal class ObjectPath
    {
        internal static ObjectPath Empty => new ObjectPath(Enumerable.Empty<Func<JToken, string>>());

        private ObjectPath(IEnumerable<Func<JToken, string>> path)
        {
            Path = path;
        }

        private ObjectPath Append(Func<JToken, string> function) => new ObjectPath(Path.Concat(new[] { function }));

        internal ObjectPath AppendProperty(string property) => Append(_ => property);

        /// <summary>
        /// Used to add a server's index according to its url.
        /// </summary>
        /// <param name="serverUrl">The "url" attribute of the server we are looking for</param>
        /// <returns>The index of the server in the servers list</returns>
        internal ObjectPath AppendServerByUrl(string serverUrl) => Append(FindServerIndex(serverUrl));

        private static Func<JToken, string> FindServerIndex(string serverUrl) => token =>
        {
            for (var index = 0; index < token?.Count(); index++)
            {
                var tokenUrl = token[index]?["url"];
                if (tokenUrl != null && tokenUrl.Value<string>() == serverUrl)
                    return index.ToString();
            }
            return null;
        };

        /// <summary>
        /// Adding an Open API parameter's index in the path.
        /// </summary>
        internal ObjectPath AppendParameterByName(string parameterName) => Append(FindParameterIndex(parameterName));

        /// <summary>
        /// Used to find a parameter's index based on its name.
        /// If the parameter is referenced it starts by resolving it.
        /// </summary>
        /// <param name="parameterName">The "name" of the parameter we are looking for</param>
        /// <returns>The index of the parameter in the list</returns>
        private static Func<JToken, string> FindParameterIndex(string parameterName) => token =>
        {
            for (var index = 0; index < token?.Count(); index++)
            {
                var tokenElement = FindTokenFromReference(token[index]);

                if (tokenElement?["name"]?.Value<string>() == parameterName)
                    return index.ToString();
            }
            return null;
        };

        /// <summary>
        /// This is the OpenAPI path name. To use it as an id we need to remove all parameter names.
        /// For example, "/a/{a}/" and "/a/{b}" are the same paths.
        /// </summary>
        internal static string OpenApiPathName(string path) => Regex.Replace(path, @"\{\w*\}", @"{}");

        /// <summary>
        /// Find the actual value of the OpenApi Path. For example, given the path /pets/{},
        /// it will return the actual value of the path in the specification: /pets/{id}
        /// </summary>
        private static Func<JToken, string> FindPathValue(string pathWithoutParameters) => token =>
            (token as JObject)?.Properties()?
                .FirstOrDefault(property => OpenApiPathName(property.Name) == pathWithoutParameters)?.Name;

        /// <summary>
        /// Adding an Open API path.
        /// </summary>
        internal ObjectPath AppendPathProperty(string openApiPath) => Append(FindPathValue(openApiPath));

        private IEnumerable<Func<JToken, string>> Path { get; }

        private static ObjectPath ParseRef(string s) =>
            new ObjectPath(s.Split('/')
                .Where(v => v != "#")
                .Select<string, Func<JToken, string>>(pathElement =>
                    _ => pathElement.Replace("~1", "/")
                        .Replace("~0", "~")
                    )
            );

        private static JToken FromObject(JObject jsonObject, string name)
        {
            if (name == null)
                return null;

            var reference = FindTokenFromReference(jsonObject);
            return reference[name];
        }

        private static JToken FindTokenFromReference(JToken token)
        {
            var reference = token?["$ref"];
            if (reference != null)
                token = ParseRef(reference.Value<string>()).CompletePath(token.Root).Last().token;
            return token;
        }

        private static IEnumerable<(JToken token, string name)> CompletePath(IEnumerable<Func<JToken, string>> path, JToken token) =>
            new[] { (token, "#") }.Concat(path.Select(toJsonRefElement =>
                {
                    var name = toJsonRefElement(token);
                    token = token is JArray jsonArray
                        ? int.TryParse(name, out var i)
                            ? jsonArray[i]
                            : null
                        : token is JObject o
                            ? FromObject(o, name)
                            : null;
                    return (token, name);
                }));

        /// <summary>
        /// Returns a sequence of property names, including the "#" string.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        internal IEnumerable<(JToken token, string name)> CompletePath(JToken t) => CompletePath(Path, t);

        /// <summary>
        /// Normalize file path by replacing '\\' by '/'
        /// </summary>
        internal static string FileNameNorm(string fileName) => fileName.Replace("\\", "/");

        /// <summary>
        /// https://tools.ietf.org/html/draft-ietf-appsawg-json-pointer-04
        /// </summary>
        /// <param name="jsonDocument"></param>
        /// <returns>The json path of the json document</returns>
        internal string JsonPointer(IJsonDocument jsonDocument)
        {
            return CompletePath(jsonDocument.Token)
                .Select(v => v.name?
                    .Replace("~", "~0")
                    .Replace("/", "~1")
                )
                .Aggregate((a, b) => a == null || b == null ? null : a + "/" + b);
        }
    }
}
