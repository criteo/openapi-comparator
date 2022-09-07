﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using Criteo.OpenApi.Comparator.Logging;

namespace Criteo.OpenApi.Comparator.UTest
{
    /// <summary>
    /// This class contains tests for the logic comparing two swagger specifications,
    /// an older version against newer version.
    ///
    /// For all but the tests that verify that version checks are done properly, the
    /// old and new specifications have the same version number, which should force
    /// the comparison logic to produce errors rather than warnings for each breaking
    /// change.
    ///
    /// Non-breaking changes are always presented as informational messages, regardless
    /// of whether the version has changed or not.
    /// </summary>
    [TestFixture]
    public class OpenApiSpecificationsCompareTests
    {
        [Test]
        public void CompareOAS_ShouldNotReturn_Differences()
        {
            var differences = CompareSpecifications("valid_oas.json");
            Assert.That(differences, Is.Empty);
        }

        [Test]
        public void CompareOAS_ShouldNotReturn_Warnings_When_NoVersionChanged()
        {
            var differences = CompareSpecifications("no_version_change.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.NoVersionChange,
                Severity = Severity.Info,
                OldJsonRef = "old/no_version_change.json#/info/version"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_VersionReversedDifferences()
        {
            var differences = CompareSpecifications("version_reversed.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.VersionsReversed,
                Severity = Severity.Error,
                OldJsonRef = "old/version_reversed.json#/info/version"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedPathDifferences()
        {
            var differences = CompareSpecifications("removed_path.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.RemovedPath,
                Severity = Severity.Warning,
                OldJsonRef = "old/removed_path.json#/paths/~1api~1Parameters~1{a}"
            });
            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.RemovedPath,
                Severity = Severity.Warning,
                OldJsonRef = "old/removed_path.json#/paths/~1api~1Responses"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedPathDifferences()
        {
            var differences =  CompareSpecifications("added_path.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.AddedPath,
                Severity = Severity.Info,
                NewJsonRef = "new/added_path.json#/paths/~1api~1Paths"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedOperationDifferences()
        {
            var differences = CompareSpecifications("removed_operation.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.RemovedOperation,
                Severity = Severity.Warning,
                NewJsonRef = "new/removed_operation.json#/paths/~1api~1Operations/post"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedOperationDifferences()
        {
            var differences = CompareSpecifications("added_operation.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.AddedOperation,
                Severity = Severity.Info,
                NewJsonRef = "new/added_operation.json#/paths/~1api~1Paths/post"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ModifiedOperationIdDifferences()
        {
            var differences = CompareSpecifications("modified_operation_id.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.ModifiedOperationId,
                Severity = Severity.Warning,
                NewJsonRef = "new/modified_operation_id.json#/paths/~1api~1Paths/get/operationId",
                OldJsonRef = "old/modified_operation_id.json#/paths/~1api~1Operations/get/operationId"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedResponseCodeDifferences()
        {
            var differences = CompareSpecifications("added_response_code.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.AddingResponseCode,
                Severity = Severity.Warning,
                NewJsonRef = "new/added_response_code.json#/paths/~1api~1Operations/post/responses/200"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedResponseCodeDifferences()
        {
            var differences = CompareSpecifications("removed_response_code.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.RemovedResponseCode,
                Severity = Severity.Warning,
                OldJsonRef = "old/removed_response_code.json#/paths/~1api~1Operations/post/responses/200"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedHeaderDifferences()
        {
            var differences = CompareSpecifications("added_header.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.AddingHeader,
                Severity = Severity.Info,
                NewJsonRef = "new/added_header.json#/paths/~1api~1Responses/get/responses/200/headers/x-c"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedHeaderDifferences()
        {
            var differences = CompareSpecifications("removed_header.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.RemovingHeader,
                Severity = Severity.Warning,
                OldJsonRef = "old/removed_header.json#/paths/~1api~1Responses/get/responses/200/headers/x-c"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_LongRunningOperationDifferences()
        {
            var differences = CompareSpecifications("long_running_operation.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.LongRunningOperationExtensionChanged,
                Severity = Severity.Warning,
                NewJsonRef = "new/long_running_operation.json#/paths/~1api~1Parameters/put/x-ms-long-running-operation"
            });
        }

        /// <summary>
        /// Verifies that if a referenced schema is removed, it's flagged.
        /// But if an unreferenced schema is removed, it's not.
        /// </summary>
        [Test]
        public void CompareOAS_ShouldReturn_RemovedSchemaDifferences()
        {
            var differences = CompareSpecifications("removed_schema.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.RemovedDefinition,
                Severity = Severity.Warning,
                OldJsonRef = "old/removed_schema.json#/schemas/Pet"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedClientParameterDifferences()
        {
            var differences = CompareSpecifications("removed_client_parameter.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.RemovedClientParameter,
                Severity = Severity.Warning,
                OldJsonRef = "old/removed_client_parameter.json#/parameters/limitParam"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedRequiredParameterDifferences()
        {
            var differences = CompareSpecifications("removed_required_parameter.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.RemovedRequiredParameter,
                Severity = Severity.Warning,
                OldJsonRef = "old/removed_required_parameter.json#/paths/~1api~1Parameters/put/parameters/1"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedRequiredParameterDifferences()
        {
            var differences = CompareSpecifications("added_required_parameter.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.AddingRequiredParameter,
                Severity = Severity.Warning,
                NewJsonRef = "new/added_required_parameter.json#/paths/~1api~1Parameters/put/parameters/1"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ParameterInHasChangedDifferences()
        {
            var differences = CompareSpecifications("parameter_in_has_changed.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.ParameterInHasChanged,
                Severity = Severity.Warning,
                NewJsonRef = "new/parameter_in_has_changed.json#/paths/~1api~1Parameters/put/parameters/0/in"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ConstantStatusHasChangedDifferences()
        {
            var differences = CompareSpecifications("constant_status_has_changed.json");

            // If the number of values in an enum increases, then a ConstraintIsWeaker message will be raised as well
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.ConstantStatusHasChanged,
                Severity = Severity.Warning,
                NewJsonRef = "new/constant_status_has_changed.json#/paths/~1api~1Parameters/put/parameters/1/enum"
            }, 1);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.ConstraintIsWeaker,
                Severity = Severity.Info,
            }, 1);
        }

        /// <summary>
        /// Verifies that if you change the reference of a parameter, it is flagged.
        /// Here the reference of a parameter and a response's item's schema is change.
        /// The name of a referenced response is also changed, but shouldn't be flagged.
        /// </summary>
        /// <remarks>
        /// Just changing the name of a parameter schema in the definitions section does not change the wire format for
        /// the parameter, so it shouldn't result in a separate error for the parameter.
        /// </remarks>
        [Test]
        public void CompareOAS_ShouldReturn_ReferenceRedirectionDifferences()
        {
            var differences = CompareSpecifications("reference_redirection.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.ReferenceRedirection,
                Severity = Severity.Warning,
                OldJsonRef = "old/reference_redirection.json#/paths/~1api~1Parameters/get/responses/200/content/application~1json/schema/items"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedEnumValueDifferences()
        {
            var differences = CompareSpecifications("removed_enum_value.json");

            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.RemovedEnumValue,
                Severity = Severity.Warning,
                OldJsonRef = "old/removed_enum_value.json#/paths/~1api~1Parameters/put/parameters/0/schema/enum"
            }, 2);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.ConstraintIsStronger,
                Severity = Severity.Info,
            }, 2);
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedEnumValueDifferences()
        {
            var differences = CompareSpecifications("added_enum_value.json");

            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.AddedEnumValue,
                Severity = Severity.Warning,
                NewJsonRef = "new/added_enum_value.json#/paths/~1api~1Parameters/put/responses/200/content/application~1json/schema/properties/petType/enum"
            }, 1);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.ConstraintIsWeaker,
                Severity = Severity.Info,
            }, 2);
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedAdditionalPropertiesDifferences()
        {
            var differences = CompareSpecifications("added_additional_properties.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.AddedAdditionalProperties,
                Severity = Severity.Warning,
                NewJsonRef = "new/added_additional_properties.json#/paths/~1api~1Parameters/get/responses/200/content/application~1json/schema/additionalProperties"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedAdditionalPropertiesDifferences()
        {
            var differences = CompareSpecifications("removed_additional_properties.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.RemovedAdditionalProperties,
                Severity = Severity.Warning,
                OldJsonRef = "old/removed_additional_properties.json#/paths/~1api~1Parameters/put/responses/200/content/application~1json/schema/additionalProperties"
            });
        }

        /// <summary>
        /// Verifies that if the format of a definition's property is changed, it's found
        /// The response format change  is counted twice: one time during the response comparison,
        /// and the other time in the components comparison
        /// The parameter format change in only counted once, because int32 -> int64 is allowed in a request direction
        /// </summary>
        [Test]
        public void CompareOAS_ShouldReturn_TypeFormatChangedDifferences()
        {
            var differences = CompareSpecifications("type_format_changed.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.TypeFormatChanged,
                Severity = Severity.Warning,
                OldJsonRef = "old/type_format_changed.json#/paths/~1pets/get/responses/200/content/application~1json/schema/properties/sleepTime/format"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RequiredStatusChangedDifferences()
        {
            var differences = CompareSpecifications("required_status_changed.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.RequiredStatusChange,
                OldJsonRef = "old/required_status_changed.json#/paths/~1pets/get/parameters/0/required"
            });
        }

        /// <summary>
        /// Verifies that if the type of a schema is changed (or a schema's property, which is also a schema), it's flagged.
        /// But if the type of an unreferenced schema is changed, it's not.
        /// </summary>
        [Test]
        public void CompareOAS_ShouldReturn_TypeChangedDifferences()
        {
            var differences = CompareSpecifications("type_changed.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.TypeChanged,
                Severity = Severity.Warning,
                OldJsonRef = "old/type_changed.json#/paths/~1pets/get/responses/200/content/application~1json/schema/items/properties/name/type"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_DefaultValueChangedDifferences()
        {
            var differences = CompareSpecifications("default_value_changed.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.DefaultValueChanged,
                Severity = Severity.Warning,
                OldJsonRef = "old/default_value_changed.json#/paths/~1pets/get/responses/200/content/application~1json/schema/items/properties/name/default"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ReadonlyPropertyChangedDifferences()
        {
            var differences = CompareSpecifications("readonly_property_changed.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.ReadonlyPropertyChanged,
                Severity = Severity.Warning,
                OldJsonRef = "old/readonly_property_changed.json#/paths/~1pets/get/responses/default/content/application~1json/schema/readOnly"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_DifferentDiscriminatorDifferences()
        {
            var differences = CompareSpecifications("different_discriminator.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.DifferentDiscriminator,
                Severity = Severity.Warning,
                OldJsonRef = "old/different_discriminator.json#/paths/~1pets/get/responses/404/content/application~1json/schema/discriminator"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedPropertyDifferences()
        {
            var differences = CompareSpecifications("removed_property.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.RemovedProperty,
                Severity = Severity.Warning,
                OldJsonRef = "old/removed_property.json#/paths/~1pets/get/responses/200/content/application~1json/schema/items/properties/petType"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedRequiredPropertyDifferences()
        {
            var differences = CompareSpecifications("added_required_property.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.AddedRequiredProperty,
                Severity = Severity.Warning,
                NewJsonRef = "new/added_required_property.json#/paths/~1pets/get/responses/200/content/application~1json/schema/items"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedOptionalParameterDifferences()
        {
            var differences = CompareSpecifications("added_optional_parameter.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.AddingOptionalParameter,
                Severity = Severity.Warning,
                NewJsonRef = "new/added_optional_parameter.json#/paths/~1api~1Parameters/put/parameters/1"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedOptionalPropertyDifferences()
        {
            var differences = CompareSpecifications("added_optional_property.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.AddedOptionalProperty,
                Severity = Severity.Warning,
                NewJsonRef = "new/added_optional_property.json#/paths/~1api~1Parameters/put/parameters/0/schema/properties/message"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ChangedParameterOrderDifferences()
        {
            var differences = CompareSpecifications("changed_parameter_order.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.ChangedParameterOrder,
                Severity = Severity.Warning,
                OldJsonRef = "old/changed_parameter_order.json#/paths/~1api~1Parameters/put/parameters"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedPropertyInResponseDifferences()
        {
            var differences = CompareSpecifications("added_property_in_response.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.AddedPropertyInResponse,
                Severity = Severity.Warning,
                NewJsonRef = "new/added_property_in_response.json#/paths/~1pets/get/responses/200/content/application~1json/schema/items/properties/petType"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedReadOnlyPropertyInResponseDifferences()
        {
            var differences = CompareSpecifications("added_readOnly_property_in_response.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.AddedReadOnlyPropertyInResponse,
                Severity = Severity.Info,
                NewJsonRef = "new/added_readOnly_property_in_response.json#/paths/~1pets/get/responses/200/content/application~1json/schema/items/properties/petType"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ConstraintChangedDifferences()
        {
            var differences = CompareSpecifications("constraint_changed.json");

            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.ConstraintChanged,
                Severity = Severity.Warning,
                OldJsonRef = "old/constraint_changed.json#/paths/~1pets/get/parameters/0/schema/properties/accessKey/pattern"
            }, 3);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.RemovedEnumValue,
                Severity = Severity.Warning
            }, 1);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.ConstraintChanged,
                Severity = Severity.Info
            }, 1);
        }

        [Test]
        public void CompareOAS_ShouldReturn_ConstraintIsStrongerDifferences()
        {
            var differences = CompareSpecifications("constraint_is_stronger.json");

            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.ConstraintIsStronger,
                Severity = Severity.Warning,
                OldJsonRef = "old/constraint_is_stronger.json#/paths/~1pets/get/parameters/0/schema/properties/minLimit/maximum"
            }, 6);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.ConstraintIsStronger,
                Severity = Severity.Info
            }, 2);
        }

        [Test]
        public void CompareOAS_ShouldReturn_ConstraintIsWeakerDifferences()
        {
            var differences = CompareSpecifications("constraint_is_weaker.json");

            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.ConstraintIsWeaker,
                Severity = Severity.Info,
                OldJsonRef = "old/constraint_is_weaker.json#/paths/~1pets/get/parameters/0/schema/properties/constrainsItems/enum"
            }, 7);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.ConstraintIsWeaker,
                Severity = Severity.Warning,
            }, 1);
        }

        [Test]
        public void CompareOAS_ShouldReturn_DifferentAllOfDifferences()
        {
            var differences = CompareSpecifications("different_allOf.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.DifferentAllOf,
                Severity = Severity.Warning,
                OldJsonRef = "old/different_allOf.json#/paths/~1pets/get/parameters/0/schema/allOf"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_OneMessagePerRule_When_RecursiveModel()
        {
            var differences = CompareSpecifications("recursive_model.json");

            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.RemovedProperty,
                Severity = Severity.Warning,
                OldJsonRef = "old/recursive_model.json#/paths/~1api~1Operations/post/parameters/0/schema/properties/error/properties/target"
            }, 1);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.ReadonlyPropertyChanged,
                Severity = Severity.Warning,
            }, 1);
        }

        [Test]
        public void CompareOAS_ShouldReturn_RequestBodyFormatNowSupportedDifferences()
        {
            var differences = CompareSpecifications("request_body_format_now_supported.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.RequestBodyFormatNowSupported,
                Severity = Severity.Info,
                NewJsonRef = "new/request_body_format_now_supported.json#/paths/~1pets/post/requestBody/content/application~1xml"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ResponseBodyFormatNowSupportedDifferences()
        {
            var differences = CompareSpecifications("response_body_format_now_supported.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.ResponseBodyFormatNowSupported,
                Severity = Severity.Info,
                NewJsonRef = "new/response_body_format_now_supported.json#/paths/~1pets/get/responses/200/content/application~1xml"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RequestBodyFormatNoLongerSupportedDifferences()
        {
            var differences = CompareSpecifications("request_body_format_no_longer_supported.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.RequestBodyFormatNoLongerSupported,
                Severity = Severity.Warning,
                OldJsonRef = "old/request_body_format_no_longer_supported.json#/paths/~1pets/post/requestBody/content/text~1plain"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ServerNoLongerSupportedDifferences()
        {
            var differences = CompareSpecifications("server_no_longer_supported.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.ServerNoLongerSupported,
                Severity = Severity.Warning,
                OldJsonRef = "old/server_no_longer_supported.json#/servers/1"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ParameterStyleChangedDifferences()
        {
            var differences = CompareSpecifications("parameter_style_changed.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.ParameterStyleChanged,
                Severity = Severity.Warning,
                OldJsonRef = "old/parameter_style_changed.json#/paths/~1pets/get/parameters/0/style"
            });
        }

        /// <summary>
        /// Comparator should return AddedOptionalProperty only if schema is referenced.
        /// Schema is referenced if one of its allOf property has a discriminator.
        /// </summary>
        [Test]
        public void CompareOAS_ShouldReturn_AddedOptionalPropertyDifferences_When_ValidPolymorphicSchema()
        {
            var differences = CompareSpecifications("polymorphic_schema.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.AddedOptionalProperty,
                Severity = Severity.Warning,
                NewJsonRef = "new/polymorphic_schema.json#/schemas/Dog/properties/breed"
            });
        }

        /// <summary>
        /// ComparePaths method should also work for x-ms-paths extension
        /// </summary>
        [Test]
        public void CompareOAS_ShouldReturn_Differences_When_XMsPathsExtension()
        {
            var differences = CompareSpecifications("x-ms-paths.json");

            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.RemovedPath,
                Severity = Severity.Warning,
                OldJsonRef = "old/x-ms-paths.json#/x-ms-paths/~1myPath~1query-drive?op=file"
            }, 1);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.RequiredStatusChange,
                Severity = Severity.Warning,
                OldJsonRef = "old/x-ms-paths.json#/x-ms-paths/~1myPath~1query-drive?op=folder/get/parameters/0/required"
            }, 1);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonMessages.TypeChanged,
                Severity = Severity.Warning,
                OldJsonRef = "old/x-ms-paths.json#/schemas/Cat/properties/sleepTime/type"
            }, 1);
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedRequestBodyDifferences()
        {
            var differences = CompareSpecifications("added_request_body.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.AddedRequestBody,
                Severity = Severity.Warning,
                NewJsonRef = "new/added_request_body.json#/paths/~1pets/post/requestBody"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedRequestBodyDifferences()
        {
            var differences = CompareSpecifications("removed_request_body.json");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonMessages.RemovedRequestBody,
                Severity = Severity.Warning,
                OldJsonRef = "old/removed_request_body.json#/paths/~1pets/post/requestBody"
            });
        }

        /// <summary>
        /// Helper method -- load two Open Api Specification documents and invoke the comparison logic.
        /// </summary>
        /// <param name="oasName">The name of the OpenApi specification document file.
        /// The file name must be the same in both the 'modified' and 'original' folder.</param>
        /// <returns>A list of messages from the comparison logic.</returns>
        private static IList<ComparisonMessage> CompareSpecifications(string oasName)
        {
            var baseDirectory = Directory.GetParent(typeof(OpenApiSpecificationsCompareTests).GetTypeInfo().Assembly.Location)
                .ToString();

            var oldFileName = Path.Combine(baseDirectory, "Resource", "old", oasName);
            var newFileName = Path.Combine(baseDirectory, "Resource", "new", oasName);

            var openApiComparator = new OpenApiComparator();
            var differences = openApiComparator.Compare(
                Path.Combine("old", oasName),
                File.ReadAllText(oldFileName),
                Path.Combine("new", oasName),
                File.ReadAllText(newFileName))
                .ToList();

            ValidateDifferences(differences);

            return differences;
        }

        private static void ValidateMessage(ComparisonMessage message)
        {
            var newLocation = message.NewLocation();
            var oldLocation = message.OldLocation();
            Assert.True(oldLocation != null || newLocation != null);

            switch (message.Mode)
            {
                case MessageType.Update:
                    Assert.NotNull(newLocation);
                    Assert.NotNull(oldLocation);
                    Assert.NotNull(message.OldJsonRef);
                    Assert.NotNull(message.NewJsonRef);
                    break;
                case MessageType.Addition:
                    Assert.NotNull(newLocation);
                    Assert.NotNull(message.NewJsonRef);
                    break;
                case MessageType.Removal:
                    Assert.NotNull(oldLocation);
                    Assert.NotNull(message.OldJsonRef);
                    break;
            }
        }

        private static void ValidateDifferences(IEnumerable<ComparisonMessage> differences)
        {
            foreach (var message in differences)
            {
                ValidateMessage(message);
            }
        }
    }

    public static class DifferencesExtension
    {
        public static void AssertContainsOnly(this IList<ComparisonMessage> differences, ExpectedDifference expectedDifference)
        {
            differences.AssertContains(expectedDifference, differences.Count);
        }

        public static void AssertContains(this IList<ComparisonMessage> differences, ExpectedDifference expectedDifference, int expectedNumberOfDifferences)
        {
            Assert.AreEqual(expectedNumberOfDifferences, differences.Count(message => message.Id == expectedDifference.Rule.Id
                && (expectedDifference.Severity == null || message.Severity == expectedDifference.Severity)));

            differences.AssertContainsJsonRef(expectedDifference);
        }

        private static void AssertContainsJsonRef(this IList<ComparisonMessage> differences, ExpectedDifference expectedDifference)
        {
            if (expectedDifference.OldJsonRef != null)
            {
                Assert.IsTrue(differences.Any(difference => difference.Id == expectedDifference.Rule.Id
                    && difference.OldJsonRef == expectedDifference.OldJsonRef));
            }

            if (expectedDifference.NewJsonRef != null)
            {
                Assert.IsTrue(differences.Any(difference => difference.Id == expectedDifference.Rule.Id
                    && difference.NewJsonRef == expectedDifference.NewJsonRef));
            }
        }
    }

    public class ExpectedDifference
    {
        public MessageTemplate Rule;

        public Severity? Severity;

        public string OldJsonRef;

        public string NewJsonRef;
    }
}