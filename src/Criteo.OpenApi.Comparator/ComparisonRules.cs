﻿// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

namespace Criteo.OpenApi.Comparator
{
    /// <summary>
    /// Contains all the comparison rules for the implemented rules
    /// </summary>
    public static class ComparisonRules
    {
        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1000.md
        /// </summary>
        public static ComparisonRule VersionsReversed = new ComparisonRule
        {
            Id = 1000,
            Code = nameof(VersionsReversed),
            Message = "The new version has a lower value than the old: {0} -> {1}",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1001.md
        /// </summary>
        public static ComparisonRule NoVersionChange = new ComparisonRule
        {
            Id = 1001,
            Code = nameof(NoVersionChange),
            Message = "The versions have not changed.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1002.md
        /// </summary>
        public static ComparisonRule ProtocolNoLongerSupported = new ComparisonRule
        {
            Id = 1002,
            Code = nameof(ProtocolNoLongerSupported),
            Message = "The new version does not support '{0}' as a protocol.",
            Type = MessageType.Removal
        };

        /// <summary>
        /// OpenApi Specification version 3 specific
        /// </summary>
        public static ComparisonRule ServerNoLongerSupported = new ComparisonRule
        {
            Id = 10021,
            Code = nameof(ServerNoLongerSupported),
            Message = "The new version does not support the server with url '{0}' anymore",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1003.md
        /// </summary>
        public static ComparisonRule RequestBodyFormatNoLongerSupported = new ComparisonRule
        {
            Id = 1003,
            Code = nameof(RequestBodyFormatNoLongerSupported),
            Message = "The new version does not support '{0}' as a request body format.",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Extension of rule 1003
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1003.md
        /// </summary>
        public static ComparisonRule ResponseBodyInOperationFormatNoLongerSupported = new ComparisonRule
        {
            Id = 10031,
            Code = nameof(ResponseBodyInOperationFormatNoLongerSupported),
            Message = "The new version of operation does not support '{0}' as a response body format.",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1004.md
        /// </summary>
        public static ComparisonRule ResponseBodyFormatNowSupported = new ComparisonRule
        {
            Id = 1004,
            Code = nameof(ResponseBodyFormatNowSupported),
            Message = "The old version did not support '{0}' as a response body format.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Extension of rule 1004
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1004.md
        /// </summary>
        public static ComparisonRule ResponseBodyInOperationFormatNowSupported = new ComparisonRule
        {
            Id = 10041,
            Code = nameof(ResponseBodyInOperationFormatNowSupported),
            Message = "The old version of operation did not support '{0}' as a response body format.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1005.md
        /// </summary>
        public static ComparisonRule RemovedPath = new ComparisonRule
        {
            Id = 1005,
            Code = nameof(RemovedPath),
            Message =
                "The new version is missing a path that was found in the old version. Was path '{0}' removed or restructured?",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1006.md
        /// </summary>
        public static ComparisonRule RemovedDefinition = new ComparisonRule
        {
            Id = 1006,
            Code = nameof(RemovedDefinition),
            Message =
                "The new version is missing a definition that was found in the old version. Was '{0}' removed or renamed?",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1007.md
        /// </summary>
        public static ComparisonRule RemovedClientParameter = new ComparisonRule
        {
            Id = 1007,
            Code = nameof(RemovedClientParameter),
            Message =
                "The new version is missing a client parameter that was found in the old version. Was '{0}' removed or renamed?",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1008.md
        /// </summary>
        public static ComparisonRule ModifiedOperationId = new ComparisonRule
        {
            Id = 1008,
            Code = nameof(ModifiedOperationId),
            Message = "The operation id has been changed from '{0}' to '{1}'. This will impact generated code.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1009.md
        /// </summary>
        public static ComparisonRule RemovedRequiredParameter = new ComparisonRule
        {
            Id = 1009,
            Code = nameof(RemovedRequiredParameter),
            Message = "The required parameter '{0}' was removed in the new version.",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1010.md
        /// </summary>
        public static ComparisonRule AddingRequiredParameter = new ComparisonRule
        {
            Id = 1010,
            Code = nameof(AddingRequiredParameter),
            Message = "The required parameter '{0}' was added in the new version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1011.md
        /// </summary>
        public static ComparisonRule AddingResponseCode = new ComparisonRule
        {
            Id = 1011,
            Code = nameof(AddingResponseCode),
            Message = "The new version adds a response code '{0}'.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1012.md
        /// </summary>
        public static ComparisonRule RemovedResponseCode = new ComparisonRule
        {
            Id = 1012,
            Code = nameof(RemovedResponseCode),
            Message = "The new version removes the response code '{0}'",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1013.md
        /// </summary>
        public static ComparisonRule AddingHeader = new ComparisonRule
        {
            Id = 1013,
            Code = nameof(AddingHeader),
            Message = "The new version adds a required header '{0}'.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1014.md
        /// </summary>
        public static ComparisonRule RemovingHeader = new ComparisonRule
        {
            Id = 1014,
            Code = nameof(RemovingHeader),
            Message = "The new version removes a required header '{0}'.",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1015.md
        /// </summary>
        public static ComparisonRule ParameterInHasChanged = new ComparisonRule
        {
            Id = 1015,
            Code = nameof(ParameterInHasChanged),
            Message = "How the parameter is passed has changed -- it used to be '{0}', now it is '{1}'.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1016.md
        /// </summary>
        public static ComparisonRule ConstantStatusHasChanged = new ComparisonRule
        {
            Id = 1016,
            Code = nameof(ConstantStatusHasChanged),
            Message = "The 'constant' status changed from the old version to the new.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1017.md
        /// </summary>
        public static ComparisonRule ReferenceRedirection = new ComparisonRule
        {
            Id = 1017,
            Code = nameof(ReferenceRedirection),
            Message = "The '$ref' property points to different models in the old and new versions.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1018.md
        /// </summary>
        public static ComparisonRule RequestBodyFormatNowSupported = new ComparisonRule
        {
            Id = 1018,
            Code = nameof(RequestBodyFormatNowSupported),
            Message = "The old version did not support '{0}' as a request body format.",
            Type = MessageType.Addition,
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1019.md
        /// </summary>
        public static ComparisonRule RemovedEnumValue = new ComparisonRule
        {
            Id = 1019,
            Code = nameof(RemovedEnumValue),
            Message = "The new version is removing enum value(s) '{0}' from the old version.",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1000.md
        /// </summary>
        public static ComparisonRule AddedEnumValue = new ComparisonRule
        {
            Id = 1020,
            Code = nameof(AddedEnumValue),
            Message = "The new version is adding enum value(s) '{0}' from the old version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1021.md
        /// </summary>
        public static ComparisonRule AddedAdditionalProperties = new ComparisonRule
        {
            Id = 1021,
            Code = nameof(AddedAdditionalProperties),
            Message = "The new version adds an 'additionalProperties' element.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1022.md
        /// </summary>
        public static ComparisonRule RemovedAdditionalProperties = new ComparisonRule
        {
            Id = 1022,
            Code = nameof(RemovedAdditionalProperties),
            Message = "The new version removes the 'additionalProperties' element.",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1023.md
        /// </summary>
        public static ComparisonRule TypeFormatChanged = new ComparisonRule
        {
            Id = 1023,
            Code = nameof(TypeFormatChanged),
            Message = "The new version has a different format than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1024.md
        /// </summary>
        public static ComparisonRule ConstraintIsStronger = new ComparisonRule
        {
            Id = 1024,
            Code = nameof(ConstraintIsStronger),
            Message = "The new version has a more constraining '{0}' value than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1025.md
        /// </summary>
        public static ComparisonRule RequiredStatusChange = new ComparisonRule
        {
            Id = 1025,
            Code = nameof(RequiredStatusChange),
            Message = "The 'required' status changed from the old version('{0}') to the new version('{1}').",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1026.md
        /// </summary>
        public static ComparisonRule TypeChanged = new ComparisonRule
        {
            Id = 1026,
            Code = nameof(TypeChanged),
            Message = "The new version has a different type '{0}' than the previous one '{1}'.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1027.md
        /// </summary>
        public static ComparisonRule DefaultValueChanged = new ComparisonRule
        {
            Id = 1027,
            Code = nameof(DefaultValueChanged),
            Message = "The new version has a different default value than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1028.md
        /// </summary>
        public static ComparisonRule ArrayCollectionFormatChanged = new ComparisonRule
        {
            Id = 1028,
            Code = nameof(ArrayCollectionFormatChanged),
            Message = "The new version has a different array collection format than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// OpenApi Specification version 3 specific
        /// </summary>
        public static ComparisonRule ParameterStyleChanged = new ComparisonRule
        {
            Id = 10281,
            Code = nameof(ParameterStyleChanged),
            Message = "Parameter '{0}' has a different style value in the new version.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1029.md
        /// </summary>
        public static ComparisonRule ReadonlyPropertyChanged = new ComparisonRule
        {
            Id = 1029,
            Code = nameof(ReadonlyPropertyChanged),
            Message = "The read only property has changed from '{0}' to '{1}'.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1031.md
        /// </summary>
        public static ComparisonRule DifferentDiscriminator = new ComparisonRule
        {
            Id = 1030,
            Code = nameof(DifferentDiscriminator),
            Message = "The new version has a different discriminator than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1031.md
        /// </summary>
        public static ComparisonRule DifferentExtends = new ComparisonRule
        {
            Id = 1031,
            Code = nameof(DifferentExtends),
            Message = "The new version has a different 'extends' property than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1032.md
        /// </summary>
        public static ComparisonRule DifferentAllOf = new ComparisonRule
        {
            Id = 1032,
            Code = nameof(DifferentAllOf),
            Message = "The new version has a different 'allOf' property than the previous one.",
            Type = MessageType.Update
        };


        /// <summary>
        /// OpenApi Specification version 3 specific
        /// </summary>
        public static ComparisonRule DifferentOneOf = new ComparisonRule
        {
            Id = 10321,
            Code = nameof(DifferentOneOf),
            Message = "The new version has a different 'oneOf' property than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1033.md
        /// </summary>
        public static ComparisonRule RemovedProperty = new ComparisonRule
        {
            Id = 1033,
            Code = nameof(RemovedProperty),
            Message =
                "The new version is missing a property found in the old version. Was '{0}' renamed or removed?",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1034.md
        /// </summary>
        public static ComparisonRule AddedRequiredProperty = new ComparisonRule
        {
            Id = 1034,
            Code = nameof(AddedRequiredProperty),
            Message = "The new version has new required property '{0}' that was not found in the old version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1035.md
        /// </summary>
        public static ComparisonRule RemovedOperation = new ComparisonRule
        {
            Id = 1035,
            Code = nameof(RemovedOperation),
            Message =
                "The new version is missing an operation that was found in the old version. Was operationId '{0}' removed or restructured?",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1036.md
        /// </summary>
        public static ComparisonRule ConstraintChanged = new ComparisonRule
        {
            Id = 1036,
            Code = nameof(ConstraintChanged),
            Message = "The new version has a different '{0}' value than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1037.md
        /// </summary>
        public static ComparisonRule ConstraintIsWeaker = new ComparisonRule
        {
            Id = 1037,
            Code = nameof(ConstraintIsWeaker),
            Message = "The new version has a less constraining '{0}' value than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1038.md
        /// </summary>
        public static ComparisonRule AddedPath = new ComparisonRule
        {
            Id = 1038,
            Code = nameof(AddedPath),
            Message = "The new version is adding a path that was not found in the old version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1039.md
        /// </summary>
        public static ComparisonRule AddedOperation = new ComparisonRule
        {
            Id = 1039,
            Code = nameof(AddedOperation),
            Message = "The new version is adding an operation that was not found in the old version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1040.md
        /// </summary>
        public static ComparisonRule AddedReadOnlyPropertyInResponse = new ComparisonRule
        {
            Id = 1040,
            Code = nameof(AddedReadOnlyPropertyInResponse),
            Message =
                "The new version has a new read-only property '{0}' in response that was not found in the old version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1041.md
        /// </summary>
        public static ComparisonRule AddedPropertyInResponse = new ComparisonRule
        {
            Id = 1041,
            Code = nameof(AddedPropertyInResponse),
            Message = "The new version has a new property '{0}' in response that was not found in the old version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1042.md
        /// </summary>
        public static ComparisonRule ChangedParameterOrder = new ComparisonRule
        {
            Id = 1042,
            Code = nameof(ChangedParameterOrder),
            Message = "The order of parameter '{0}' was changed. ",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1043.md
        /// </summary>
        public static ComparisonRule AddingOptionalParameter = new ComparisonRule
        {
            Id = 1043,
            Code = nameof(AddingOptionalParameter),
            Message = "The optional parameter '{0}' was added in the new version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1044.md
        /// </summary>
        public static ComparisonRule LongRunningOperationExtensionChanged = new ComparisonRule
        {
            Id = 1044,
            Code = nameof(LongRunningOperationExtensionChanged),
            Message = "The new version has a different 'x-ms-long-running-operation' value than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1045.md
        /// </summary>
        public static ComparisonRule AddedOptionalProperty = new ComparisonRule
        {
            Id = 1045,
            Code = nameof(AddedOptionalProperty),
            Message = "The new version has a new optional property '{0}' that was not found in the old version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1046.md
        /// </summary>
        public static ComparisonRule AddedRequestBody = new ComparisonRule
        {
            Id = 1046,
            Code = nameof(AddedRequestBody),
            Message = "The new version is adding a requestBody that was not found in the old version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1047.md
        /// </summary>
        public static ComparisonRule RemovedRequestBody = new ComparisonRule
        {
            Id = 1047,
            Code = nameof(RemovedRequestBody),
            Message = "The new version is removing a requestBody that was found in the old version.",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1048.md
        /// </summary>
        public static ComparisonRule AddedSchema = new ComparisonRule
        {
            Id = 1048,
            Code = nameof(AddedSchema),
            Message = "The new version is adding a new schema that was not found in the old version.",
            Type = MessageType.Removal
        };

        /// <summary>
        /// OpenApi Specification version 3 specific
        /// </summary>
        public static ComparisonRule NullablePropertyChanged = new ComparisonRule()
        {
            Id = 2000,
            Code = nameof(NullablePropertyChanged),
            Message = "The nullable property has changed from '{0}' to '{1}'.",
            Type = MessageType.Update
        };
    }
}
