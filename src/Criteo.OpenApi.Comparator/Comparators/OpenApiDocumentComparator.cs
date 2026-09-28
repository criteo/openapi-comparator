// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using Criteo.OpenApi.Comparator.Comparators.Extensions;
using Criteo.OpenApi.Comparator.Logging;
#if NET10_0
using Microsoft.OpenApi;
using OpenApiObject = System.Text.Json.Nodes.JsonObject;
using OpenApiParameter = Microsoft.OpenApi.IOpenApiParameter;
using OpenApiReference = Microsoft.OpenApi.BaseOpenApiReference;
using OpenApiRequestBody = Microsoft.OpenApi.IOpenApiRequestBody;
using OpenApiResponse = Microsoft.OpenApi.IOpenApiResponse;
using OpenApiSchema = Microsoft.OpenApi.IOpenApiSchema;
using OperationType = System.Net.Http.HttpMethod;
#else
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Exceptions;
using Microsoft.OpenApi.Models;
#endif

namespace Criteo.OpenApi.Comparator.Comparators
{
    internal class OpenApiDocumentComparator
    {
        private readonly OperationComparator _operationComparator;
        private readonly SchemaComparator _schemaComparator;
        private readonly ParameterComparator _parameterComparator;
        private readonly ResponseComparator _responseComparator;

        private readonly IDictionary<OpenApiSchema, bool> _isSchemaReferenced = new Dictionary<OpenApiSchema, bool>();

        internal OpenApiDocumentComparator()
        {
            _schemaComparator = new SchemaComparator();
            var contentComparator = new ContentComparator(_schemaComparator);
            _parameterComparator = new ParameterComparator(_schemaComparator, contentComparator);
            var requestBodyComparator = new RequestBodyComparator(contentComparator);
            _responseComparator = new ResponseComparator(contentComparator);
            _operationComparator = new OperationComparator(_parameterComparator, requestBodyComparator, _responseComparator);
        }

        /// <summary>
        /// Compare a modified document node (this) to a previous one and look for breaking as well as non-breaking changes.
        /// </summary>
        /// <param name="context">The modified document context.</param>
        /// <param name="oldDocument">The original document model.</param>
        /// <param name="newDocument">The new document model.</param>
        /// <returns>A list of messages from the comparison.</returns>
        internal IEnumerable<ComparisonMessage> Compare(
            ComparisonContext context,
            OpenApiDocument oldDocument,
            OpenApiDocument newDocument
        )
        {
            if (context == null)
                throw new ArgumentException(nameof(context));

            if (oldDocument == null)
                throw new ArgumentNullException(nameof(oldDocument));

            if (newDocument == null)
                throw new ArgumentNullException(nameof(newDocument));

            if (context.NewOpenApiDocument != newDocument)
                throw new ArgumentException("context.NewOpenApiDocument != newDocument");

            if (context.OldOpenApiDocument != oldDocument)
                throw new ArgumentException("context.OldOpenApiDocument != oldDocument");

            CompareVersions(context, oldDocument.Info?.Version, newDocument.Info?.Version);

            CompareServers(context, oldDocument.Servers, newDocument.Servers);

            ComparePaths(context, oldDocument.Paths, newDocument.Paths);

            CompareCustomPaths(context, oldDocument, newDocument);

            CompareComponents(context, oldDocument, newDocument);

            context.Pop();

            return context.Messages;
        }

