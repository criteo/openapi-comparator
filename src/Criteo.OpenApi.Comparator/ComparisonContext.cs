using System;
using System.Collections.Generic;
using Criteo.OpenApi.Comparator.Parser;
using Criteo.OpenApi.Comparator.Logging;

namespace Criteo.OpenApi.Comparator
{
    /// <summary>
    /// Provides context for a comparison, such as the ancestors in the validation tree, the root object
    /// and information about the key or index that locate this object in the parent's list or dictionary
    /// </summary>
    public class ComparisonContext<T>
    {
        private readonly JsonDocument<T> _newOpenApiDocument;
        private readonly JsonDocument<T> _oldOpenApiDocument;

        /// <summary>
        /// Initializes a top level context for comparisons
        /// </summary>
        /// <param name="oldOpenApiDocument">an old document of type T.</param>
        /// <param name="newOpenApiDocument">a new document of type T</param>
        /// <param name="settings">Comparison settings retrieved from command line</param>
        public ComparisonContext(JsonDocument<T> oldOpenApiDocument, JsonDocument<T> newOpenApiDocument, Settings.Settings settings = null)
        {
            _oldOpenApiDocument = oldOpenApiDocument;
            _newOpenApiDocument = newOpenApiDocument;

            if (settings != null)
                Strict = settings.Strict;
        }

        /// <summary>
        /// The original root object in the graph that is being compared
        /// </summary>
        public T OldOpenApiDocument => _oldOpenApiDocument.Typed;

        /// Old swagger
        public T NewOpenApiDocument => _newOpenApiDocument.Typed;

        /// <summary>
        /// If true, then checking should be strict, in other words, breaking changes are errors instead of warnings.
        /// </summary>
        public bool Strict { get; set; }

        /// Request, Response, Both or None
        public DataDirection Direction { get; set; } = DataDirection.None;

        /// <summary>
        /// Uri File { get; }
        /// </summary>
        private ObjectPath Path => _path.Peek();

        /// public void PushIndex(int index) => _path.Push(Path.AppendIndex(index));
        public void PushProperty(string property) => _path.Push(Path.AppendProperty(property));

        public void PushParameterByName(string name) => _path.Push(Path.AppendParameterByName(name));

        public void PushServerByUrl(string url) => _path.Push(Path.AppendServerByUrl(url));

        public void PushPathProperty(string name, bool asProperty = false) => _path.Push(asProperty
            ? Path.AppendProperty(name)
            : Path.AppendPathProperty(name));

        public void Pop() => _path.Pop();

        private Stack<ObjectPath> _path = new Stack<ObjectPath>(new[] { ObjectPath.Empty });

        /// <summary>
        /// Store new difference as an info
        /// </summary>
        /// <param name="template">Type/rule of the difference</param>
        /// <param name="formatArguments">Dynamic arguments providing dynamic details</param>
        public void LogInfo(MessageTemplate template, params object[] formatArguments) =>
            _messages.Add(new ComparisonMessage(
                template,
                Path,
                _oldOpenApiDocument,
                _newOpenApiDocument,
                Severity.Info,
                formatArguments
            ));

        /// <summary>
        /// Store new difference as an error
        /// </summary>
        /// <param name="template">Type/rule of the difference</param>
        /// <param name="formatArguments">Dynamic arguments providing dynamic details</param>
        public void LogError(MessageTemplate template, params object[] formatArguments) =>
            _messages.Add(new ComparisonMessage(
                template,
                Path,
                _oldOpenApiDocument,
                _newOpenApiDocument,
                Severity.Error,
                formatArguments
            ));

        /// <summary>
        /// Log a breaking change as Error if Strict option is set and as Warning if not
        /// </summary>
        /// <param name="template">Message's format</param>
        /// <param name="formatArguments">Dynamic arguments providing dynamic details</param>
        public void LogBreakingChange(MessageTemplate template, params object[] formatArguments) =>
            _messages.Add(new ComparisonMessage(
                template,
                Path,
                _oldOpenApiDocument,
                _newOpenApiDocument,
                Strict ? Severity.Error : Severity.Warning,
                formatArguments
            ));

        /// <summary>
        /// Lists all the found differences
        /// </summary>
        public IEnumerable<ComparisonMessage> Messages
        {
            get
            {
                // TODO: How to eliminate duplicate messages
                // Issue: https://github.com/Azure/openapi-diff/issues/48
                return _messages; //.Distinct(new CustomComparer());
            }
        }

        private readonly IList<ComparisonMessage> _messages = new List<ComparisonMessage>();
    }

    /// <summary>
    /// Specifies if the currently compared swagger element is attached to a request, a response, both or none
    /// </summary>
    [Flags]
    public enum DataDirection
    {
        None = 0,
        Request = 1,
        Response = 2,
        Both = 3,
    }
}
