// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Criteo.OpenApi.Comparator.Comparators.Extensions;
using Microsoft.OpenApi;

namespace Criteo.OpenApi.Comparator.Comparators
{
    internal class SchemaComparator
    {
        private readonly LinkedList<IOpenApiSchema> _visitedSchemas;

        private readonly IDictionary<IOpenApiSchema, DataDirection> _compareDirections;

        internal SchemaComparator()
        {
            _visitedSchemas = new LinkedList<IOpenApiSchema>();
            _compareDirections = new Dictionary<IOpenApiSchema, DataDirection>();
        }

        internal void Compare(ComparisonContext context,
            IOpenApiSchema oldSchema,
            IOpenApiSchema newSchema,
            bool isSchemaReferenced = true)
        {
            if (oldSchema == null && newSchema == null)
                return;

            if (oldSchema == null)
            {
                context.LogError(ComparisonRules.AddedSchema);
                return;
            }

            if (newSchema == null)
            {
                context.LogBreakingChange(ComparisonRules.RemovedDefinition, default(string));
                return;
            }

            if (newSchema.GetReference()?.ReferenceV3 != null
                && !newSchema.GetReference().ReferenceV3.Equals(oldSchema.GetReference()?.ReferenceV3))
            {
                context.LogBreakingChange(ComparisonRules.ReferenceRedirection);
            }

            var areSchemasReferenced = false;
            if (!string.IsNullOrWhiteSpace(newSchema.GetReference()?.ReferenceV3))
            {
                newSchema = newSchema.GetReference().Resolve(context.NewOpenApiDocument.Components.Schemas);
                areSchemasReferenced = true;
                if (newSchema == null)
                    return;
            }

            if (!string.IsNullOrWhiteSpace(oldSchema.GetReference()?.ReferenceV3))
            {
                oldSchema = oldSchema.GetReference().Resolve(context.OldOpenApiDocument.Components.Schemas);
                areSchemasReferenced = true;
                if (oldSchema == null)
                    return;
            }

            // Component definitions no longer carry their own reference in OpenAPI.NET v2.
            // Avoid comparing them again after visiting them through an operation.
            if (context.Direction == DataDirection.None && _visitedSchemas.Contains(oldSchema))
                return;

            if (context.Direction != DataDirection.None)
            {
                _compareDirections.TryGetValue(newSchema, out var savedDirection);

                // If this direction has already been checked, skip it
                if (context.Direction == savedDirection || savedDirection == DataDirection.Both)
                    return;

                context.Direction |= savedDirection;
                _compareDirections[newSchema] = context.Direction;
            }

            if (areSchemasReferenced)
            {
                if (_visitedSchemas.Contains(oldSchema) && context.Direction != DataDirection.Both)
                    return;

                _visitedSchemas.AddFirst(oldSchema);
            }

            CompareReadOnly(context, oldSchema.ReadOnly, newSchema.ReadOnly);

            CompareDiscriminator(context, oldSchema.Discriminator, newSchema.Discriminator);

            CompareDefault(context, oldSchema.Default, newSchema.Default);

            CompareConstraints(context, oldSchema, newSchema);

            CompareType(context, oldSchema.Type & ~JsonSchemaType.Null, newSchema.Type & ~JsonSchemaType.Null);

            CompareItems(context, oldSchema.Items, newSchema.Items);

            var enumExtension = oldSchema.Extensions?.TryGetValue("x-ms-enum", out var extension) == true
                ? (extension as JsonNodeExtension)?.Node as JsonObject : null;
            CompareEnum(context, oldSchema.Enum, newSchema.Enum, enumExtension);

            CompareFormat(context, oldSchema, newSchema);

            CompareAllOf(context, oldSchema.AllOf, newSchema.AllOf);

            CompareOneOf(context, oldSchema.OneOf, newSchema.OneOf);

            CompareProperties(context, oldSchema, newSchema, isSchemaReferenced);

            CompareRequired(context, oldSchema.Required, newSchema.Required);

            CompareNullable(context, oldSchema.Type?.HasFlag(JsonSchemaType.Null) == true, newSchema.Type?.HasFlag(JsonSchemaType.Null) == true);
        }

        private static void CompareNullable(ComparisonContext context,
            bool oldNullable,
            bool newNullable)
        {
            if (oldNullable == newNullable)
                return;
            context.PushProperty("nullable");
            context.LogBreakingChange(
                ComparisonRules.NullablePropertyChanged,
                oldNullable.ToString().ToLower(),
                newNullable.ToString().ToLower()
            );
            context.Pop();
        }

