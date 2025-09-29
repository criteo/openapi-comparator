// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Linq;
using Criteo.OpenApi.Comparator.Parser;
using Criteo.OpenApi.Comparator.Logging;
using Microsoft.OpenApi.Models;
using System.Data;

namespace Criteo.OpenApi.Comparator
{
    /// <summary>
    /// Represents a single validation violation.
    /// </summary>
    public class ComparisonMessage
    {
        private const string DocBaseUrl = "https://github.com/criteo/openapi-comparator/tree/main/documentation/rules/";

        internal ComparisonMessage(
            ComparisonRule rule,
            ObjectPath path,
            IJsonDocument oldDocument,
            IJsonDocument newDocument,
            params object[] formatArguments
        )
        {
            Severity = rule.Severity;
            Message = $"{string.Format(CultureInfo.CurrentCulture, rule.Message, formatArguments)}";
            Path = path;
            OldDocument = oldDocument;
            NewDocument = newDocument;
            Id = rule.Id;
            Code = rule.Code;
            DocUrl = $"{DocBaseUrl}{rule.Id}.md";
            Mode = rule.Type;
        }

        internal ComparisonMessage(OpenApiError item)
        {
            var rule = ComparisonRules.OpenApiError;
            Severity = rule.Severity;
            Message = item.Message + " " + item.Pointer;
            Id = rule.Id;
            Code = rule.Code;
            DocUrl = $"{DocBaseUrl}{rule.Id}.md";
            Mode = rule.Type;
        }

        private IJsonDocument OldDocument { get; }

        private IJsonDocument NewDocument { get; }

        /// Info, Error, Warning
        public MessageSeverity Severity { get; set; }

        /// <summary>
        /// Explicit description of the change.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The JSON document path of the element being verified.
        /// </summary>
        private ObjectPath Path { get; }

        /// <returns>JSON Pointer of the old JSON reference</returns>
        /// JSON Pointer defines a string syntax for identifying a specific value
        /// within a JSON document
        public string OldJsonRef => Path?.JsonPointer(OldDocument);

        /// <summary>
        /// A JToken from the old document that contains such information as location.
        /// If there are null nodes in the path, and the it's not an change of addition, the null nodes should be removed, thus can return its parent token
        /// </summary>
        /// <seealso cref="IJsonLineInfo"/>
        public JToken OldJson() => Mode != MessageType.Addition
            ? Path.CompletePath(OldDocument.Token).LastOrDefault(t => t.token != null).token
            : Path.CompletePath(OldDocument.Token).LastOrDefault().token;

        /// <returns>JSON Pointer of the new JSON reference</returns>
        /// JSON Pointer defines a string syntax for identifying a specific value
        /// within a JSON document
        public string NewJsonRef => Path?.JsonPointer(NewDocument);

        /// <summary>
        /// A JToken from the new document that contains such information as location.
        /// </summary>
        /// <seealso cref="IJsonLineInfo"/>
        public JToken NewJson() => Mode != MessageType.Removal
            ? Path.CompletePath(NewDocument.Token).LastOrDefault(t => t.token != null).token
            : Path.CompletePath(NewDocument.Token).LastOrDefault().token;

        /// <summary>
        /// JSON Pointer of the old resolved JSON reference
        /// </summary>
        public string OldJsonPath => Path?.JsonPointer(OldDocument, resolveReferences: true);

        /// <summary>
        /// JSON Pointer of the new resolved JSON reference
        /// </summary>
        public string NewJsonPath => Path?.JsonPointer(NewDocument, resolveReferences: true);

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
        private static string Location(IJsonLineInfo jsonToken)
        {
            // up cast.
            return jsonToken == null
                ? null
                : $"{jsonToken.LineNumber}:{jsonToken.LinePosition}";
        }

        /// <summary>
        /// Location of the old swagger file
        /// </summary>
        public string OldLocation() => Location(OldJson());

        /// <summary>
        /// Location of the new swagger file
        /// </summary>
        public string NewLocation() => Location(NewJson());

        /// <summary>
        /// Converts the currents ComparisonMessage object into a string message
        /// </summary>
        public override string ToString() =>
            $"code = {Code},\n" +
            $"type = {Severity},\n" +
            $"message = {Message},\n" +
            $"docurl = {DocUrl},\n" +
            $"mode = {Mode}";
    }
}
