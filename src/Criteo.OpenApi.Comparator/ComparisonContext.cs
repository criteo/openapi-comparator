// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Criteo.OpenApi.Comparator.Parser;
using Criteo.OpenApi.Comparator.Logging;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator
{
    /// <summary>
    /// Provides context for a comparison, such as the ancestors in the validation tree, the root object
    /// and information about the key or index that locate this object in the parent's list or dictionary
    /// </summary>
    internal class ComparisonContext
    {
        private readonly JsonDocument<OpenApiDocument> _newOpenApiDocument;
        private readonly JsonDocument<OpenApiDocument> _oldOpenApiDocument;

        /// <summary>
        /// Initializes a top level context for comparisons
        /// </summary>
        /// <param name="oldOpenApiDocument">an old document of type T.</param>
        /// <param name="newOpenApiDocument">a new document of type T</param>
        internal ComparisonContext(JsonDocument<OpenApiDocument> oldOpenApiDocument, JsonDocument<OpenApiDocument> newOpenApiDocument)
        {
            _oldOpenApiDocument = oldOpenApiDocument;
            _newOpenApiDocument = newOpenApiDocument;
        }

        /// <summary>
        /// The original root object in the graph that is being compared
        /// </summary>
        internal OpenApiDocument OldOpenApiDocument => _oldOpenApiDocument.Typed;

        /// Old swagger
        internal OpenApiDocument NewOpenApiDocument => _newOpenApiDocument.Typed;

        /// If true, then breaking changes are errors instead of warnings. If null base this on version differences
        internal bool? Strict { get; set; }

        /// Request, Response, Both or None
        private readonly DisposableDataDirection _direction = new();

        internal DataDirection Direction { get => _direction.Direction; set => _direction.Direction = value; }

        internal IDisposable WithDirection(DataDirection direction)
        {
            _direction.Direction = direction;
            return _direction;
        }

        private ObjectPath Path => _path.Peek();

        internal void PushProperty(string property) => _path.Push(Path.AppendProperty(property));

        internal void PushParameterByName(string name) => _path.Push(Path.AppendParameterByName(name));

        internal void PushServerByUrl(string url) => _path.Push(Path.AppendServerByUrl(url));

        internal void PushPathProperty(string name, bool asProperty = false) => _path.Push(asProperty
            ? Path.AppendProperty(name)
            : Path.AppendPathProperty(name));

        internal void Pop() => _path.Pop();

        private readonly Stack<ObjectPath> _path = new Stack<ObjectPath>(new[] { ObjectPath.Empty });

        internal void Log(ComparisonRule rule, params object[] formatArguments) =>
            _messages.Add(new ComparisonMessage(
                rule,
                Path,
                _oldOpenApiDocument,
                _newOpenApiDocument,
                Convert(rule.Severity),
                formatArguments
            ));

        private Severity Convert(MessageSeverity ruleLogType)
        {
            return ruleLogType switch
            {
                MessageSeverity.Info => Severity.Info,
                MessageSeverity.Warning => Severity.Warning,
                MessageSeverity.Breaking => Strict ? Severity.Error : Severity.Warning,
                _ => Severity.Error
            };
        }

        internal void LogError(ComparisonRule rule, params object[] formatArguments) =>
            _messages.Add(new ComparisonMessage(
                rule,
                Path,
                _oldOpenApiDocument,
                _newOpenApiDocument,
                Severity.Error,
                false,
                formatArguments
            ));

        /// <summary>
        /// Lists all the found differences
        /// </summary>
        internal IEnumerable<ComparisonMessage> Messages
        {
            get
            {
                // TODO: How to eliminate duplicate messages
                // Issue: https://github.com/Azure/openapi-diff/issues/48
                return _messages; //.Distinct(new CustomComparer());
            }
        }

        public bool HasVersionChanged { get; set; }

        private readonly IList<ComparisonMessage> _messages = new List<ComparisonMessage>();
    }

    internal class DisposableDataDirection : IDisposable
    {
        public DataDirection Direction { get; set; }

        public void Dispose() => Direction = DataDirection.None;
    }

    /// <summary>
    /// Specifies if the currently compared swagger element is attached to a request, a response, both or none
    /// </summary>
    [Flags]
    internal enum DataDirection
    {
        None = 0,
        Request = 1,
        Response = 2,
        Both = 3,
    }
}