        /// <summary>
        /// Since some services may rely on semantic versioning, comparing versions is fairly complex.
        /// </summary>
        /// <param name="context">A comparison context.</param>
        /// <param name="newVersion">The new version string.</param>
        /// <param name="oldVersion">The old version string</param>
        /// <remarks>
        /// In semantic versioning schemes, only the major and minor version numbers are considered when comparing versions.
        /// Build numbers are ignored.
        /// </remarks>
        private static void CompareVersions(ComparisonContext context, string oldVersion, string newVersion)
        {
            if (newVersion == null || oldVersion == null)
                return;

            context.PushProperty("info");
            context.PushProperty("version");

            var newVersionAsList = newVersion.Split('.');
            var oldVersionAsList = oldVersion.Split('.');

            // If the version consists only of numbers separated by '.', we'll consider it semantic versioning.
            if (!context.Strict && oldVersionAsList.Length > 0 && newVersionAsList.Length > 0)
            {
                var isVersionChanged = false;

                // Situation 1: The versioning scheme is semantic, i.e. it uses a major.minor.build-number scheme, where each component is an integer.
                //              In this case, we care about the major/minor numbers, but not the build number. In other words, if all that is different
                //              is the build number, it will not be treated as a version change.

                var newMajor = 0;
                var areIntegers = int.TryParse(oldVersionAsList[0], out var oldMajor) && int.TryParse(newVersionAsList[0], out newMajor);

                if (areIntegers && oldMajor != newMajor)
                {
                    isVersionChanged = true;

                    if (oldMajor > newMajor)
                    {
                        context.LogError(ComparisonRules.VersionsReversed, oldVersion, newVersion);
                    }
                }

                if (!isVersionChanged && areIntegers && oldVersionAsList.Length > 1 && newVersionAsList.Length > 1)
                {
                    var newMinor = 0;
                    areIntegers = int.TryParse(oldVersionAsList[1], out var oldMinor) && int.TryParse(newVersionAsList[1], out newMinor);

                    if (areIntegers && oldMinor != newMinor)
                    {
                        isVersionChanged = true;

                        if (oldMinor > newMinor)
                        {
                            context.LogError(ComparisonRules.VersionsReversed, oldVersion, newVersion);
                        }
                    }
                }

                // Situation 2: The versioning scheme is something else, maybe a date or just a label?
                //              Regardless of what it is, we just check whether the two strings are equal or not.

                if (!isVersionChanged && !areIntegers)
                {
                    isVersionChanged = !oldVersion.ToLower().Equals(newVersion.ToLower());
                }

                context.Strict = !isVersionChanged;
            }

            if (context.Strict)
            {
                // There was no version change between the documents. This is not an error, but noteworthy.
                context.LogInfo(ComparisonRules.NoVersionChange);
            }

            context.Pop();
            context.Pop();
        }

        private static void CompareServers(ComparisonContext context,
            IList<OpenApiServer> oldServers,
            IList<OpenApiServer> newServers)
        {
            if (oldServers == null && newServers == null)
                return;

            context.PushProperty("servers");
            var oldServerUrls = oldServers == null ? new List<string>() : oldServers.Select(server => server.Url);
            var newServerUrls = newServers == null ? new List<string>() : newServers.Select(server => server.Url);

            foreach (var oldServerUrl in oldServerUrls.Except(newServerUrls))
            {
                context.PushServerByUrl(oldServerUrl);
                context.LogBreakingChange(ComparisonRules.ServerNoLongerSupported, oldServerUrl);
                context.Pop();
            }
            context.Pop();
        }

        /// <summary>
        /// Check that no paths were removed, and compare the paths that are still there.
        /// Check whether any new paths are being added
        /// </summary>
        private void ComparePaths(ComparisonContext context,
            OpenApiPaths oldPaths, OpenApiPaths newPaths,
            bool isFromExtension = false)
        {
            if (oldPaths == null && newPaths == null)
                return;

            var oldPathsWithVariables = oldPaths?.Keys;

            oldPaths = RemovePathVariables(oldPaths ?? new OpenApiPaths());
            newPaths = RemovePathVariables(newPaths ?? new OpenApiPaths());

            var commonPaths = newPaths.Keys.Where(oldPaths.Keys.Contains).ToList();

            context.PushProperty(isFromExtension ? "x-ms-paths" : "paths");
            foreach (var removedPath in oldPaths.Keys.Except(commonPaths))
            {
                context.PushPathProperty(removedPath, isFromExtension);
                var removedPathWithVariables =
                    oldPathsWithVariables.First(oldPath => ObjectPath.OpenApiPathName(oldPath) == removedPath);
                context.LogBreakingChange(ComparisonRules.RemovedPath, removedPathWithVariables);
                context.Pop();
            }

            foreach (var addedPath in newPaths.Keys.Except(commonPaths))
            {
                context.PushPathProperty(addedPath, isFromExtension);
                context.LogInfo(ComparisonRules.AddedPath);
                context.Pop();
            }

            foreach (var commonPath in commonPaths)
            {
                var oldPathItem = oldPaths[commonPath];
                var newPathItem = newPaths[commonPath];

                context.PushPathProperty(commonPath, isFromExtension);
                _operationComparator.CompareParameters(context, oldPathItem.Parameters, newPathItem.Parameters);
                CompareOperations(context, oldPathItem.Operations, newPathItem.Operations);
                context.Pop();
            }
            context.Pop();
        }

