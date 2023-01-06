using CommandLine;

namespace Criteo.OpenApi.Comparator.Cli
{
    /// <summary>
    /// Command line options
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Path to old OpenAPI Specification
        /// </summary>
        [Option('o', "old", Required = true, HelpText = "Path to old OpenAPI Specification.")]
        public string OldSpec { get; set; }
        
        /// <summary>
        /// Path to new OpenAPI Specification
        /// </summary>
        [Option('n', "new", Required = true, HelpText = "Path to new OpenAPI Specification.")]
        public string NewSpec { get; set; }

        /// <summary>
        /// Specifies in which format the differences should be displayed (default Json)
        /// </summary>
        [Option('f', "outputFormat", Required = false, HelpText = "Specifies in which format the differences should be displayed (default Json). Possible values: Json | Text.")]
        public OutputFormat OutputFormat { get; set; } = OutputFormat.Json;
    }

    /// <summary>
    /// Display format for detected differences
    /// </summary>
    public enum OutputFormat
    {
        /// <summary>
        /// Ideal for deserialization
        /// </summary>
        Json = 0,
        
        /// <summary>
        /// Ideal for human reading (<see cref="ComparisonMessage.ToString"/>)
        /// </summary>
        Text = 1,
        
    }
}