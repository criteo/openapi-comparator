using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Exceptions;
using Microsoft.OpenApi.Models;
using ParameterLocation = Microsoft.OpenApi.Models.ParameterLocation;

namespace Criteo.OpenApi.Comparator.Comparators.Extensions
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

            foreach (var path in rawPaths)
            {
                paths.Add(path.Key, (path.Value as OpenApiObject)?.ToOpenApiPathItem());
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

            foreach (var property in propertiesAsObject)
            {
                properties.Add(property.Key, (property.Value as OpenApiObject)?.ToSchema());
            }
            return properties;
        }

        private static OpenApiSchema ToSchema(this OpenApiObject schemaAsObject)
        {
            var schema = new OpenApiSchema();
            foreach (var schemaObject in schemaAsObject)
            {
                switch (schemaObject.Key)
                {
                    case "$ref":
                        schema.UnresolvedReference = true;
                        schema.Reference = (schemaObject.Value as OpenApiString)?.Value.ToReference(ReferenceType.Schema);
                        return schema;
                    case "readOnly":
                        schema.ReadOnly = (schemaObject.Value as OpenApiBoolean)?.Value ?? false;
                        continue;
                    case "discriminator":
                        schema.Discriminator = (schemaObject.Value as OpenApiObject)?.ToDiscriminator();
                        continue;
                    case "default":
                        schema.Default = schemaObject.Value;
                        continue;
                    case "type":
                        schema.Type = (schemaObject.Value as OpenApiString)?.Value;
                        continue;
                    case "items":
                        schema.Items = (schemaObject.Value as OpenApiObject)?.ToSchema();
                        continue;
                    case "enum":
                        schema.Enum = schemaObject.Value as OpenApiArray;
                        continue;
                    case "format":
                        schema.Format = (schemaObject.Value as OpenApiString)?.Value;
                        continue;
                    case "allOf":
                        schema.AllOf = (schemaObject.Value as OpenApiArray)?.ToAllOf();
                        continue;
                    case "properties":
                        schema.Properties = (schemaObject.Value as OpenApiObject)?.ToProperties();
                        continue;
                    case "required":
                        schema.Required = (schemaObject.Value as OpenApiArray)?.ToRequiredArray();
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

            foreach (var parameterAsObject in parametersAsObject)
            {
                switch (parameterAsObject.Key)
                {
                    case "$ref":
                        parameter.UnresolvedReference = true;
                        parameter.Reference = (parameterAsObject.Value as OpenApiString)?.Value.ToReference(ReferenceType.Parameter);
                        return parameter;
                    case "name":
                        parameter.Name = (parameterAsObject.Value as OpenApiString)?.Value;
                        continue;
                    case "in":
                        parameter.In = (parameterAsObject.Value as OpenApiString)?.Value.ToParameterLocation();
                        continue;
                    case "required":
                        parameter.Required = (parameterAsObject.Value as OpenApiBoolean)?.Value ?? false;
                        continue;
                    case "schema":
                        parameter.Schema = (parameterAsObject.Value as OpenApiObject)?.ToSchema();
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

            foreach (var responseAsObject in responsesAsObject)
            {
                switch (responseAsObject.Key)
                {
                    case "content":
                        requestBody.Content = (responseAsObject.Value as OpenApiObject)?.ToContent();
                        continue;
                    case "required":
                        requestBody.Required = (responseAsObject.Value as OpenApiBoolean)?.Value ?? false;
                        continue;
                    case "$ref":
                        requestBody.UnresolvedReference = true;
                        requestBody.Reference = (responseAsObject.Value as OpenApiString)?.Value.ToReference(ReferenceType.RequestBody);
                        return requestBody;
                }
            }
            return requestBody;
        }

        private static OpenApiMediaType ToMediaTypeObject(this OpenApiObject mediaTypeAsObject)
        {
            var response = new OpenApiMediaType();

            foreach (var mediaType in mediaTypeAsObject)
            {
                switch (mediaType.Key)
                {
                    case "schema":
                        response.Schema = (mediaType.Value as OpenApiObject)?.ToSchema();
                        continue;
                }
            }

            return response;
        }

        private static IDictionary<string, OpenApiMediaType> ToContent(this OpenApiObject mediaTypeAsObject)
        {
            var response = new Dictionary<string, OpenApiMediaType>();

            foreach (var mediaType in mediaTypeAsObject)
            {
                response.Add(mediaType.Key, (mediaType.Value as OpenApiObject)?.ToMediaTypeObject());
            }

            return response;
        }

        private static OpenApiResponse ToResponse(this OpenApiObject responseAsObject)
        {
            var response = new OpenApiResponse();

            foreach (var responseAttribute in responseAsObject)
            {
                switch (responseAttribute.Key)
                {
                    case "$ref":
                        response.UnresolvedReference = true;
                        response.Reference = (responseAttribute.Value as OpenApiString)?.Value.ToReference(ReferenceType.Response);
                        return response;
                    case "content":
                        response.Content = (responseAttribute.Value as OpenApiObject)?.ToContent();
                        continue;
                }
            }

            return response;
        }

        private static OpenApiResponses ToResponses(this OpenApiObject responsesAsObject)
        {
            var responses = new OpenApiResponses();

            foreach (var responseAsObject in responsesAsObject)
            {
                responses.Add(responseAsObject.Key, (responseAsObject.Value as OpenApiObject)?.ToResponse());
            }
            return responses;
        }

        private static OpenApiOperation ToOperation(this OpenApiObject operationAsObject)
        {
            var operation = new OpenApiOperation();

            foreach (var operationProperty in operationAsObject)
            {
                switch (operationProperty.Key)
                {
                    case "operationId":
                        operation.OperationId = (operationProperty.Value as OpenApiString)?.Value;
                        continue;
                    case "parameters":
                        operation.Parameters = (operationProperty.Value as OpenApiArray)?.ToParameters();
                        continue;
                    case "requestBody":
                        operation.RequestBody = (operationProperty.Value as OpenApiObject)?.ToRequestBody();
                        continue;
                    case "responses":
                        operation.Responses = (operationProperty.Value as OpenApiObject)?.ToResponses();
                        continue;
                }
            }
            return operation;
        }

        private static OpenApiPathItem ToOpenApiPathItem(this OpenApiObject rawPathItem)
        {
            var pathItem = new OpenApiPathItem();

            foreach (var operation in rawPathItem)
            {
                if (_operationTypesMap.ContainsKey(operation.Key))
                {
                    pathItem.Operations.Add(_operationTypesMap[operation.Key], (operation.Value as OpenApiObject)?.ToOperation());
                }
                else
                {
                    switch (operation.Key)
                    {
                        case "parameters":
                            pathItem.Parameters = (operation.Value as OpenApiArray)?.ToParameters();
                            continue;
                    }
                }
            }

            return pathItem;
        }
    }
}
