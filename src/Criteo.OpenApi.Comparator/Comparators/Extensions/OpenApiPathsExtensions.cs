// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using ParameterLocation = Microsoft.OpenApi.ParameterLocation;

namespace Criteo.OpenApi.Comparator.Comparators.Extensions
{
    internal static class OpenApiPathsExtensions
    {
        private static readonly IDictionary<string, HttpMethod> _operationTypesMap =
            new Dictionary<string, HttpMethod>
            {
                ["get"] = HttpMethod.Get,
                ["post"] = HttpMethod.Post,
                ["put"] = HttpMethod.Put,
                ["delete"] = HttpMethod.Delete,
                ["head"] = HttpMethod.Head,
                ["patch"] = HttpMethod.Patch,
                ["trace"] = HttpMethod.Trace,
                ["options"] = HttpMethod.Options,
            };

        internal static OpenApiPaths ToOpenApiPaths(this JsonObject rawPaths)
        {
            var paths = new OpenApiPaths();

            foreach (var path in rawPaths)
            {
                paths.Add(path.Key, (path.Value as JsonObject)?.ToOpenApiPathItem());
            }

            return paths;
        }

        private static ParameterLocation ToParameterLocation(this string location)
        {
            switch (location?.ToLower())
            {
                case "path":
                    return ParameterLocation.Path;
                case "query":
                    return ParameterLocation.Query;
                case "header":
                    return ParameterLocation.Header;
                case "cookie":
                    return ParameterLocation.Cookie;
                default:
                    throw new OpenApiException($"Invalid parameter location: {location}");
            }
        }

        private static OpenApiDiscriminator ToDiscriminator(this JsonObject discriminatorAsObject)
        {
            var discriminator = new OpenApiDiscriminator();
            discriminatorAsObject.TryGetPropertyValue("propertyName", out var propertyName);
            if (propertyName == null)
                throw new OpenApiException("\"propertyName\" attribute should be provided in a discriminator object");
            discriminator.PropertyName = propertyName.JsonString();
            return discriminator;
        }

        private static string ToReferenceId(this string reference) => reference?.Split('/').Last();

        private static IList<IOpenApiSchema> ToAllOf(this JsonArray allOf) =>
            allOf.Select(allOfElement => (allOfElement as JsonObject)?.ToSchema()).ToList();

        private static IDictionary<string, IOpenApiSchema> ToProperties(this JsonObject propertiesAsObject)
        {
            var properties = new Dictionary<string, IOpenApiSchema>();

            foreach (var property in propertiesAsObject)
            {
                properties.Add(property.Key, (property.Value as JsonObject)?.ToSchema());
            }

            return properties;
        }

        private static IOpenApiSchema ToSchema(this JsonObject schemaAsObject)
        {
            var schema = new OpenApiSchema();
            foreach (var schemaObject in schemaAsObject)
            {
                switch (schemaObject.Key)
                {
                    case "$ref":
                        return new OpenApiSchemaReference(schemaObject.Value.JsonString().ToReferenceId());
                    case "readOnly":
                        schema.ReadOnly = schemaObject.Value.JsonBoolean() ?? false;
                        continue;
                    case "discriminator":
                        schema.Discriminator = (schemaObject.Value as JsonObject)?.ToDiscriminator();
                        continue;
                    case "default":
                        schema.Default = schemaObject.Value;
                        continue;
                    case "type":
                        schema.Type = OpenApiTypeMapper.ToJsonSchemaType(schemaObject.Value.JsonString());
                        continue;
                    case "items":
                        schema.Items = (schemaObject.Value as JsonObject)?.ToSchema();
                        continue;
                    case "enum":
                        schema.Enum = (schemaObject.Value as JsonArray)?.ToList();
                        continue;
                    case "format":
                        schema.Format = schemaObject.Value.JsonString();
                        continue;
                    case "allOf":
                        schema.AllOf = (schemaObject.Value as JsonArray)?.ToAllOf();
                        continue;
                    case "properties":
                        schema.Properties = (schemaObject.Value as JsonObject)?.ToProperties();
                        continue;
                    case "required":
                        schema.Required = (schemaObject.Value as JsonArray)?.ToRequiredArray();
                        continue;
                }
            }

            return schema;
        }

        private static ISet<string> ToRequiredArray(this JsonArray array) =>
            array.Select(requiredProperty => requiredProperty.JsonString()).ToHashSet();