        private static void CompareReadOnly(ComparisonContext context,
            bool oldReadOnly,
            bool newReadOnly)
        {
            if (oldReadOnly != newReadOnly)
            {
                context.PushProperty("readOnly");
                context.LogBreakingChange(
                    ComparisonRules.ReadonlyPropertyChanged,
                    oldReadOnly.ToString().ToLower(),
                    newReadOnly.ToString().ToLower()
                );
                context.Pop();
            }
        }

        private static void CompareDiscriminator(ComparisonContext context,
            OpenApiDiscriminator oldDiscriminator, OpenApiDiscriminator newDiscriminator)
        {
            if (oldDiscriminator == null && newDiscriminator != null
                || oldDiscriminator?.PropertyName != null && !oldDiscriminator.PropertyName.Equals(newDiscriminator?.PropertyName))
            {
                context.PushProperty("discriminator");
                context.LogBreakingChange(ComparisonRules.DifferentDiscriminator);
                context.Pop();
            }
        }

        private static void CompareDefault(ComparisonContext context,
            JsonNode oldDefault,
            JsonNode newDefault)
        {
            if (oldDefault == null && newDefault == null)
                return;

            if (!oldDefault.DifferFrom(newDefault))
                return;

            context.PushProperty("default");
            context.LogBreakingChange(ComparisonRules.DefaultValueChanged);
            context.Pop();
        }

        private static decimal? ParseBound(string value) =>
            value == null ? null : decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

        private static void CompareConstraints(ComparisonContext context,
            IOpenApiSchema oldSchema, IOpenApiSchema newSchema)
        {
            if (ParseBound(oldSchema.Maximum ?? oldSchema.ExclusiveMaximum).DifferFrom(
                    ParseBound(newSchema.Maximum ?? newSchema.ExclusiveMaximum))
                || (oldSchema.ExclusiveMaximum != null) != (newSchema.ExclusiveMaximum != null))
            {
                CompareConstraint(context, ParseBound(oldSchema.Maximum ?? oldSchema.ExclusiveMaximum), ParseBound(newSchema.Maximum ?? newSchema.ExclusiveMaximum), "maximum", false,
                    (oldSchema.ExclusiveMaximum != null) != (newSchema.ExclusiveMaximum != null));
            }

            if (ParseBound(oldSchema.Minimum ?? oldSchema.ExclusiveMinimum).DifferFrom(
                    ParseBound(newSchema.Minimum ?? newSchema.ExclusiveMinimum))
                || (oldSchema.ExclusiveMinimum != null) != (newSchema.ExclusiveMinimum != null))
            {
                CompareConstraint(context, ParseBound(oldSchema.Minimum ?? oldSchema.ExclusiveMinimum), ParseBound(newSchema.Minimum ?? newSchema.ExclusiveMinimum), "minimum", true,
                    (oldSchema.ExclusiveMinimum != null) != (newSchema.ExclusiveMinimum != null));
            }

            if (oldSchema.MaxLength.DifferFrom(newSchema.MaxLength))
            {
                CompareConstraint(context, oldSchema.MaxLength, newSchema.MaxLength, "maxLength", false);
            }

            if (oldSchema.MinLength.DifferFrom(newSchema.MinLength))
            {
                CompareConstraint(context, oldSchema.MinLength, newSchema.MinLength, "minLength", true);
            }

            if (oldSchema.MaxItems.DifferFrom(newSchema.MaxItems))
            {
                CompareConstraint(context, oldSchema.MaxItems, newSchema.MaxItems, "maxItems", false);
            }

            if (oldSchema.MinItems.DifferFrom(newSchema.MinItems))
            {
                CompareConstraint(context, oldSchema.MinItems, newSchema.MinItems, "minItems", true);
            }

            if (oldSchema.MultipleOf.DifferFrom(newSchema.MultipleOf))
            {
                context.PushProperty("multipleOf");
                context.LogBreakingChange(ComparisonRules.ConstraintChanged, "multipleOf");
                context.Pop();
            }

            if (oldSchema.UniqueItems != newSchema.UniqueItems)
            {
                context.PushProperty("uniqueItems");
                context.LogBreakingChange(ComparisonRules.ConstraintChanged, "uniqueItems");
                context.Pop();
            }

            if (oldSchema.Pattern.DifferFrom(newSchema.Pattern))
            {
                context.PushProperty("pattern");
                context.LogBreakingChange(ComparisonRules.ConstraintChanged, "pattern");
                context.Pop();
            }
        }

