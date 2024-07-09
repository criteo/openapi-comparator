// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

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
            var differences = CompareSpecifications("valid_oas");
            Assert.That(differences, Is.Empty);
        }

        [Test]
        public void CompareOAS_ShouldNotReturn_Warnings_When_NoVersionChanged()
        {
            var differences = CompareSpecifications("no_version_change");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.NoVersionChange,
                Severity = Severity.Info,
                OldJsonRef = "#/info/version"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_VersionReversedDifferences()
        {
            var differences = CompareSpecifications("version_reversed");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.VersionsReversed,
                Severity = Severity.Error,
                OldJsonRef = "#/info/version"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedPathDifferences()
        {
            var differences = CompareSpecifications("removed_path");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedPath,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1api~1Parameters~1{a}"
            });
            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedPath,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1api~1Responses"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedPathDifferences()
        {
            var differences =  CompareSpecifications("added_path");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.AddedPath,
                Severity = Severity.Info,
                NewJsonRef = "#/paths/~1api~1Paths"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedOperationDifferences()
        {
            var differences = CompareSpecifications("removed_operation");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedOperation,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1api~1Operations/post"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedOperationDifferences()
        {
            var differences = CompareSpecifications("added_operation");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.AddedOperation,
                Severity = Severity.Info,
                NewJsonRef = "#/paths/~1api~1Paths/post"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ModifiedOperationIdDifferences()
        {
            var differences = CompareSpecifications("modified_operation_id");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.ModifiedOperationId,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1api~1Paths/get/operationId",
                OldJsonRef = "#/paths/~1api~1Operations/get/operationId"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedResponseCodeDifferences()
        {
            var differences = CompareSpecifications("added_response_code");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.AddingResponseCode,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1api~1Operations/post/responses/200"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedResponseCodeDifferences()
        {
            var differences = CompareSpecifications("removed_response_code");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedResponseCode,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1api~1Operations/post/responses/200"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedHeaderDifferences()
        {
            var differences = CompareSpecifications("added_header");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.AddingHeader,
                Severity = Severity.Info,
                NewJsonRef = "#/paths/~1api~1Responses/get/responses/200/headers/x-c"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedHeaderDifferences()
        {
            var differences = CompareSpecifications("removed_header");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovingHeader,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1api~1Responses/get/responses/200/headers/x-c"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_LongRunningOperationDifferences()
        {
            var differences = CompareSpecifications("long_running_operation");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.LongRunningOperationExtensionChanged,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1api~1Parameters/put/x-ms-long-running-operation"
            });
        }

        /// <summary>
        /// Verifies that if a referenced schema is removed, it's flagged.
        /// But if an unreferenced schema is removed, it's not.
        /// </summary>
        [Test]
        public void CompareOAS_ShouldReturn_RemovedSchemaDifferences()
        {
            var differences = CompareSpecifications("removed_schema");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedDefinition,
                Severity = Severity.Warning,
                OldJsonRef = "#/schemas/Pet"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedClientParameterDifferences()
        {
            var differences = CompareSpecifications("removed_client_parameter");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedClientParameter,
                Severity = Severity.Warning,
                OldJsonRef = "#/parameters/limitParam"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedRequiredParameterDifferences()
        {
            var differences = CompareSpecifications("removed_required_parameter");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedRequiredParameter,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1api~1Parameters/put/parameters/1"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedRequiredParameterDifferences()
        {
            var differences = CompareSpecifications("added_required_parameter");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.AddingRequiredParameter,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1api~1Parameters/put/parameters/1"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ParameterInHasChangedDifferences()
        {
            var differences = CompareSpecifications("parameter_in_has_changed");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.ParameterInHasChanged,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1api~1Parameters/put/parameters/0/in"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ConstantStatusHasChangedDifferences()
        {
            var differences = CompareSpecifications("constant_status_has_changed");

            // If the number of values in an enum increases, then a ConstraintIsWeaker message will be raised as well
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.ConstantStatusHasChanged,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1api~1Parameters/put/parameters/1/enum"
            }, 1);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.ConstraintIsWeaker,
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
            var differences = CompareSpecifications("reference_redirection");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.ReferenceRedirection,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1api~1Parameters/get/responses/200/content/application~1json/schema/items"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedEnumValueDifferences()
        {
            var differences = CompareSpecifications("removed_enum_value");

            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedEnumValue,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1api~1Parameters/put/parameters/0/schema/enum"
            }, 2);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.ConstraintIsStronger,
                Severity = Severity.Info,
            }, 2);
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedEnumValueDifferences()
        {
            var differences = CompareSpecifications("added_enum_value");

            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.AddedEnumValue,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1api~1Parameters/put/responses/200/content/application~1json/schema/properties/petType/enum"
            }, 1);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.ConstraintIsWeaker,
                Severity = Severity.Info,
            }, 2);
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedAdditionalPropertiesDifferences()
        {
            var differences = CompareSpecifications("added_additional_properties");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.AddedAdditionalProperties,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1api~1Parameters/get/responses/200/content/application~1json/schema/additionalProperties"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedAdditionalPropertiesDifferences()
        {
            var differences = CompareSpecifications("removed_additional_properties");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedAdditionalProperties,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1api~1Parameters/put/responses/200/content/application~1json/schema/additionalProperties"
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
            var differences = CompareSpecifications("type_format_changed");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.TypeFormatChanged,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1pets/get/responses/200/content/application~1json/schema/properties/sleepTime/format"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RequiredStatusChangedDifferences()
        {
            var differences = CompareSpecifications("required_status_changed");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.RequiredStatusChange,
                OldJsonRef = "#/paths/~1pets/get/parameters/0/required"
            });
        }

        /// <summary>
        /// Verifies that if the type of a schema is changed (or a schema's property, which is also a schema), it's flagged.
        /// But if the type of an unreferenced schema is changed, it's not.
        /// </summary>
        [Test]
        public void CompareOAS_ShouldReturn_TypeChangedDifferences()
        {
            var differences = CompareSpecifications("type_changed");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.TypeChanged,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1pets/get/responses/200/content/application~1json/schema/items/properties/name/type"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_DefaultValueChangedDifferences()
        {
            var differences = CompareSpecifications("default_value_changed");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.DefaultValueChanged,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1pets/get/responses/200/content/application~1json/schema/items/properties/name/default"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ReadonlyPropertyChangedDifferences()
        {
            var differences = CompareSpecifications("readonly_property_changed");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.ReadonlyPropertyChanged,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1pets/get/responses/default/content/application~1json/schema/readOnly"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_DifferentDiscriminatorDifferences()
        {
            var differences = CompareSpecifications("different_discriminator");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.DifferentDiscriminator,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1pets/get/responses/404/content/application~1json/schema/discriminator"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedPropertyDifferences()
        {
            var differences = CompareSpecifications("removed_property");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedProperty,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1pets/get/responses/200/content/application~1json/schema/items/properties/petType"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedRequiredPropertyDifferences()
        {
            var differences = CompareSpecifications("added_required_property");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.AddedRequiredProperty,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1pets/get/responses/200/content/application~1json/schema/items"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedOptionalParameterDifferences()
        {
            var differences = CompareSpecifications("added_optional_parameter");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.AddingOptionalParameter,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1api~1Parameters/put/parameters/1"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedOptionalPropertyDifferences()
        {
            var differences = CompareSpecifications("added_optional_property");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.AddedOptionalProperty,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1api~1Parameters/put/parameters/0/schema/properties/message"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ChangedParameterOrderDifferences()
        {
            var differences = CompareSpecifications("changed_parameter_order");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.ChangedParameterOrder,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1api~1Parameters/put/parameters"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedPropertyInResponseDifferences()
        {
            var differences = CompareSpecifications("added_property_in_response");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.AddedPropertyInResponse,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1pets/get/responses/200/content/application~1json/schema/items/properties/petType"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedReadOnlyPropertyInResponseDifferences()
        {
            var differences = CompareSpecifications("added_readOnly_property_in_response");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.AddedReadOnlyPropertyInResponse,
                Severity = Severity.Info,
                NewJsonRef = "#/paths/~1pets/get/responses/200/content/application~1json/schema/items/properties/petType"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ConstraintChangedDifferences()
        {
            var differences = CompareSpecifications("constraint_changed");

            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.ConstraintChanged,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1pets/get/parameters/0/schema/properties/accessKey/pattern"
            }, 3);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedEnumValue,
                Severity = Severity.Warning
            }, 1);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.ConstraintChanged,
                Severity = Severity.Info
            }, 1);
        }

        [Test]
        public void CompareOAS_ShouldReturn_ConstraintIsStrongerDifferences()
        {
            var differences = CompareSpecifications("constraint_is_stronger");

            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.ConstraintIsStronger,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1pets/get/parameters/0/schema/properties/minLimit/maximum"
            }, 6);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.ConstraintIsStronger,
                Severity = Severity.Info
            }, 2);
        }

        [Test]
        public void CompareOAS_ShouldReturn_ConstraintIsWeakerDifferences()
        {
            var differences = CompareSpecifications("constraint_is_weaker");

            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.ConstraintIsWeaker,
                Severity = Severity.Info,
                OldJsonRef = "#/paths/~1pets/get/parameters/0/schema/properties/constrainsItems/enum"
            }, 7);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.ConstraintIsWeaker,
                Severity = Severity.Warning,
            }, 1);
        }

        [Test]
        public void CompareOAS_ShouldReturn_DifferentAllOfDifferences()
        {
            var differences = CompareSpecifications("different_allOf");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.DifferentAllOf,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1pets/get/parameters/0/schema/allOf"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_OneMessagePerRule_When_RecursiveModel()
        {
            var differences = CompareSpecifications("recursive_model");

            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedProperty,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1api~1Operations/post/parameters/0/schema/properties/error/properties/target"
            }, 1);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.ReadonlyPropertyChanged,
                Severity = Severity.Warning,
            }, 1);
        }

        [Test]
        public void CompareOAS_ShouldReturn_RequestBodyFormatNowSupportedDifferences()
        {
            var differences = CompareSpecifications("request_body_format_now_supported");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.RequestBodyFormatNowSupported,
                Severity = Severity.Info,
                NewJsonRef = "#/paths/~1pets/post/requestBody/content/application~1xml"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ResponseBodyFormatNowSupportedDifferences()
        {
            var differences = CompareSpecifications("response_body_format_now_supported");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.ResponseBodyFormatNowSupported,
                Severity = Severity.Info,
                NewJsonRef = "#/paths/~1pets/get/responses/200/content/application~1xml"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RequestBodyFormatNoLongerSupportedDifferences()
        {
            var differences = CompareSpecifications("request_body_format_no_longer_supported");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.RequestBodyFormatNoLongerSupported,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1pets/post/requestBody/content/text~1plain"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ServerNoLongerSupportedDifferences()
        {
            var differences = CompareSpecifications("server_no_longer_supported");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.ServerNoLongerSupported,
                Severity = Severity.Warning,
                OldJsonRef = "#/servers/1"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_ParameterStyleChangedDifferences()
        {
            var differences = CompareSpecifications("parameter_style_changed");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.ParameterStyleChanged,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1pets/get/parameters/0/style"
            });
        }

        /// <summary>
        /// Comparator should return AddedOptionalProperty only if schema is referenced.
        /// Schema is referenced if one of its allOf property has a discriminator.
        /// </summary>
        [Test]
        public void CompareOAS_ShouldReturn_AddedOptionalPropertyDifferences_When_ValidPolymorphicSchema()
        {
            var differences = CompareSpecifications("polymorphic_schema");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.AddedOptionalProperty,
                Severity = Severity.Warning,
                NewJsonRef = "#/schemas/Dog/properties/breed"
            });
        }

        /// <summary>
        /// ComparePaths method should also work for x-ms-paths extension
        /// </summary>
        [Test]
        public void CompareOAS_ShouldReturn_Differences_When_XMsPathsExtension()
        {
            var differences = CompareSpecifications("x-ms-paths");

            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedPath,
                Severity = Severity.Warning,
                OldJsonRef = "#/x-ms-paths/~1myPath~1query-drive?op=file"
            }, 1);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.RequiredStatusChange,
                Severity = Severity.Warning,
                OldJsonRef = "#/x-ms-paths/~1myPath~1query-drive?op=folder/get/parameters/0/required"
            }, 1);
            differences.AssertContains(new ExpectedDifference
            {
                Rule = ComparisonRules.TypeChanged,
                Severity = Severity.Warning,
                OldJsonRef = "#/schemas/Cat/properties/sleepTime/type"
            }, 1);
        }

        [Test]
        public void CompareOAS_ShouldReturn_AddedRequestBodyDifferences()
        {
            var differences = CompareSpecifications("added_request_body");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.AddedRequestBody,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1pets/post/requestBody"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedRequestBodyDifferences()
        {
            var differences = CompareSpecifications("removed_request_body");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedRequestBody,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1pets/post/requestBody"
            });
        }

        [Test]
        public void Compare_OAS_Should_Return_Removed_Schema_When_Schema_Is_Removed_In_Response()
        {
            var differences = CompareSpecifications("removed_schema_in_response");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedDefinition,
                Severity = Severity.Warning,
                OldJsonRef = "#/paths/~1api~1Parameters/put/responses/200/content/application~1json/schema"
            });
        }

        [Test]
        public void Compare_OAS_Should_Return_Added_Schema_When_Schema_Is_Added_In_Response()
        {
            var differences = CompareSpecifications("added_schema_in_response");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.AddedSchema,
                Severity = Severity.Error,
                NewJsonRef = "#/paths/~1api~1Parameters/put/responses/200/content/application~1json/schema"
            });
        }

        [Test]
        public void Compare_OAS_Should_Return_Nullable_Property_Changed_When_Nullable_Property_Is_Changed()
        {
            var differences = CompareSpecifications("nullable_property_changed");

            differences.AssertContainsOnly(new ExpectedDifference
            {
                Rule = ComparisonRules.NullablePropertyChanged,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1pets/get/responses/200/content/application~1json/schema/items/properties/tag/nullable"
            });
        }

        [Test]
        public void CompareOAS_ShouldReturn_RemovedRequiredPropertyDifferences()
        {
            var differences = CompareSpecifications("removed_required_property.json");

            var expectedDifference = new ExpectedDifference
            {
                Rule = ComparisonRules.RemovedRequiredProperty,
                Severity = Severity.Warning,
                NewJsonRef = "#/paths/~1pets/put/responses/200/content/text~1plain/schema"
            };
            differences.AssertContains(expectedDifference, 1);
            differences.AssertContainsOnly(expectedDifference);
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

            var oldFileName = Path.Combine(baseDirectory, "Resource", "old", oasName + ".json");
            var newFileName = Path.Combine(baseDirectory, "Resource", "new", oasName + ".yaml");

            var differences = OpenApiComparator.Compare(
                File.ReadAllText(oldFileName),
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
        public ComparisonRule Rule;

        public Severity? Severity;

        public string OldJsonRef;

        public string NewJsonRef;
    }
}
