// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Parser
{
    internal abstract class SwaggerJsonConverter : JsonConverter
    {
        protected JObject Document { get; set; }

        /// <summary>
        /// Writes the JSON representation of the object
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        protected JsonSerializerSettings GetSettings(JsonSerializer serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            var settings = new JsonSerializerSettings
            {
                MaxDepth = 128,
                TypeNameHandling = TypeNameHandling.None,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore
            };

            foreach (var converter in serializer.Converters)
            {
                if (converter != this)
                    settings.Converters.Add(converter);
            }

            return settings;
        }
    }

    /// <summary>
    /// This converter is used to merge the common parameters that do not exist a the operation level
    /// </summary>
    internal class PathLevelParameterConverter : SwaggerJsonConverter
    {
        internal PathLevelParameterConverter(string json)
        {
            Document = JObject.Parse(json);
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Dictionary<string, OpenApiOperation>));
        }

        /// <summary>
        /// To merge common parameters at the path level into the parameters at
        /// the operation level if they do not exist at the operation level
        /// </summary>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var swagger = JObject.Load(reader);
            if (swagger["parameters"] != null)
            {
                var commonParameters = new Dictionary<string, JObject>();
                foreach (var parameter in swagger["parameters"])
                {
                    var key = GetParameterKey(parameter as JObject);
                    commonParameters[key] = parameter as JObject;
                }

                var operations = swagger.Properties()
                    .Where(p => p.Name != "parameters")
                    .Select(p => p.Value as JObject);

                // Iterating over the operations to merge the common parameters if they do not exist
                foreach (var operation in operations)
                {
                    if (operation != null && operation["parameters"] == null)
                    {
                        operation["parameters"] = new JArray();
                    }

                    // Common parameters not existing in the operation's parameters
                    var missingParameters = commonParameters
                        .Where(parameter => operation["parameters"]
                            .All(p => GetParameterKey(p as JObject) != parameter.Key)
                        );

                    foreach (var parameter in missingParameters)
                    {
                        (operation["parameters"] as JArray).Add(commonParameters[parameter.Key]);
                    }
                }

                // Removing the common parameters to avoid serialization errors
                swagger.Remove("parameters");
            }

            var result = new Dictionary<string, OpenApiOperation>();

            foreach (var jToken in swagger.Children())
            {
                var operation = (JProperty) jToken;

                if (operation.Name == null || operation.Name.StartsWith("x-", StringComparison.InvariantCultureIgnoreCase)) continue;

                result[operation.Name] = JsonConvert.DeserializeObject<OpenApiOperation>(
                    operation.Value.ToString(),
                    GetSettings(serializer)
                );
            }

            return result;
        }

        /// <summary>
        /// Returns the value of the reference or the value of the name and location (query, path, body)
        /// </summary>
        private static string GetParameterKey(JObject param)
        {
            return (string) (param["$ref"] ?? param["name"] + "+" + param["in"]);
        }
    }
}
