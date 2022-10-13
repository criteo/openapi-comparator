namespace Criteo.OpenApi.Comparator
{
    /// <summary>
    /// Links a difference to its related comparison rule
    /// </summary>
    public class ComparisonRule
    {
        public int Id { get; set; }

        public string Code { get; set; }

        /// <summary>
        /// Rule message, giving more details about the difference
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Difference type (Addition, Update, Removal)
        /// </summary>
        public MessageType Type { get; set; }
    }
}