        private static IOpenApiParameter ToParameter(this JsonObject parametersAsObject)
        {
            var parameter = new OpenApiParameter();

            foreach (var parameterAsObject in parametersAsObject)
            {
                switch (parameterAsObject.Key)
                {
                    case "$ref":
                        return new OpenApiParameterReference(parameterAsObject.Value.JsonString().ToReferenceId());
                    case "name":
                        parameter.Name = parameterAsObject.Value.JsonString();
                        continue;
                    case "in":
                        parameter.In = parameterAsObject.Value.JsonString().ToParameterLocation();
                        continue;
                    case "required":
                        parameter.Required = parameterAsObject.Value.JsonBoolean() ?? false;
                        continue;
                    case "schema":
                        parameter.Schema = (parameterAsObject.Value as JsonObject)?.ToSchema();
                        continue;
                }
            }

            return parameter;
        }

        private static IList<IOpenApiParameter> ToParameters(this JsonArray parametersAsObject)
        {
            var parameters = new List<IOpenApiParameter>();

            foreach (var parameterAsObject in parametersAsObject)
            {
                parameters.Add((parameterAsObject as JsonObject)?.ToParameter());
            }

            return parameters;
        }

        private static IOpenApiRequestBody ToRequestBody(this JsonObject responsesAsObject)
        {
            var requestBody = new OpenApiRequestBody();

            foreach (var responseAsObject in responsesAsObject)
            {
                switch (responseAsObject.Key)
                {
                    case "content":
                        requestBody.Content = (responseAsObject.Value as JsonObject)?.ToContent();
                        continue;
                    case "required":
                        requestBody.Required = responseAsObject.Value.JsonBoolean() ?? false;
                        continue;
                    case "$ref":
                        return new OpenApiRequestBodyReference(responseAsObject.Value.JsonString().ToReferenceId());
                }
            }

            return requestBody;
        }

        private static OpenApiMediaType ToMediaTypeObject(this JsonObject mediaTypeAsObject)
        {
            var response = new OpenApiMediaType();

            foreach (var mediaType in mediaTypeAsObject)
            {
                switch (mediaType.Key)
                {
                    case "schema":
                        response.Schema = (mediaType.Value as JsonObject)?.ToSchema();
                        continue;
                }
            }

            return response;
        }

        private static IDictionary<string, OpenApiMediaType> ToContent(this JsonObject mediaTypeAsObject)
        {
            var response = new Dictionary<string, OpenApiMediaType>();

            foreach (var mediaType in mediaTypeAsObject)
            {
                response.Add(mediaType.Key, (mediaType.Value as JsonObject)?.ToMediaTypeObject());
            }

            return response;
        }

        private static IOpenApiResponse ToResponse(this JsonObject responseAsObject)
        {
            var response = new OpenApiResponse();

            foreach (var responseAttribute in responseAsObject)
            {
                switch (responseAttribute.Key)
                {
                    case "$ref":
                        return new OpenApiResponseReference(responseAttribute.Value.JsonString().ToReferenceId());
                    case "content":
                        response.Content = (responseAttribute.Value as JsonObject)?.ToContent();
                        continue;
                }
            }

            return response;
        }

        private static OpenApiResponses ToResponses(this JsonObject responsesAsObject)
        {
            var responses = new OpenApiResponses();

            foreach (var responseAsObject in responsesAsObject)
            {
                responses.Add(responseAsObject.Key, (responseAsObject.Value as JsonObject)?.ToResponse());
            }

            return responses;
        }

        private static OpenApiOperation ToOperation(this JsonObject operationAsObject)
        {
            var operation = new OpenApiOperation();

            foreach (var operationProperty in operationAsObject)
            {
                switch (operationProperty.Key)
                {
                    case "operationId":
                        operation.OperationId = operationProperty.Value.JsonString();
                        continue;
                    case "parameters":
                        operation.Parameters = (operationProperty.Value as JsonArray)?.ToParameters();
                        continue;
                    case "requestBody":
                        operation.RequestBody = (operationProperty.Value as JsonObject)?.ToRequestBody();
                        continue;
                    case "responses":
                        operation.Responses = (operationProperty.Value as JsonObject)?.ToResponses();
                        continue;
                }
            }

            return operation;
        }

        private static OpenApiPathItem ToOpenApiPathItem(this JsonObject rawPathItem)
        {
            var pathItem = new OpenApiPathItem { Operations = new Dictionary<HttpMethod, OpenApiOperation>() };

            foreach (var operation in rawPathItem)
            {
                if (_operationTypesMap.ContainsKey(operation.Key))
                {
                    pathItem.Operations.Add(_operationTypesMap[operation.Key], (operation.Value as JsonObject)?.ToOperation());
                }
                else
                {
                    switch (operation.Key)
                    {
                        case "parameters":
                            pathItem.Parameters = (operation.Value as JsonArray)?.ToParameters();
                            continue;
                    }
                }
            }

            return pathItem;
        }
    }
}