        private void CompareOperations(ComparisonContext context,
            IDictionary<OperationType,OpenApiOperation> oldOperations,
            IDictionary<OperationType,OpenApiOperation> newOperations)
        {
            var commonOperations = newOperations.Keys.Where(oldOperations.Keys.Contains).ToList();

            foreach (var removedOperationName in oldOperations.Keys.Except(commonOperations))
            {
                context.PushProperty(removedOperationName.ToString().ToLower());
                context.LogBreakingChange(ComparisonRules.RemovedOperation, oldOperations[removedOperationName].OperationId);
                context.Pop();
            }

            foreach (var addedOperationName in newOperations.Keys.Except(commonOperations))
            {
                context.PushProperty(addedOperationName.ToString().ToLower());
                context.LogInfo(ComparisonRules.AddedOperation);
                context.Pop();
            }

            foreach (var operationName in commonOperations)
            {
                context.PushProperty(operationName.ToString().ToLower());
                _operationComparator.Compare(context, oldOperations[operationName], newOperations[operationName]);
                context.Pop();
            }
        }

        /// <summary>
        /// Since renaming a path parameter doesn't logically alter the path, we must remove the parameter names
        /// before comparing paths using string comparison.
        /// </summary>
        /// <param name="paths">A dictionary of paths, potentially with embedded parameter names.</param>
        /// <returns>A transformed dictionary, where paths do not embed parameter names.</returns>
        private static OpenApiPaths RemovePathVariables(OpenApiPaths paths)
        {
            var pathsResult = new OpenApiPaths();
            foreach (var path in paths)
            {
                pathsResult[ObjectPath.OpenApiPathName(path.Key)] = path.Value;
            }
            return pathsResult;
        }

        private void CompareCustomPaths(ComparisonContext context, OpenApiDocument oldDocument,
            OpenApiDocument newDocument)
        {
            const string customPathsName = "x-ms-paths";
            var oldCustomPathsAsObject = oldDocument.Extensions != null
                && oldDocument.Extensions.TryGetValue(customPathsName, out var oldExtension)
                ? oldExtension
                : null;
            var newCustomPathsAsObject = newDocument.Extensions != null
                && newDocument.Extensions.TryGetValue(customPathsName, out var newExtension)
                ? newExtension
                : null;

            if (oldCustomPathsAsObject == null || newCustomPathsAsObject == null)
                return;

            try
            {
#if NET10_0
                var oldCustomPaths = oldCustomPathsAsObject.ToJsonNode() as OpenApiObject;
                var newCustomPaths = newCustomPathsAsObject.ToJsonNode() as OpenApiObject;
                if (oldCustomPaths == null || newCustomPaths == null)
                    throw new InvalidCastException();

                ComparePaths(context, oldCustomPaths.ToOpenApiPaths(), newCustomPaths.ToOpenApiPaths(), true);
#else
                ComparePaths(context, ((OpenApiObject) oldCustomPathsAsObject).ToOpenApiPaths(),
                    ((OpenApiObject) newCustomPathsAsObject).ToOpenApiPaths(), true);
#endif
            }
            catch (InvalidCastException)
            {
                throw new OpenApiException($"Invalid format for {customPathsName} extension. It should have an OpenApi path format.");
            }
        }

        private void CompareComponents(ComparisonContext context,
            OpenApiDocument oldDocument,
            OpenApiDocument newDocument)
        {
            if (oldDocument.Components == null && newDocument.Components == null)
                return;

            oldDocument.Components = oldDocument.Components ?? new OpenApiComponents();
            newDocument.Components = newDocument.Components ?? new OpenApiComponents();

            TrackSchemasReference(oldDocument);
            TrackSchemasReference(newDocument);

            context.PushProperty("components");

            CompareSchemas(context, oldDocument.Components.Schemas, newDocument.Components.Schemas);

#if NET10_0
            var oldReferencedParameters = GetReferencedParameterNames(oldDocument);
            var newReferencedParameters = GetReferencedParameterNames(newDocument);
            var oldParameters = oldDocument.Components.Parameters?
                .Where(parameter => oldReferencedParameters.Contains(parameter.Key)
                    && !newDocument.Components.Parameters.ContainsKey(parameter.Key))
                .ToDictionary(parameter => parameter.Key, parameter => parameter.Value);
            var newParameters = new Dictionary<string, OpenApiParameter>();
#else
            var oldParameters = oldDocument.Components.Parameters;
            var newParameters = newDocument.Components.Parameters;
#endif
            CompareParameters(context, oldParameters, newParameters);

            CompareResponses(context, oldDocument.Components.Responses, newDocument.Components.Responses);
        }

#if NET10_0
        private static HashSet<string> GetReferencedParameterNames(OpenApiDocument document)
        {
            var referencedParameters = new HashSet<string>();
            if (document.Components.Parameters == null)
                return referencedParameters;

            foreach (var path in document.Paths.Values)
            {
                if (path.Operations == null)
                    continue;

                foreach (var operation in path.Operations.Values)
                {
                    if (operation.Parameters == null)
                        continue;

                    foreach (var parameter in operation.Parameters)
                    {
                        var reference = parameter.GetReference();
                        if (reference?.ReferenceV3 == null)
                        {
                            var parameterName = parameter.Name;
                            if (document.Components.Parameters.ContainsKey(parameterName))
                                referencedParameters.Add(parameterName);
                            continue;
                        }

                        var componentName = reference.ReferenceV3.Split('/').Last();
                        if (document.Components.Parameters.ContainsKey(componentName))
                            referencedParameters.Add(componentName);
                    }
                }
            }

            return referencedParameters;
        }
#endif

