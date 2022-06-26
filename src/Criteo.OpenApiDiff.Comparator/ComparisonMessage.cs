// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Criteo.OpenApiDiff.Core;
using Criteo.OpenApiDiff.Core.Logging;

namespace Criteo.OpenApiDiff.Comparator
{
    /// <summary>
    /// Represents a single validation violation.
    /// </summary>
    public class ComparisonMessage
    {
        private const string _docBaseUrl = "https://github.com/Azure/openapi-diff/tree/master/docs/rules/";

        /// <param name="template">Links a difference to its related comparison rule</param>
        /// <param name="path">Path of the compared JSON element</param>
        /// <param name="oldDocument">Old JSON element</param>
        /// <param name="newDocument">New JSON element</param>
        /// <param name="severity">Severity of the difference (Info, Error, Warning)</param>
        /// <param name="formatArguments">List of arguments inserted in the string message as dynamic arguments</param>
        public ComparisonMessage(
            MessageTemplate template,
            ObjectPath path,
            IJsonDocument oldDocument,
            IJsonDocument newDocument,
            Category severity,
            params object[] formatArguments
        )
        {
            Severity = severity;
            Message = $"{string.Format(CultureInfo.CurrentCulture, template.Message, formatArguments)}";
            Path = path;
            OldDocument = oldDocument;
            NewDocument = newDocument;
            Id = template.Id;
            Code = template.Code;
            DocUrl = $"{_docBaseUrl}{template.Id}.md";
            Mode = template.Type;
        }

        private IJsonDocument OldDocument { get; }

        private IJsonDocument NewDocument { get; }

        /// Info, Error, Warning
        public Category Severity { get; }

        public string Message { get; }

        /// <summary>
        /// The JSON document path of the element being verified.
        /// </summary>
        private ObjectPath Path { get; }

        /// <returns>JSON Pointer of the old JSON reference</returns>
        /// JSON Pointer defines a string syntax for identifying a specific value
        /// within a JSON document
        public string OldJsonRef => Path.JsonPointer(OldDocument);

        /// <summary>
        /// A JToken from the old document that contains such information as location.
        /// If there are null nodes in the path, and the it's not an change of addtion, the null nodes should be removed, thus can return its parent token
        /// </summary>
        /// <seealso cref="IJsonLineInfo"/>
        public JToken OldJson() => Mode != MessageType.Addition
            ? Path.CompletePath(OldDocument.Token).LastOrDefault(t => t.token != null).token
            : Path.CompletePath(OldDocument.Token).LastOrDefault().token;

        /// <returns>JSON Pointer of the new JSON reference</returns>
        /// JSON Pointer defines a string syntax for identifying a specific value
        /// within a JSON document
        public string NewJsonRef => Path.JsonPointer(NewDocument);

        /// <summary>
        /// A JToken from the new document that contains such information as location.
        /// </summary>
        /// <seealso cref="IJsonLineInfo"/>
        public JToken NewJson() => Mode != MessageType.Removal
            ? Path.CompletePath(NewDocument.Token).LastOrDefault(t => t.token != null).token
            : Path.CompletePath(NewDocument.Token).LastOrDefault().token;


        /// <summary>
        /// The id of the validation message
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The code of the validation message
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Documentation Url for the Message
        /// </summary>
        private string DocUrl { get; }

        /// Addition, Update or Removal
        public MessageType Mode { get; }

        /// <summary>
        /// Return a location of the given JSON token `t` in the document `j`.
        /// </summary>
        /// <returns>a string in this format `fileName:lineNumber:linePosition`</returns>
        private static string Location(IJsonDocument jsonDocument, IJsonLineInfo jsonToken)
        {
            // up cast.
            return jsonToken == null
                ? null
                : $"{ObjectPath.FileNameNorm(jsonDocument.FileName)}:{jsonToken.LineNumber}:{jsonToken.LinePosition}";
        }

        /// <summary>
        /// Location of the old swagger file
        /// </summary>
        public string OldLocation() => Location(OldDocument, OldJson());

        /// <summary>
        /// Location of the new swagger file
        /// </summary>
        public string NewLocation() => Location(NewDocument, NewJson());

        /// <summary>
        /// Converts JSON object into a serialized JSON string
        /// </summary>
        /// <returns>JSON as string</returns>
        public string GetValidationMessagesAsJson()
        {
            var rawMessage = new JsonComparisonMessage
            {
                Id = Id.ToString(),
                Code = Code,
                Message = Message,
                Type = Severity.ToString(),
                DocUrl = DocUrl,
                Mode = Mode.ToString(),
                Old = new JsonLocation { Ref = OldJsonRef, Path = OldJson()?.Path, Location = OldLocation(), },
                New = new JsonLocation { Ref = NewJsonRef, Path = NewJson()?.Path, Location = NewLocation(), }
            };

            return JsonConvert.SerializeObject(
                rawMessage,
                Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
            );
        }

        /// <summary>
        /// Converts the currents ComparisonMessage object into a string message
        /// </summary>
        public override string ToString() =>
            $"code = {Code}, type = {Severity}, message = {Message}, docurl = {DocUrl}, mode = {Mode}";
    }

    public class CustomComparer : IEqualityComparer<ComparisonMessage>
    {
        /// <summary>
        /// Compares two comparison messages
        /// </summary>
        public bool Equals(ComparisonMessage message1, ComparisonMessage message2) =>
            message1?.Message == message2?.Message;

        /// <param name="comparison">ComparisonMessage</param>
        /// <returns>The hash code of a ComparisonMessage object</returns>
        public int GetHashCode(ComparisonMessage comparison) => comparison.Message.GetHashCode();
    }
}
