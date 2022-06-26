namespace Criteo.OpenApiDiff.Comparator
{
    internal sealed class JsonComparisonMessage
    {
        public string Id;

        public string Code;

        public string Message;

        public JsonLocation Old;

        public JsonLocation New;

        public string Type;

        public string DocUrl;

        public string Mode;
    }
}
