using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Exceptions;
using Microsoft.OpenApi.Models;
using ParameterLocation = Microsoft.OpenApi.Models.ParameterLocation;

namespace Criteo.OpenApiDiff.Comparator.Comparators.Extensions
{
    public static class OpenApiPathsExtensions
    {
        private static readonly IDictionary<string, OperationType> _operationTypesMap =
            new Dictionary<string, OperationType>
            {
                ["get"] = OperationType.Get,
                ["post"] = OperationType.Post,
                ["put"] = OperationType.Put,
                ["delete"] = OperationType.Delete,
                ["head"] = OperationType.Head,
                ["patch"] =  OperationType.Patch,
                ["trace"] = OperationType.Trace,
                ["options"] = OperationType.Options,
            };

        public static OpenApiPaths ToOpenApiPaths(this OpenApiObject rawPaths)
        {
            var paths = new OpenApiPaths();

            foreach (var (path, pathItem) in rawPaths)
            {
                paths.Add(path, (pathItem as OpenApiObject)?.ToOpenApiPathItem());
            }
            return paths;
        }

        private static ParameterLocation ToParameterLocation(this string location)
        {
            switch (location.ToLower())
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

        private static OpenApiDiscriminator ToDiscriminator(this OpenApiObject discriminatorAsObject)
        {
            var discriminator = new OpenApiDiscriminator();
            discriminatorAsObject.TryGetValue("propertyName", out var propertyName);
            if (propertyName == null)
                throw new OpenApiException("\"propertyName\" attribute should be provided in a discriminator object");
            discriminator.PropertyName = (propertyName as OpenApiString)?.Value;
            return discriminator;
        }

        private static OpenApiReference ToReference(this string reference, ReferenceType type)
        {
            return new OpenApiReference
            {
                Id = reference,
                Type = type
            };
        }

        private static IList<OpenApiSchema> ToAllOf(this OpenApiArray allOf) =>
            allOf.Select(allOfElement => (allOfElement as OpenApiObject)?.ToSchema()).ToList();

        private static IDictionary<string, OpenApiSchema> ToProperties(this OpenApiObject propertiesAsObject)
        {
            var properties = new Dictionary<string, OpenApiSchema>();

            foreach (var (propertyName, property) in propertiesAsObject)
            {
                properties.Add(propertyName, (property as OpenApiObject)?.ToSchema());
            }
            return properties;
        }

        private static OpenApiSchema ToSchema(this OpenApiObject schemaAsObject)
        {
            var schema = new OpenApiSchema();
            foreach (var (schemaAttribute, value) in schemaAsObject)
            {
                switch (schemaAttribute)
                {
                    case "$ref":
                        schema.UnresolvedReference = true;
                        schema.Reference = (value as OpenApiString)?.Value.ToReference(ReferenceType.Schema);
                        return schema;
                    case "readOnly":
                        schema.ReadOnly = (value as OpenApiBoolean)?.Value ?? false;
                        continue;
                    case "discriminator":
                        schema.Discriminator = (value as OpenApiObject)?.ToDiscriminator();
                        continue;
                    case "default":
                        schema.Default = value;
                        continue;
                    case "type":
                        schema.Type = (value as OpenApiString)?.Value;
                        continue;
                    case "items":
                        schema.Items = (value as OpenApiObject)?.ToSchema();
                        continue;
                    case "enum":
                        schema.Enum = value as OpenApiArray;
                        continue;
                    case "format":
                        schema.Format = (value as OpenApiString)?.Value;
                        continue;
                    case "allOf":
                        schema.AllOf = (value as OpenApiArray)?.ToAllOf();
                        continue;
                    case "properties":
                        schema.Properties = (value as OpenApiObject)?.ToProperties();
                        continue;
                    case "required":
                        schema.Required = (value as OpenApiArray)?.ToRequiredArray();
                        continue;
                }
            }
            return schema;
        }

        private static ISet<string> ToRequiredArray(this OpenApiArray array)
        {
            return (ISet<string>) array.Select(requiredProperty => (requiredProperty as OpenApiString)?.Value);
        }

        private static OpenApiParameter ToParameter(this OpenApiObject parametersAsObject)
        {
            var parameter = new OpenApiParameter();

            foreach (var (parameterAttribute, parameterValue) in parametersAsObject)
            {
                switch (parameterAttribute)
                {
                    case "$ref":
                        parameter.UnresolvedReference = true;
                        parameter.Reference = (parameterValue as OpenApiString)?.Value.ToReference(ReferenceType.Parameter);
                        return parameter;
                    case "name":
                        parameter.Name = (parameterValue as OpenApiString)?.Value;
                        continue;
                    case "in":
                        parameter.In = (parameterValue as OpenApiString)?.Value.ToParameterLocation();
                        continue;
                    case "required":
                        parameter.Required = (parameterValue as OpenApiBoolean)?.Value ?? false;
                        continue;
                    case "schema":
                        parameter.Schema = (parameterValue as OpenApiObject)?.ToSchema();
                        continue;
                }
            }
            return parameter;
        }

        private static IList<OpenApiParameter> ToParameters(this OpenApiArray parametersAsObject)
        {
            var parameters = new List<OpenApiParameter>();

            foreach (var parameterAsObject in parametersAsObject)
            {
                parameters.Add((parameterAsObject as OpenApiObject)?.ToParameter());
            }
            return parameters;
        }

        private static OpenApiRequestBody ToRequestBody(this OpenApiObject responsesAsObject)
        {
            var requestBody = new OpenApiRequestBody();

            foreach (var (key, value) in responsesAsObject)
            {
                switch (key)
                {
                    case "content":
                        requestBody.Content = (value as OpenApiObject)?.ToContent();
                        continue;
                    case "required":
                        requestBody.Required = (value as OpenApiBoolean)?.Value ?? false;
                        continue;
                    case "$ref":
                        requestBody.UnresolvedReference = true;
                        requestBody.Reference = (value as OpenApiString)?.Value.ToReference(ReferenceType.RequestBody);
                        return requestBody;
                }
            }
            return requestBody;
        }

        private static OpenApiMediaType ToMediaTypeObject(this OpenApiObject mediaTypeAsObject)
        {
            var response = new OpenApiMediaType();

            foreach (var (mediaTypeAttribute, value) in mediaTypeAsObject)
            {
                switch (mediaTypeAttribute)
                {
                    case "schema":
                        response.Schema = (value as OpenApiObject)?.ToSchema();
                        continue;
                }
            }

            return response;
        }

        private static IDictionary<string, OpenApiMediaType> ToContent(this OpenApiObject mediaTypeAsObject)
        {
            var response = new Dictionary<string, OpenApiMediaType>();

            foreach (var (mediaType, mediaTypeObject) in mediaTypeAsObject)
            {
                response.Add(mediaType, (mediaTypeObject as OpenApiObject)?.ToMediaTypeObject());
            }

            return response;
        }

        private static OpenApiResponse ToResponse(this OpenApiObject responseAsObject)
        {
            var response = new OpenApiResponse();

            foreach (var (responseAttribute, value) in responseAsObject)
            {
                switch (responseAttribute)
                {
                    case "$ref":
                        response.UnresolvedReference = true;
                        response.Reference = (value as OpenApiString)?.Value.ToReference(ReferenceType.Response);
                        return response;
                    case "content":
                        response.Content = (value as OpenApiObject)?.ToContent();
                        continue;
                }
            }

            return response;
        }

        private static OpenApiResponses ToResponses(this OpenApiObject responsesAsObject)
        {
            var responses = new OpenApiResponses();

            foreach (var (statusCode, responseAsObject) in responsesAsObject)
            {
                responses.Add(statusCode, (responseAsObject as OpenApiObject)?.ToResponse());
            }
            return responses;
        }

        private static OpenApiOperation ToOperation(this OpenApiObject operationAsObject)
        {
            var operation = new OpenApiOperation();

            foreach (var (key, value) in operationAsObject)
            {
                switch (key)
                {
                    case "operationId":
                        operation.OperationId = (value as OpenApiString)?.Value;
                        continue;
                    case "parameters":
                        operation.Parameters = (value as OpenApiArray)?.ToParameters();
                        continue;
                    case "requestBody":
                        operation.RequestBody = (value as OpenApiObject)?.ToRequestBody();
                        continue;
                    case "responses":
                        operation.Responses = (value as OpenApiObject)?.ToResponses();
                        continue;
                }
            }
            return operation;
        }

        private static OpenApiPathItem ToOpenApiPathItem(this OpenApiObject rawPathItem)
        {
            var pathItem = new OpenApiPathItem();

            foreach (var (pathItemKey, operation) in rawPathItem)
            {
                if (_operationTypesMap.ContainsKey(pathItemKey))
                {
                    pathItem.Operations.Add(_operationTypesMap[pathItemKey], (operation as OpenApiObject)?.ToOperation());
                }
                else
                {
                    switch (pathItemKey)
                    {
                        case "parameters":
                            pathItem.Parameters = (operation as OpenApiArray)?.ToParameters();
                            continue;
                    }
                }
            }

            return pathItem;
        }
    }
}