        private static void CompareConstraint(ComparisonContext context, decimal? oldConstraint,
            decimal? newConstraint, string attributeName, bool isLowerBound, bool additionalCondition = false)
        {
            context.PushProperty(attributeName);
            if (additionalCondition)
            {
                context.LogBreakingChange(ComparisonRules.ConstraintChanged, attributeName);
            }
            else if (Narrows(oldConstraint, newConstraint, isLowerBound))
            {
                if (context.Direction == DataDirection.Request)
                    context.LogBreakingChange(ComparisonRules.ConstraintIsStronger, attributeName);
                else
                    context.LogInfo(ComparisonRules.ConstraintIsStronger, attributeName);
            }
            else if (Widens(oldConstraint, newConstraint, isLowerBound))
            {
                if (context.Direction == DataDirection.Response)
                    context.LogBreakingChange(ComparisonRules.ConstraintIsWeaker, attributeName);
                else
                    context.LogInfo(ComparisonRules.ConstraintIsWeaker, attributeName);
            }
            context.Pop();
        }

        private static bool Narrows(decimal? oldConstraint, decimal? newConstraint, bool isLowerBound)
        {
            if (oldConstraint == null && newConstraint == null)
                return false;

            if (oldConstraint == null)
                return true;

            if (newConstraint == null)
                return false;

            return isLowerBound
                ? newConstraint > oldConstraint
                : newConstraint < oldConstraint;
        }

        private static bool Widens(decimal? oldConstraint, decimal? newConstraint, bool isLowerBound)
        {
            if (oldConstraint == null && newConstraint == null)
                return false;

            if (oldConstraint == null)
                return false;

            if (newConstraint == null)
                return true;

            return isLowerBound
                ? newConstraint < oldConstraint
                : newConstraint > oldConstraint;
        }

        private static void CompareType(ComparisonContext context, JsonSchemaType? oldType, JsonSchemaType? newType)
        {
            if (oldType == null && newType == null)
                return;

            // Are the types the same?
            if (oldType == null || newType == null || !oldType.Equals(newType))
            {
                var oldTypeString = oldType == null ? "" : oldType.ToString().ToLowerInvariant();
                var newTypeString = newType == null ? "" : newType.ToString().ToLowerInvariant();

                context.PushProperty("type");
                context.LogBreakingChange(ComparisonRules.TypeChanged, newTypeString, oldTypeString);
                context.Pop();
            }
        }

        private void CompareItems(ComparisonContext context,
            IOpenApiSchema oldItems,
            IOpenApiSchema newItems)
        {
            if (oldItems == null || newItems == null)
                return;

            context.PushProperty("items");
            Compare(context, oldItems, newItems);
            context.Pop();
        }

        private static void CompareEnum(ComparisonContext context,
            ICollection<JsonNode> oldEnum,
            ICollection<JsonNode> newEnum,
            JsonObject enumExtension)
        {
            if (oldEnum == null && newEnum == null)
                return;

            var relaxes = newEnum == null;
            var constrains = oldEnum == null;

            context.PushProperty("enum");

            if (!relaxes && !constrains)
            {
                // 1. Look for removed elements (constraining).
                var removedEnums = oldEnum.Where(oldEnumElement => newEnum.All(oldEnumElement.DifferFrom)).ToList();
                constrains = removedEnums.Any();

                // 2. Look for added elements (relaxing).
                var addedEnums = newEnum.Where(newEnumElement => oldEnum.All(newEnumElement.DifferFrom)).ToList();
                relaxes = addedEnums.Any();

                if (constrains)
                {
                    LogAction logger = context.Direction == DataDirection.Response ? context.LogWarning : context.LogBreakingChange;
                    logger(ComparisonRules.RemovedEnumValue, string.Join(", ", removedEnums.Select(e => e.StringValue())));
                }

                if (relaxes && !IsEnumModelAsString(enumExtension))
                {
                    LogAction logger = context.Direction == DataDirection.Request ? context.LogWarning : context.LogBreakingChange;
                    logger(ComparisonRules.AddedEnumValue, string.Join(", ", addedEnums.Select(e => e.StringValue())));
                }
            }

            if (relaxes && constrains)
                context.LogInfo(ComparisonRules.ConstraintChanged, "enum");
            else if (relaxes)
                context.LogInfo(ComparisonRules.ConstraintIsWeaker, "enum");
            else if (constrains)
                context.LogInfo(ComparisonRules.ConstraintIsStronger, "enum");

            context.Pop();
        }

