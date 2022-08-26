// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Criteo.OpenApi.Comparator.Comparators.Extensions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators
{
    /// <summary>
    /// OpenApi schema object comparator.
    /// </summary>
    public class SchemaComparator
    {
        private readonly LinkedList<OpenApiSchema> _visitedSchemas;

        private readonly IDictionary<OpenApiSchema, DataDirection> _compareDirections;

        public SchemaComparator()
        {
            _visitedSchemas = new LinkedList<OpenApiSchema>();
            _compareDirections = new Dictionary<OpenApiSchema, DataDirection>();
        }

        /// <summary>
        /// Compare a modified document node (this) to a previous one and look for breaking as well as non-breaking changes.
        /// </summary>
        /// <param name="context">The modified document context.</param>
        /// <param name="oldSchema">The original schema model.</param>
        /// <param name="newSchema">The new schema model.</param>
        /// <param name="isSchemaReferenced">True if te schema if referenced somewhere in the document</param>
        /// <returns>A list of messages from the comparison.</returns>
        public IEnumerable<ComparisonMessage> Compare(ComparisonContext<OpenApiDocument> context,
            OpenApiSchema oldSchema,
            OpenApiSchema newSchema,
            bool isSchemaReferenced = true
            )
        {
            if (oldSchema == null && newSchema == null)
                return context.Messages;

            if (oldSchema == null)
                throw new ArgumentNullException(nameof(oldSchema));

            if (newSchema == null)
                throw new ArgumentNullException(nameof(newSchema));

            if (newSchema.Reference?.ReferenceV3 != null
                && !newSchema.Reference.ReferenceV3.Equals(oldSchema.Reference?.ReferenceV3))
            {
                context.LogBreakingChange(ComparisonMessages.ReferenceRedirection);
                return context.Messages;
            }

            var areSchemasReferenced = false;
            if (!string.IsNullOrWhiteSpace(newSchema.Reference?.ReferenceV3))
            {
                newSchema = newSchema.Reference.Resolve(context.NewOpenApiDocument.Components.Schemas);
                areSchemasReferenced = true;
                if (newSchema == null)
                    return context.Messages;
            }

            if (!string.IsNullOrWhiteSpace(oldSchema.Reference?.ReferenceV3))
            {
                oldSchema = oldSchema.Reference.Resolve(context.OldOpenApiDocument.Components.Schemas);
                areSchemasReferenced = true;
                if (oldSchema == null)
                    return context.Messages;
            }

            // Avoid doing the comparison repeatedly by marking for which direction it's already been done.
            if (context.Direction != DataDirection.None)
            {
                if (!_compareDirections.ContainsKey(newSchema))
                {
                    _compareDirections[newSchema] = DataDirection.None;
                }
                // Comparing two referenced schemas in the context of a parameter or response -- did we already do this?
                if (_compareDirections[newSchema] == context.Direction
                    || _compareDirections[newSchema] == DataDirection.Both)
                    return new ComparisonMessage[0];

                _compareDirections[newSchema] |= context.Direction;
            }

            if (areSchemasReferenced)
            {
                if (_visitedSchemas.Contains(oldSchema))
                    return context.Messages;

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

            CompareProperties(context, oldSchema, newSchema, isSchemaReferenced);

            CompareRequired(context, oldSchema.Required, newSchema.Required);

            return context.Messages;
        }

        private static void CompareReadOnly(ComparisonContext<OpenApiDocument> context,
            bool oldReadOnly,
            bool newReadOnly)
        {
            if (oldReadOnly != newReadOnly)
            {
                context.PushProperty("readOnly");
                context.LogBreakingChange(
                    ComparisonMessages.ReadonlyPropertyChanged,
                    oldReadOnly.ToString().ToLower(),
                    newReadOnly.ToString().ToLower()
                );
                context.Pop();
            }
        }

        private static void CompareDiscriminator(ComparisonContext<OpenApiDocument> context,
            OpenApiDiscriminator oldDiscriminator, OpenApiDiscriminator newDiscriminator)
        {
            if (oldDiscriminator == null && newDiscriminator != null
                || oldDiscriminator?.PropertyName != null && !oldDiscriminator.PropertyName.Equals(newDiscriminator?.PropertyName))
            {
                context.PushProperty("discriminator");
                context.LogBreakingChange(ComparisonMessages.DifferentDiscriminator);
                context.Pop();
            }
        }

        private static void CompareDefault(ComparisonContext<OpenApiDocument> context,
            IOpenApiAny oldDefault,
            IOpenApiAny newDefault)
        {
            if (oldDefault == null && newDefault == null)
                return;

            if (!oldDefault.DifferFrom(newDefault))
                return;

            context.PushProperty("default");
            context.LogBreakingChange(ComparisonMessages.DefaultValueChanged);
            context.Pop();
        }

         private static void CompareConstraints(ComparisonContext<OpenApiDocument> context,
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
                context.LogBreakingChange(ComparisonMessages.ConstraintChanged, "multipleOf");
                context.Pop();
            }

            if (oldSchema.UniqueItems != newSchema.UniqueItems)
            {
                context.PushProperty("uniqueItems");
                context.LogBreakingChange(ComparisonMessages.ConstraintChanged, "uniqueItems");
                context.Pop();
            }

            if (oldSchema.Pattern.DifferFrom(newSchema.Pattern))
            {
                context.PushProperty("pattern");
                context.LogBreakingChange(ComparisonMessages.ConstraintChanged, "pattern");
                context.Pop();
            }
        }

         private static void CompareConstraint(ComparisonContext<OpenApiDocument> context, decimal? oldConstraint,
             decimal? newConstraint, string attributeName, bool isLowerBound, bool additionalCondition = false)
         {
             context.PushProperty(attributeName);
             if (additionalCondition)
             {
                 context.LogBreakingChange(ComparisonMessages.ConstraintChanged, attributeName);
             }
             else if (Narrows(oldConstraint, newConstraint, isLowerBound))
             {
                 if (context.Direction == DataDirection.Request)
                    context.LogBreakingChange(ComparisonMessages.ConstraintIsStronger, attributeName);
                 else
                    context.LogInfo(ComparisonMessages.ConstraintIsStronger, attributeName);
             }
             else if (Widens(oldConstraint, newConstraint, isLowerBound))
             {
                 if (context.Direction == DataDirection.Response)
                    context.LogBreakingChange(ComparisonMessages.ConstraintIsWeaker, attributeName);
                 else
                    context.LogInfo(ComparisonMessages.ConstraintIsWeaker, attributeName);
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

        private static void CompareType(ComparisonContext<OpenApiDocument> context, string oldType, string newType)
        {
            if (oldType == null && newType == null)
                return;

            // Are the types the same?
            if (oldType == null || newType == null || !oldType.Equals(newType))
            {
                var oldTypeString = oldType == null ? "" : oldType.ToLower();
                var newTypeString = newType == null ? "" : newType.ToLower();

                context.PushProperty("type");
                context.LogBreakingChange(ComparisonMessages.TypeChanged, newTypeString, oldTypeString);
                context.Pop();
            }
        }

        private void CompareItems(ComparisonContext<OpenApiDocument> context,
            OpenApiSchema oldItems,
            OpenApiSchema newItems)
        {
            if (oldItems == null || newItems == null) return;

            context.PushProperty("items");
            Compare(context, oldItems, newItems);
            context.Pop();
        }

        private static void CompareEnum(ComparisonContext<OpenApiDocument> context,
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

                if (context.Direction == DataDirection.Request && constrains)
                {
                    context.LogBreakingChange(ComparisonMessages.RemovedEnumValue,
                            string.Join(", ", removedEnums));
                }

                if (context.Direction == DataDirection.Response && relaxes)
                {
                    if (!IsEnumModelAsString(enumExtension))
                    {
                        context.LogBreakingChange(ComparisonMessages.AddedEnumValue,
                            string.Join(", ", addedEnums));
                    }
                }
            }

            if (relaxes && constrains)
                context.LogInfo(ComparisonMessages.ConstraintChanged, "enum");
            else if (relaxes)
                context.LogInfo(ComparisonMessages.ConstraintIsWeaker, "enum");
            else if (constrains)
                context.LogInfo(ComparisonMessages.ConstraintIsStronger, "enum");

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

        private static void CompareFormat(ComparisonContext<OpenApiDocument> context,
            OpenApiSchema oldSchema,
            OpenApiSchema newSchema)
        {
            if (!oldSchema.Format.DifferFrom(newSchema.Format)
                || IsFormatChangeAllowed(context, oldSchema, newSchema))
                return;

            context.PushProperty("format");
            context.LogBreakingChange(ComparisonMessages.TypeFormatChanged);
            context.Pop();
        }

        private static bool IsFormatChangeAllowed(ComparisonContext<OpenApiDocument> context,
            OpenApiSchema oldSchema,
            OpenApiSchema newSchema)
        {
            if (newSchema.Type == null || !newSchema.Type.Equals("integer") || context.Strict
                || oldSchema.Format == null || newSchema.Format == null)
                return false;

            var formatChangedFromInt32ToInt64 = oldSchema.Format.Equals("int32") && newSchema.Format.Equals("int64");
            var formatChangedFromInt64ToInt32 = oldSchema.Format.Equals("int64") && newSchema.Format.Equals("int32");

            return context.Direction == DataDirection.Request && formatChangedFromInt32ToInt64
                || context.Direction == DataDirection.Response && formatChangedFromInt64ToInt32;
        }

        private static void CompareAllOf(ComparisonContext<OpenApiDocument> context,
            IList<OpenApiSchema> oldAllOf, IList<OpenApiSchema> newAllOf)
        {
            if (oldAllOf == null && newAllOf == null)
                return;

            context.PushProperty("allOf");
            if (oldAllOf == null || newAllOf == null)
            {
                context.LogBreakingChange(ComparisonMessages.DifferentAllOf);
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
                context.LogBreakingChange(ComparisonMessages.DifferentAllOf);
            }
            context.Pop();
        }

        private void CompareProperties(ComparisonContext<OpenApiDocument> context,
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

        private static void CompareRemovedProperties(ComparisonContext<OpenApiDocument> context,
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
                context.LogBreakingChange(ComparisonMessages.RemovedProperty, propertyName);
                context.Pop();
            }
        }

        private static void CompareAddedProperties(ComparisonContext<OpenApiDocument> context,
            OpenApiSchema oldSchema, OpenApiSchema newSchema, bool isSchemaReferenced)
        {
            if (newSchema.Properties == null)
                return;

            var addedProperties = oldSchema.Properties == null
                ? newSchema.Properties
                : newSchema.Properties.Where(property =>
                    !oldSchema.Properties.TryGetValue(property.Key, out var oldProperty) || oldProperty == null);

            foreach (var (propertyName, addedProperty) in addedProperties)
            {
                context.PushProperty(propertyName);

                if (oldSchema.IsPropertyRequired(propertyName))
                {
                    context.LogBreakingChange(ComparisonMessages.AddedRequiredProperty, propertyName);
                }

                if (context.Direction == DataDirection.Response)
                {
                    if (addedProperty.ReadOnly)
                        context.LogInfo(ComparisonMessages.AddedReadOnlyPropertyInResponse, propertyName);
                    else
                        context.LogBreakingChange(ComparisonMessages.AddedPropertyInResponse, propertyName);
                }
                else if (isSchemaReferenced && !newSchema.IsPropertyRequired(propertyName))
                {
                    context.LogBreakingChange(ComparisonMessages.AddedOptionalProperty, propertyName);
                }

                context.Pop();
            }
        }

        private void CompareCommonProperties(ComparisonContext<OpenApiDocument> context,
            OpenApiSchema oldSchema, OpenApiSchema newSchema)
        {
            if (oldSchema.Properties == null || newSchema.Properties == null)
                return;

            var commonProperties =
                oldSchema.Properties.Where(property => newSchema.Properties.ContainsKey(property.Key));
            foreach (var (propertyName, oldProperty) in commonProperties)
            {
                context.PushProperty(propertyName);
                Compare(context, oldProperty, newSchema.Properties[propertyName]);
                context.Pop();
            }
        }

        private void CompareAdditionalProperties(ComparisonContext<OpenApiDocument> context,
            OpenApiSchema oldAdditionalProperties, OpenApiSchema newAdditionalProperties)
        {
            context.PushProperty("additionalProperties");
            if (oldAdditionalProperties == null && newAdditionalProperties != null)
            {
                context.LogBreakingChange(ComparisonMessages.AddedAdditionalProperties);
            }
            else if (oldAdditionalProperties != null && newAdditionalProperties == null)
            {
                context.LogBreakingChange(ComparisonMessages.RemovedAdditionalProperties);
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
        private static void CompareRequired(ComparisonContext<OpenApiDocument> context,
            ISet<string> oldRequired,
            ISet<string> newRequired)
        {
            if (newRequired == null)
                return;

            if (oldRequired == null)
            {
                context.LogBreakingChange(ComparisonMessages.AddedRequiredProperty, string.Join(", ", newRequired));
                return;
            }

            List<string> addedRequiredProperties = newRequired.Except(oldRequired).ToList();
            if (addedRequiredProperties.Any())
            {
                context.LogBreakingChange(ComparisonMessages.AddedRequiredProperty,
                    string.Join(", ", addedRequiredProperties));
            }
        }
    }
}
