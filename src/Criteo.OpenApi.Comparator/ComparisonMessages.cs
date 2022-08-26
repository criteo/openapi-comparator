namespace Criteo.OpenApi.Comparator
{
    /// <summary>
    /// Contains all the message templates for the implemented rules
    /// </summary>
    public static class ComparisonMessages
    {
        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1000.md
        /// </summary>
        public static MessageTemplate VersionsReversed = new MessageTemplate
        {
            Id = 1000,
            Code = nameof(VersionsReversed),
            Message = "The new version has a lower value than the old: {0} -> {1}",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1001.md
        /// </summary>
        public static MessageTemplate NoVersionChange = new MessageTemplate
        {
            Id = 1001,
            Code = nameof(NoVersionChange),
            Message = "The versions have not changed.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1002.md
        /// </summary>
        public static MessageTemplate ProtocolNoLongerSupported = new MessageTemplate
        {
            Id = 1002,
            Code = nameof(ProtocolNoLongerSupported),
            Message = "The new version does not support '{0}' as a protocol.",
            Type = MessageType.Removal
        };

        /// <summary>
        /// OpenApi Specification version 3 specific
        /// </summary>
        public static MessageTemplate ServerNoLongerSupported = new MessageTemplate
        {
            Id = 10021,
            Code = nameof(ServerNoLongerSupported),
            Message = "The new version does not support the server with url '{0}' anymore",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1003.md
        /// </summary>
        public static MessageTemplate RequestBodyFormatNoLongerSupported = new MessageTemplate
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
        public static MessageTemplate ResponseBodyInOperationFormatNoLongerSupported = new MessageTemplate
        {
            Id = 10031,
            Code = nameof(ResponseBodyInOperationFormatNoLongerSupported),
            Message = "The new version of operation does not support '{0}' as a response body format.",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1004.md
        /// </summary>
        public static MessageTemplate ResponseBodyFormatNowSupported = new MessageTemplate
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
        public static MessageTemplate ResponseBodyInOperationFormatNowSupported = new MessageTemplate
        {
            Id = 10041,
            Code = nameof(ResponseBodyInOperationFormatNowSupported),
            Message = "The old version of operation did not support '{0}' as a response body format.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1005.md
        /// </summary>
        public static MessageTemplate RemovedPath = new MessageTemplate
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
        public static MessageTemplate RemovedDefinition = new MessageTemplate
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
        public static MessageTemplate RemovedClientParameter = new MessageTemplate
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
        public static MessageTemplate ModifiedOperationId = new MessageTemplate
        {
            Id = 1008,
            Code = nameof(ModifiedOperationId),
            Message = "The operation id has been changed from '{0}' to '{1}'. This will impact generated code.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1009.md
        /// </summary>
        public static MessageTemplate RemovedRequiredParameter = new MessageTemplate
        {
            Id = 1009,
            Code = nameof(RemovedRequiredParameter),
            Message = "The required parameter '{0}' was removed in the new version.",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1010.md
        /// </summary>
        public static MessageTemplate AddingRequiredParameter = new MessageTemplate
        {
            Id = 1010,
            Code = nameof(AddingRequiredParameter),
            Message = "The required parameter '{0}' was added in the new version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1011.md
        /// </summary>
        public static MessageTemplate AddingResponseCode = new MessageTemplate
        {
            Id = 1011,
            Code = nameof(AddingResponseCode),
            Message = "The new version adds a response code '{0}'.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1012.md
        /// </summary>
        public static MessageTemplate RemovedResponseCode = new MessageTemplate
        {
            Id = 1012,
            Code = nameof(RemovedResponseCode),
            Message = "The new version removes the response code '{0}'",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1013.md
        /// </summary>
        public static MessageTemplate AddingHeader = new MessageTemplate
        {
            Id = 1013,
            Code = nameof(AddingHeader),
            Message = "The new version adds a required header '{0}'.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1014.md
        /// </summary>
        public static MessageTemplate RemovingHeader = new MessageTemplate
        {
            Id = 1014,
            Code = nameof(RemovingHeader),
            Message = "The new version removes a required header '{0}'.",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1015.md
        /// </summary>
        public static MessageTemplate ParameterInHasChanged = new MessageTemplate
        {
            Id = 1015,
            Code = nameof(ParameterInHasChanged),
            Message = "How the parameter is passed has changed -- it used to be '{0}', now it is '{1}'.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1016.md
        /// </summary>
        public static MessageTemplate ConstantStatusHasChanged = new MessageTemplate
        {
            Id = 1016,
            Code = nameof(ConstantStatusHasChanged),
            Message = "The 'constant' status changed from the old version to the new.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1017.md
        /// </summary>
        public static MessageTemplate ReferenceRedirection = new MessageTemplate
        {
            Id = 1017,
            Code = nameof(ReferenceRedirection),
            Message = "The '$ref' property points to different models in the old and new versions.",
            Type = MessageType.Update
        };

        public static MessageTemplate RequestBodyFormatNowSupported = new MessageTemplate
        {
            Id = 1018,
            Code = nameof(RequestBodyFormatNowSupported),
            Message = "The old version did not support '{0}' as a request body format.",
            Type = MessageType.Addition,
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1019.md
        /// </summary>
        public static MessageTemplate RemovedEnumValue = new MessageTemplate
        {
            Id = 1019,
            Code = nameof(RemovedEnumValue),
            Message = "The new version is removing enum value(s) '{0}' from the old version.",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1000.md
        /// </summary>
        public static MessageTemplate AddedEnumValue = new MessageTemplate
        {
            Id = 1020,
            Code = nameof(AddedEnumValue),
            Message = "The new version is adding enum value(s) '{0}' from the old version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1021.md
        /// </summary>
        public static MessageTemplate AddedAdditionalProperties = new MessageTemplate
        {
            Id = 1021,
            Code = nameof(AddedAdditionalProperties),
            Message = "The new version adds an 'additionalProperties' element.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1022.md
        /// </summary>
        public static MessageTemplate RemovedAdditionalProperties = new MessageTemplate
        {
            Id = 1022,
            Code = nameof(RemovedAdditionalProperties),
            Message = "The new version removes the 'additionalProperties' element.",
            Type = MessageType.Removal
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1023.md
        /// </summary>
        public static MessageTemplate TypeFormatChanged = new MessageTemplate
        {
            Id = 1023,
            Code = nameof(TypeFormatChanged),
            Message = "The new version has a different format than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1024.md
        /// </summary>
        public static MessageTemplate ConstraintIsStronger = new MessageTemplate
        {
            Id = 1024,
            Code = nameof(ConstraintIsStronger),
            Message = "The new version has a more constraining '{0}' value than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1025.md
        /// </summary>
        public static MessageTemplate RequiredStatusChange = new MessageTemplate
        {
            Id = 1025,
            Code = nameof(RequiredStatusChange),
            Message = "The 'required' status changed from the old version('{0}') to the new version('{1}').",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1026.md
        /// </summary>
        public static MessageTemplate TypeChanged = new MessageTemplate
        {
            Id = 1026,
            Code = nameof(TypeChanged),
            Message = "The new version has a different type '{0}' than the previous one '{1}'.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1027.md
        /// </summary>
        public static MessageTemplate DefaultValueChanged = new MessageTemplate
        {
            Id = 1027,
            Code = nameof(DefaultValueChanged),
            Message = "The new version has a different default value than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1028.md
        /// </summary>
        public static MessageTemplate ArrayCollectionFormatChanged = new MessageTemplate
        {
            Id = 1028,
            Code = nameof(ArrayCollectionFormatChanged),
            Message = "The new version has a different array collection format than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// OpenApi Specification version 3 specific
        /// </summary>
        public static MessageTemplate ParameterStyleChanged = new MessageTemplate
        {
            Id = 10281,
            Code = nameof(ParameterStyleChanged),
            Message = "Parameter '{0}' has a different style value in the new version.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1029.md
        /// </summary>
        public static MessageTemplate ReadonlyPropertyChanged = new MessageTemplate
        {
            Id = 1029,
            Code = nameof(ReadonlyPropertyChanged),
            Message = "The read only property has changed from '{0}' to '{1}'.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1031.md
        /// </summary>
        public static MessageTemplate DifferentDiscriminator = new MessageTemplate
        {
            Id = 1030,
            Code = nameof(DifferentDiscriminator),
            Message = "The new version has a different discriminator than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1031.md
        /// </summary>
        public static MessageTemplate DifferentExtends = new MessageTemplate
        {
            Id = 1031,
            Code = nameof(DifferentExtends),
            Message = "The new version has a different 'extends' property than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1032.md
        /// </summary>
        public static MessageTemplate DifferentAllOf = new MessageTemplate
        {
            Id = 1032,
            Code = nameof(DifferentAllOf),
            Message = "The new version has a different 'allOf' property than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1033.md
        /// </summary>
        public static MessageTemplate RemovedProperty = new MessageTemplate
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
        public static MessageTemplate AddedRequiredProperty = new MessageTemplate
        {
            Id = 1034,
            Code = nameof(AddedRequiredProperty),
            Message = "The new version has new required property '{0}' that was not found in the old version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1035.md
        /// </summary>
        public static MessageTemplate RemovedOperation = new MessageTemplate
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
        public static MessageTemplate ConstraintChanged = new MessageTemplate
        {
            Id = 1036,
            Code = nameof(ConstraintChanged),
            Message = "The new version has a different '{0}' value than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1037.md
        /// </summary>
        public static MessageTemplate ConstraintIsWeaker = new MessageTemplate
        {
            Id = 1037,
            Code = nameof(ConstraintIsWeaker),
            Message = "The new version has a less constraining '{0}' value than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1038.md
        /// </summary>
        public static MessageTemplate AddedPath = new MessageTemplate
        {
            Id = 1038,
            Code = nameof(AddedPath),
            Message = "The new version is adding a path that was not found in the old version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1039.md
        /// </summary>
        public static MessageTemplate AddedOperation = new MessageTemplate
        {
            Id = 1039,
            Code = nameof(AddedOperation),
            Message = "The new version is adding an operation that was not found in the old version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1040.md
        /// </summary>
        public static MessageTemplate AddedReadOnlyPropertyInResponse = new MessageTemplate
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
        public static MessageTemplate AddedPropertyInResponse = new MessageTemplate
        {
            Id = 1041,
            Code = nameof(AddedPropertyInResponse),
            Message = "The new version has a new property '{0}' in response that was not found in the old version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1042.md
        /// </summary>
        public static MessageTemplate ChangedParameterOrder = new MessageTemplate
        {
            Id = 1042,
            Code = nameof(ChangedParameterOrder),
            Message = "The order of parameter '{0}' was changed. ",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1043.md
        /// </summary>
        public static MessageTemplate AddingOptionalParameter = new MessageTemplate
        {
            Id = 1043,
            Code = nameof(AddingOptionalParameter),
            Message = "The optional parameter '{0}' was added in the new version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1044.md
        /// </summary>
        public static MessageTemplate LongRunningOperationExtensionChanged = new MessageTemplate
        {
            Id = 1044,
            Code = nameof(LongRunningOperationExtensionChanged),
            Message = "The new version has a different 'x-ms-long-running-operation' value than the previous one.",
            Type = MessageType.Update
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1045.md
        /// </summary>
        public static MessageTemplate AddedOptionalProperty = new MessageTemplate
        {
            Id = 1045,
            Code = nameof(AddedOptionalProperty),
            Message = "The new version has a new optional property '{0}' that was not found in the old version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1046.md
        /// </summary>
        public static MessageTemplate AddedRequestBody = new MessageTemplate
        {
            Id = 1046,
            Code = nameof(AddedRequestBody),
            Message = "The new version is adding a requestBody that was not found in the old version.",
            Type = MessageType.Addition
        };

        /// <summary>
        /// Rule documentation: https://github.com/Azure/openapi-diff/blob/master/docs/rules/1047.md
        /// </summary>
        public static MessageTemplate RemovedRequestBody = new MessageTemplate
        {
            Id = 1047,
            Code = nameof(RemovedRequestBody),
            Message = "The new version is removing a requestBody that was found in the old version.",
            Type = MessageType.Removal
        };
    }
}