        /// <summary>
        /// In order to avoid comparing schemas that are not used, we go through all references that are
        /// found in operations, global parameters, and global responses.
        /// Schemas that are referenced from other schemas are included only by transitive closure.
        /// </summary>
        private void TrackSchemasReference(OpenApiDocument document)
         {
             if (document.Components.Schemas == null)
                 return;

             InitSchemasReference(document);

             TrackSchemasReferenceInPaths(document);

             TrackSchemasReferenceInComponents(document);
         }

         private void InitSchemasReference(OpenApiDocument document)
         {
             foreach (var schema in document.Components.Schemas.Values)
             {
                 if (!_isSchemaReferenced.ContainsKey(schema))
                 {
                     _isSchemaReferenced[schema] = false;
                 }
             }
         }

         private void TrackSchemasReferenceInPaths(OpenApiDocument document)
         {
             foreach (var pathItem in document.Paths.Values)
             {
                 if (pathItem.Operations == null)
                     continue;

                 foreach (var operation in pathItem.Operations.Values)
                 {
                     TrackSchemasReferenceInParameters(operation.Parameters, document.Components.Schemas);

                     TrackSchemasReferenceInRequestBody(operation.RequestBody, document.Components.Schemas);

                     if (operation.Responses != null)
                         TrackSchemasReferenceInResponses(operation.Responses.Values, document.Components.Schemas);
                 }
             }
         }

         private void TrackSchemasReferenceInComponents(OpenApiDocument document)
         {
             if (document.Components.Parameters != null)
                 TrackSchemasReferenceInParameters(document.Components.Parameters.Values, document.Components.Schemas);

             foreach (var requestBody in document.Components.RequestBodies?.Values ?? Enumerable.Empty<OpenApiRequestBody>())
             {
                 TrackSchemasReferenceInRequestBody(requestBody, document.Components.Schemas);
             }

             if (document.Components.Responses != null)
                 TrackSchemasReferenceInResponses(document.Components.Responses.Values, document.Components.Schemas);

             DetectPolymorphicSchemas(document.Components.Schemas);
         }

         private void TrackSchemasReferenceInParameters(IEnumerable<OpenApiParameter> parameters,
             IDictionary<string, OpenApiSchema> commonSchemas)
         {
             if (parameters == null)
                 return;

             foreach (var parameter in parameters)
             {
                 var reference = parameter.Schema?.GetReference();
                 if (!string.IsNullOrWhiteSpace(reference?.ReferenceV3))
                 {
                     var schema = reference.Resolve(commonSchemas);
                     _isSchemaReferenced[schema ?? parameter.Schema] = true;
                 }
             }
         }

         private void TrackSchemasReferenceInRequestBody(OpenApiRequestBody requestBody,
             IDictionary<string, OpenApiSchema> commonSchemas)
         {
             if (requestBody?.Content == null)
                 return;

             foreach (var requestBodyType in requestBody.Content.Values)
             {
                 var reference = requestBodyType.Schema?.GetReference();
                 if (!string.IsNullOrWhiteSpace(reference?.ReferenceV3))
                 {
                     var schema = reference.Resolve(commonSchemas);
                     _isSchemaReferenced[schema ?? requestBodyType.Schema] = true;
                 }
             }
         }

         private void TrackSchemasReferenceInResponses(ICollection<OpenApiResponse> responses,
             IDictionary<string, OpenApiSchema> commonSchemas)
         {
             if (responses == null)
                 return;

             foreach (var responseStatus in responses)
             {
                 if (responseStatus.Content == null)
                     continue;

                 foreach (var responseMediaType in responseStatus.Content.Values)
                 {
                     var reference = responseMediaType?.Schema?.GetReference();
                     if (!string.IsNullOrWhiteSpace(reference?.ReferenceV3))
                     {
                         var schema = reference.Resolve(commonSchemas);
                         _isSchemaReferenced[schema ?? responseMediaType.Schema] = true;
                     }
                 }
             }
         }