        private static bool IsEnumModelAsString(JsonObject enumExtension)
        {
            var isEnumModelAsString = false;
            if (enumExtension?["modelAsString"] != null && enumExtension.TryGetPropertyValue("modelAsString", out var modelAsString))
            {
                isEnumModelAsString = modelAsString.GetValue<bool>();
            }

            return isEnumModelAsString;
        }

        private static void CompareFormat(ComparisonContext context,
            IOpenApiSchema oldSchema,
            IOpenApiSchema newSchema)
        {
            if (!oldSchema.Format.DifferFrom(newSchema.Format)
                || IsFormatChangeAllowed(context, oldSchema, newSchema))
                return;

            context.PushProperty("format");
            context.LogBreakingChange(ComparisonRules.TypeFormatChanged);
            context.Pop();
        }

        private static bool IsFormatChangeAllowed(ComparisonContext context,
            IOpenApiSchema oldSchema,
            IOpenApiSchema newSchema)
        {
            if (newSchema.Type == null || newSchema.Type != JsonSchemaType.Integer || context.Strict
                || oldSchema.Format == null || newSchema.Format == null)
                return false;

            var formatChangedFromInt32ToInt64 = oldSchema.Format.Equals("int32") && newSchema.Format.Equals("int64");
            var formatChangedFromInt64ToInt32 = oldSchema.Format.Equals("int64") && newSchema.Format.Equals("int32");

            return context.Direction == DataDirection.Request && formatChangedFromInt32ToInt64
                || context.Direction == DataDirection.Response && formatChangedFromInt64ToInt32;
        }

        private static void CompareAllOf(ComparisonContext context,
            IList<IOpenApiSchema> oldAllOf, IList<IOpenApiSchema> newAllOf)
        {
            if (oldAllOf == null && newAllOf == null)
                return;

            context.PushProperty("allOf");
            if (oldAllOf == null || newAllOf == null)
            {
                context.LogBreakingChange(ComparisonRules.DifferentAllOf);
                context.Pop();
                return;
            }

            var newAllOfReferences = newAllOf.Where(schema => schema.GetReference() != null)
                .Select(schema => schema.GetReference().ReferenceV3).ToList();
            var oldAllOfReferences = oldAllOf.Where(schema => schema.GetReference() != null)
                .Select(schema => schema.GetReference().ReferenceV3).ToList();

            var differenceCount = newAllOfReferences.Except(oldAllOfReferences).Count();
            differenceCount += oldAllOfReferences.Except(newAllOfReferences).Count();

            if (differenceCount > 0)
            {
                context.LogBreakingChange(ComparisonRules.DifferentAllOf);
            }
            context.Pop();
        }

        private void CompareOneOf(
            ComparisonContext context, IList<IOpenApiSchema> oldOneOf, IList<IOpenApiSchema> newOneOf)
        {
            if (oldOneOf == null && newOneOf == null)
                return;

            context.PushProperty("oneOf");
            if (oldOneOf == null || newOneOf == null)
            {
                context.LogBreakingChange(ComparisonRules.DifferentOneOf);
                context.Pop();
                return;
            }

            var newOneOfReferences = newOneOf.Where(schema => schema.GetReference() != null)
                .Select(schema => schema.GetReference().ReferenceV3).ToList();
            var oldOneOfReferences = oldOneOf.Where(schema => schema.GetReference() != null)
                .Select(schema => schema.GetReference().ReferenceV3).ToList();

            var differenceCount = newOneOfReferences.Except(oldOneOfReferences).Count();
            differenceCount += oldOneOfReferences.Except(newOneOfReferences).Count();

            if (differenceCount > 0)
            {
                context.LogBreakingChange(ComparisonRules.DifferentOneOf);
            }

            var commonReferences = oldOneOfReferences
                .Select((value, index) => (value, index, newIndex: newOneOfReferences.FindIndex(v => v == value)))
                .Where(tuple => tuple.newIndex != -1);

            foreach (var (reference, index, newIndex) in commonReferences)
            {
                context.PushProperty(index.ToString());
                Compare(context, oldOneOf[index], newOneOf[newIndex]);
                context.Pop();
            }

            context.Pop();
        }

