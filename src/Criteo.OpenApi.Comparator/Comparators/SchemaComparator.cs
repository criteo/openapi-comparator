// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Criteo.OpenApi.Comparator.Comparators.Extensions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators
{
    internal class SchemaComparator
    {
        private readonly LinkedList<OpenApiSchema> _visitedSchemas;

        private readonly IDictionary<OpenApiSchema, DataDirection> _compareDirections;

        internal SchemaComparator()
        {
            _visitedSchemas = new LinkedList<OpenApiSchema>();
            _compareDirections = new Dictionary<OpenApiSchema, DataDirection>();
        }

        internal void Compare(ComparisonContext context,
            OpenApiSchema oldSchema,
            OpenApiSchema newSchema,
            bool isSchemaReferenced = true)
        {
            if (oldSchema == null && newSchema == null)
                return;

            if (oldSchema == null)
            {
                context.Log(ComparisonRules.AddedSchema);
                return;
            }

            if (newSchema == null)
            {
                context.Log(ComparisonRules.RemovedDefinition, default(string));
                return;
            }

            if (newSchema.Reference?.ReferenceV3 != null
                && !newSchema.Reference.ReferenceV3.Equals(oldSchema.Reference?.ReferenceV3))
            {
                context.Log(ComparisonRules.ReferenceRedirection);
            }

            var areSchemasReferenced = false;
            if (!string.IsNullOrWhiteSpace(newSchema.Reference?.ReferenceV3))
            {
                newSchema = newSchema.Reference.Resolve(context.NewOpenApiDocument.Components.Schemas);
                areSchemasReferenced = true;
                if (newSchema == null)
                    return;
            }

            if (!string.IsNullOrWhiteSpace(oldSchema.Reference?.ReferenceV3))
            {
                oldSchema = oldSchema.Reference.Resolve(context.OldOpenApiDocument.Components.Schemas);
                areSchemasReferenced = true;
                if (oldSchema == null)
                    return;
            }

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

            CompareType(context, oldSchema.Type, newSchema.Type);

            CompareItems(context, oldSchema.Items, newSchema.Items);

            oldSchema.Extensions.TryGetValue("x-ms-enum", out var enumExtension);
            CompareEnum(context, oldSchema.Enum, newSchema.Enum, enumExtension as OpenApiObject);

            CompareFormat(context, oldSchema, newSchema);

            CompareAllOf(context, oldSchema.AllOf, newSchema.AllOf);

            CompareOneOf(context, oldSchema.OneOf, newSchema.OneOf);

            CompareProperties(context, oldSchema, newSchema, isSchemaReferenced);

            CompareRequired(context, oldSchema, newSchema);

            CompareNullable(context, oldSchema.Nullable, newSchema.Nullable);
        }

        private static void CompareNullable(ComparisonContext context,
            bool oldNullable,
            bool newNullable)
        {
            if (oldNullable == newNullable) return;
            context.PushProperty("nullable");
            context.Log(
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
                context.Log(
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
                context.Log(ComparisonRules.DifferentDiscriminator);
                context.Pop();
            }
        }

        private static void CompareDefault(ComparisonContext context,
            IOpenApiAny oldDefault,
            IOpenApiAny newDefault)
        {
            if (oldDefault == null && newDefault == null)
                return;

            if (!oldDefault.DifferFrom(newDefault))
                return;

            context.PushProperty("default");
            context.Log(ComparisonRules.DefaultValueChanged);
            context.Pop();
        }

         private static void CompareConstraints(ComparisonContext context,
             OpenApiSchema oldSchema, OpenApiSchema newSchema)
        {
            if (oldSchema.Maximum.DifferFrom(newSchema.Maximum)
                || oldSchema.ExclusiveMaximum != newSchema.ExclusiveMaximum)
            {
                CompareConstraint(context, oldSchema.Maximum, newSchema.Maximum, "maximum", false,
                    oldSchema.ExclusiveMaximum != newSchema.ExclusiveMaximum);
            }

            if (oldSchema.Minimum.DifferFrom(newSchema.Minimum)
                || oldSchema.ExclusiveMinimum != newSchema.ExclusiveMinimum)
            {
                CompareConstraint(context, oldSchema.Minimum, newSchema.Minimum, "minimum", true,
                    oldSchema.ExclusiveMinimum != newSchema.ExclusiveMinimum);
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
                context.Log(ComparisonRules.MultipleOfConstraintChanged, "multipleOf");
                context.Pop();
            }

            if (oldSchema.UniqueItems != newSchema.UniqueItems)
            {
                context.PushProperty("uniqueItems");
                context.Log(ComparisonRules.UniqueItemsConstraintChanged, "uniqueItems");
                context.Pop();
            }

            if (oldSchema.Pattern.DifferFrom(newSchema.Pattern))
            {
                context.PushProperty("pattern");
                context.Log(ComparisonRules.PatternConstraintChanged, "pattern");
                context.Pop();
            }
        }

         private static void CompareConstraint(ComparisonContext context, decimal? oldConstraint,
             decimal? newConstraint, string attributeName, bool isLowerBound, bool additionalCondition = false)
         {
             context.PushProperty(attributeName);
             if (additionalCondition)
             {
                 context.Log(ComparisonRules.ConstraintChanged, attributeName);
             }
             else if (Narrows(oldConstraint, newConstraint, isLowerBound))
             {
                 if (context.Direction == DataDirection.Request)
                    context.Log(ComparisonRules.ConstraintIsStronger, attributeName);
                 else
                    context.Log(ComparisonRules.ResponseConstraintIsStronger, attributeName);
             }
             else if (Widens(oldConstraint, newConstraint, isLowerBound))
             {
                 if (context.Direction == DataDirection.Response)
                    context.Log(ComparisonRules.ConstraintIsWeaker, attributeName);
                 else
                    context.Log(ComparisonRules.RequestConstraintIsWeaker, attributeName);
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

        private static void CompareType(ComparisonContext context, string oldType, string newType)
        {
            if (oldType == null && newType == null)
                return;

            // Are the types the same?
            if (oldType == null || newType == null || !oldType.Equals(newType))
            {
                var oldTypeString = oldType == null ? "" : oldType.ToLower();
                var newTypeString = newType == null ? "" : newType.ToLower();

                context.PushProperty("type");
                context.Log(ComparisonRules.TypeChanged, newTypeString, oldTypeString);
                context.Pop();
            }
        }

        private void CompareItems(ComparisonContext context,
            OpenApiSchema oldItems,
            OpenApiSchema newItems)
        {
            if (oldItems == null || newItems == null) return;

            context.PushProperty("items");
            Compare(context, oldItems, newItems);
            context.Pop();
        }

        private static void CompareEnum(ComparisonContext context,
            ICollection<IOpenApiAny> oldEnum,
            ICollection<IOpenApiAny> newEnum,
            OpenApiObject enumExtension)
        {
            if (oldEnum == null && newEnum == null) return;

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
                    var rule = context.Direction == DataDirection.Response ? ComparisonRules.RemovedEnumResponseValue : ComparisonRules.RemovedEnumValue;
                    context.Log(rule, string.Join(", ", removedEnums.Select(e => e.StringValue())));
                }

                if (relaxes && !IsEnumModelAsString(enumExtension))
                {
                    var rule = context.Direction == DataDirection.Request ? ComparisonRules.AddedEnumRequestValue : ComparisonRules.AddedEnumValue;
                    context.Log(rule, string.Join(", ", addedEnums.Select(e => e.StringValue())));
                }
            }

            if (relaxes && constrains)
                context.Log(ComparisonRules.EnumConstraintChanged, "enum");
            else if (relaxes)
                context.Log(ComparisonRules.EnumConstraintIsWeaker, "enum");
            else if (constrains)
                context.Log(ComparisonRules.EnumConstraintIsStronger, "enum");

            context.Pop();
        }

        private static bool IsEnumModelAsString(OpenApiObject enumExtension)
        {
            var isEnumModelAsString = false;
            if (enumExtension?["modelAsString"] != null && enumExtension.TryGetValue("modelAsString", out var modelAsString))
            {
                isEnumModelAsString = (modelAsString as OpenApiBoolean)?.Value ?? false;
            }

            return isEnumModelAsString;
        }

        private static void CompareFormat(ComparisonContext context,
            OpenApiSchema oldSchema,
            OpenApiSchema newSchema)
        {
            if (!oldSchema.Format.DifferFrom(newSchema.Format))
                return;

            context.PushProperty("format");
            if (IsFormatChangeAllowed(context, oldSchema, newSchema))
                context.Log(ComparisonRules.WideningTypeFormatChanged);
            else
                context.Log(ComparisonRules.TypeFormatChanged);
            context.Pop();
        }

        private static bool IsFormatChangeAllowed(ComparisonContext context,
            OpenApiSchema oldSchema,
            OpenApiSchema newSchema)
        {
            if (newSchema.Type == null || !newSchema.Type.Equals("integer") 
                || oldSchema.Format == null || newSchema.Format == null)
                return false;

            var formatChangedFromInt32ToInt64 = oldSchema.Format.Equals("int32") && newSchema.Format.Equals("int64");
            var formatChangedFromInt64ToInt32 = oldSchema.Format.Equals("int64") && newSchema.Format.Equals("int32");

            return context.Direction == DataDirection.Request && formatChangedFromInt32ToInt64
                || context.Direction == DataDirection.Response && formatChangedFromInt64ToInt32;
        }

        private static void CompareAllOf(ComparisonContext context,
            IList<OpenApiSchema> oldAllOf, IList<OpenApiSchema> newAllOf)
        {
            if (oldAllOf == null && newAllOf == null)
                return;

            context.PushProperty("allOf");
            if (oldAllOf == null || newAllOf == null)
            {
                context.Log(ComparisonRules.DifferentAllOf);
                context.Pop();
                return;
            }

            var newAllOfReferences = newAllOf.Where(schema => schema.Reference != null)
                .Select(schema => schema.Reference.ReferenceV3).ToList();
            var oldAllOfReferences = oldAllOf.Where(schema => schema.Reference != null)
                .Select(schema => schema.Reference.ReferenceV3).ToList();

            var differenceCount = newAllOfReferences.Except(oldAllOfReferences).Count();
            differenceCount += oldAllOfReferences.Except(newAllOfReferences).Count();

            if (differenceCount > 0)
            {
                context.Log(ComparisonRules.DifferentAllOf);
            }
            context.Pop();
        }

        private void CompareOneOf(
            ComparisonContext context, IList<OpenApiSchema> oldOneOf, IList<OpenApiSchema> newOneOf)
        {
            if (oldOneOf == null && newOneOf == null)
                return;

            context.PushProperty("oneOf");
            if (oldOneOf == null || newOneOf == null)
            {
                context.Log(ComparisonRules.DifferentOneOf);
                context.Pop();
                return;
            }

            var newOneOfReferences = newOneOf.Where(schema => schema.Reference != null)
                .Select(schema => schema.Reference.ReferenceV3).ToList();
            var oldOneOfReferences = oldOneOf.Where(schema => schema.Reference != null)
                .Select(schema => schema.Reference.ReferenceV3).ToList();

            var differenceCount = newOneOfReferences.Except(oldOneOfReferences).Count();
            differenceCount += oldOneOfReferences.Except(newOneOfReferences).Count();

            if (differenceCount > 0)
            {
                context.Log(ComparisonRules.DifferentOneOf);
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
            OpenApiSchema oldSchema,
            OpenApiSchema newSchema,
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
            OpenApiSchema oldSchema, OpenApiSchema newSchema)
        {
            if (oldSchema.Properties == null)
                return;

            var removedProperties = newSchema.Properties == null
                ? oldSchema.Properties.Keys
                : oldSchema.Properties.Keys.Where(propertyName => !newSchema.Properties.ContainsKey(propertyName));
            foreach (var propertyName in removedProperties)
            {
                context.PushProperty(propertyName);
                context.Log(ComparisonRules.RemovedProperty, propertyName);
                context.Pop();
            }
        }

        private static void CompareAddedProperties(ComparisonContext context,
            OpenApiSchema oldSchema, OpenApiSchema newSchema, bool isSchemaReferenced)
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

                if (context.Direction == DataDirection.Response)
                {
                    if (property.Value.ReadOnly)
                        context.Log(ComparisonRules.AddedReadOnlyPropertyInResponse, property.Key);
                    else if (newSchema.AdditionalPropertiesAllowed && oldSchema.AdditionalPropertiesAllowed)
                        context.Log(ComparisonRules.AddedPropertyInResponse, property.Key);
                    else
                        context.Log(ComparisonRules.AddedBreakingPropertyInResponse, property.Key);
                }
                else if (isSchemaReferenced && !newSchema.IsPropertyRequired(property.Key))
                {
                    if (oldSchema.IsPropertyRequired(property.Key))
                    {
                        context.Log(ComparisonRules.AddedRequiredProperty, property.Key);
                    }
                    else
                    {
                        context.Log(ComparisonRules.AddedOptionalProperty, property.Key);
                    }
                }

                context.Pop();
            }
        }

        private void CompareCommonProperties(ComparisonContext context,
            OpenApiSchema oldSchema, OpenApiSchema newSchema)
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
            OpenApiSchema oldAdditionalProperties, OpenApiSchema newAdditionalProperties)
        {
            context.PushProperty("additionalProperties");
            if (oldAdditionalProperties == null && newAdditionalProperties != null)
            {
                context.Log(ComparisonRules.AddedAdditionalProperties);
            }
            else if (oldAdditionalProperties != null && newAdditionalProperties == null)
            {
                context.Log(ComparisonRules.RemovedAdditionalProperties);
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
            OpenApiSchema oldRequired,
            OpenApiSchema newRequired)
        {
            if (newRequired == null)
                return;

            if (oldRequired == null)
            {
                context.Log(ComparisonRules.AddedRequiredProperty, string.Join(", ", newRequired));
                return;
            }

            List<string> addedRequiredProperties = newRequired.Required.Except(oldRequired.Required).ToList();
            if (addedRequiredProperties.Any())
            {
                if (context.Direction == DataDirection.Request || !oldRequired.AdditionalPropertiesAllowed || ! newRequired.AdditionalPropertiesAllowed)
                {
                    context.Log(ComparisonRules.AddedRequiredProperty,
                    string.Join(", ", addedRequiredProperties));
                }
                else
                {
                    context.Log(ComparisonRules.AddedRequiredResponseProperty,
                        string.Join(", ", addedRequiredProperties));
                }
            }
        }
    }
}