         /// <summary>
         /// If a schema has an allOf property which has a discriminator,
         /// then the schema is considered has referenced after all.
         /// </summary>
         private void DetectPolymorphicSchemas(IDictionary<string, OpenApiSchema> schemas)
         {
             var unReferencedSchemas = schemas.Values
                 .Where(schema => !_isSchemaReferenced[schema]);
             foreach (var schema in unReferencedSchemas)
             {
                 if (schema.Extensions != null && schema.Extensions.ContainsKey("x-ms-discriminator-value"))
                 {
                     _isSchemaReferenced[schema] = true;
                     continue;
                 }

                 if (schema.AllOf == null)
                     continue;

                 var referencedAllOffProperties = schema.AllOf.Where(property =>
                     !string.IsNullOrWhiteSpace(property.GetReference()?.ReferenceV3));
                 foreach (var property in referencedAllOffProperties)
                 {
                     var reference = property.GetReference();
                     _isSchemaReferenced[schema] = FindDiscriminator(reference, schemas);

                     if (_isSchemaReferenced[schema])
                         break;
                 }
             }
         }

         /// <summary>
         /// Detect if a schema's hierarchy contains a discriminator.
         /// </summary>
         /// <param name="reference">A document-relative reference object</param>
         /// <param name="schemas">The schemas dictionary to use</param>
         /// <returns></returns>
         private static bool FindDiscriminator(OpenApiReference reference, IDictionary<string, OpenApiSchema> schemas)
         {
             var schema = reference.Resolve(schemas);
             if (schema?.Discriminator != null)
                 return true;

             if (schema?.AllOf == null)
                 return false;

             return schema.AllOf.Any(subSchema =>
                 subSchema.GetReference() != null && FindDiscriminator(subSchema.GetReference(), schemas));
         }

         private void CompareSchemas(ComparisonContext context,
             IDictionary<string, OpenApiSchema> oldSchemas,
             IDictionary<string, OpenApiSchema> newSchemas)
         {
             oldSchemas ??= new Dictionary<string, OpenApiSchema>();
             newSchemas ??= new Dictionary<string, OpenApiSchema>();
             context.PushProperty("schemas");
             foreach (var oldSchema in oldSchemas)
             {
                 context.PushProperty(oldSchema.Key);
                 if (!newSchemas.TryGetValue(oldSchema.Key, out var newSchema))
                 {
                 if (!_isSchemaReferenced[oldSchema.Value])
                     {
                         // It's only an error if the schema is referenced in the old service.
                         context.LogBreakingChange(ComparisonRules.RemovedDefinition, oldSchema.Key);
                     }
                 }
                 else
                 {
                     if (!_schemaComparator.WasVisited(oldSchema.Value))
                         _schemaComparator.Compare(context, oldSchema.Value, newSchema, _isSchemaReferenced[oldSchema.Value]);
                 }
                 context.Pop();
             }
             context.Pop();
         }

         private void CompareParameters(ComparisonContext context,
             IDictionary<string, OpenApiParameter> oldParameters,
             IDictionary<string, OpenApiParameter> newParameters)
         {
             oldParameters ??= new Dictionary<string, OpenApiParameter>();
             newParameters ??= new Dictionary<string, OpenApiParameter>();
             context.PushProperty("parameters");
             foreach (var oldParameterName in oldParameters.Keys)
             {
                 context.PushProperty(oldParameterName);
                 if (!newParameters.TryGetValue(oldParameterName, out var newParameter))
                 {
                     context.LogBreakingChange(ComparisonRules.RemovedClientParameter, oldParameterName);
                 }
                 else
                 {
                     _parameterComparator.Compare(context, oldParameters[oldParameterName], newParameter);
                 }
                 context.Pop();
             }
             context.Pop();
         }

         private void CompareResponses(ComparisonContext context,
             IDictionary<string, OpenApiResponse> oldResponses,
             IDictionary<string, OpenApiResponse> newResponses)
         {
             oldResponses ??= new Dictionary<string, OpenApiResponse>();
             newResponses ??= new Dictionary<string, OpenApiResponse>();
             context.PushProperty("responses");
             foreach (var oldDefinition in oldResponses.Keys)
             {
                 if (!newResponses.TryGetValue(oldDefinition, out var response))
                 {
                     context.LogBreakingChange(ComparisonRules.RemovedDefinition, oldDefinition);
                 }
                 else
                 {
                     context.PushProperty(oldDefinition);
                     _responseComparator.Compare(context, oldResponses[oldDefinition], response);
                     context.Pop();
                 }
             }
             context.Pop();
         }
    }
}