        private void CompareProperties(ComparisonContext context,
            IOpenApiSchema oldSchema,
            IOpenApiSchema newSchema,
            bool isSchemaReferenced)
        {
            CompareAdditionalProperties(context, oldSchema.AdditionalProperties, newSchema.AdditionalProperties);

            context.PushProperty("properties");

            CompareRemovedProperties(context, oldSchema, newSchema);

            CompareAddedProperties(context, oldSchema, newSchema, isSchemaReferenced);

            CompareCommonProperties(context, oldSchema, newSchema);

            context.Pop();
        }

        private static void CompareRemovedProperties(ComparisonContext context,
            IOpenApiSchema oldSchema, IOpenApiSchema newSchema)
        {
            if (oldSchema.Properties == null)
                return;

            var removedProperties = newSchema.Properties == null
                ? oldSchema.Properties.Keys
                : oldSchema.Properties.Keys.Where(propertyName => !newSchema.Properties.ContainsKey(propertyName));
            foreach (var propertyName in removedProperties)
            {
                context.PushProperty(propertyName);
                context.LogBreakingChange(ComparisonRules.RemovedProperty, propertyName);
                context.Pop();
            }
        }

        private static void CompareAddedProperties(ComparisonContext context,
            IOpenApiSchema oldSchema, IOpenApiSchema newSchema, bool isSchemaReferenced)
        {
            if (newSchema.Properties == null)
                return;

            var addedProperties = oldSchema.Properties == null
                ? newSchema.Properties
                : newSchema.Properties.Where(property =>
                    !oldSchema.Properties.TryGetValue(property.Key, out var oldProperty) || oldProperty == null);

            foreach (var property in addedProperties)
            {
                context.PushProperty(property.Key);

                if (oldSchema.IsPropertyRequired(property.Key))
                {
                    context.LogBreakingChange(ComparisonRules.AddedRequiredProperty, property.Key);
                }

                if (context.Direction == DataDirection.Response)
                {
                    if (property.Value.ReadOnly)
                        context.LogInfo(ComparisonRules.AddedReadOnlyPropertyInResponse, property.Key);
                    else
                        context.LogBreakingChange(ComparisonRules.AddedPropertyInResponse, property.Key);
                }
                else if (isSchemaReferenced && !newSchema.IsPropertyRequired(property.Key))
                {
                    context.LogBreakingChange(ComparisonRules.AddedOptionalProperty, property.Key);
                }

                context.Pop();
            }
        }

        private void CompareCommonProperties(ComparisonContext context,
            IOpenApiSchema oldSchema, IOpenApiSchema newSchema)
        {
            if (oldSchema.Properties == null || newSchema.Properties == null)
                return;

            var commonProperties =
                oldSchema.Properties.Where(property => newSchema.Properties.ContainsKey(property.Key));
            foreach (var property in commonProperties)
            {
                context.PushProperty(property.Key);
                Compare(context, property.Value, newSchema.Properties[property.Key]);
                context.Pop();
            }
        }

        private void CompareAdditionalProperties(ComparisonContext context,
            IOpenApiSchema oldAdditionalProperties, IOpenApiSchema newAdditionalProperties)
        {
            context.PushProperty("additionalProperties");
            if (oldAdditionalProperties == null && newAdditionalProperties != null)
            {
                context.LogBreakingChange(ComparisonRules.AddedAdditionalProperties);
            }
            else if (oldAdditionalProperties != null && newAdditionalProperties == null)
            {
                context.LogBreakingChange(ComparisonRules.RemovedAdditionalProperties);
            }
            else if (newAdditionalProperties != null)
            {
                Compare(context, oldAdditionalProperties, newAdditionalProperties);
            }
            context.Pop();
        }

        /// <summary>
        /// Compares list of required properties of this model
        /// </summary>
        /// <param name="context">Comparision Context</param>
        /// <param name="oldRequired">A set of old required properties</param>
        /// <param name="newRequired">A set of new required properties</param>
        private static void CompareRequired(ComparisonContext context,
            ISet<string> oldRequired,
            ISet<string> newRequired)
        {
            if (newRequired == null)
                return;

            if (oldRequired == null)
            {
                context.LogBreakingChange(ComparisonRules.AddedRequiredProperty, string.Join(", ", newRequired));
                return;
            }

            List<string> addedRequiredProperties = newRequired.Except(oldRequired).ToList();
            if (addedRequiredProperties.Any())
            {
                context.LogBreakingChange(ComparisonRules.AddedRequiredProperty,
                    string.Join(", ", addedRequiredProperties));
            }
        }
    }
}
