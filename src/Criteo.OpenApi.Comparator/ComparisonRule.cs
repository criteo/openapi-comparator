namespace Criteo.OpenApi.Comparator
{
    /// <summary>
    /// Describes an atomic type of difference
    /// </summary>
    public class ComparisonRule
    {
        /// <summary> Unique identifier </summary>
        public int Id { get; set; }

        /// <summary> Verbose identifier of the rule </summary>
        public string Code { get; set; }

        /// <summary> Rule message, giving more details about the difference </summary>
        public string Message { get; set; }

        /// <summary> Difference type (Addition, Update, Removal) </summary>
        public MessageType Type { get; set; }
    }
}
