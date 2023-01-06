using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Criteo.OpenApi.Comparator.Cli
{
    /// <summary>
    /// Entry point for OpenAPI Comparator command line tool
    /// </summary>
    public static class Program
    {
        /// <param name="args">
        /// Must contain --old|-o and --new|-n parameters which are paths to old and new OpenAPI specification
        /// </param>
        public static int Main(string[] args)
        {
            var parserResult = CommandLine.Parser.Default.ParseArguments<Options>(args);

            if (parserResult.Errors.Any())
            {
                return 1;
            }
            
            var options = parserResult.Value;

            var oldFileFound = TryReadFile(options.OldSpec, out var oldOpenApiSpecification);
            var newFileFound = TryReadFile(options.NewSpec, out var newOpenApiSpecification);

            if (!oldFileFound || !newFileFound)
            {
                Console.WriteLine("Exiting.");
                return 1;
            }
            
            var differences = OpenApiComparator.Compare(oldOpenApiSpecification, newOpenApiSpecification);

            DisplayOutput(differences, options.OutputFormat);
            
            return 0;
        }

        private static bool TryReadFile(string path, out string fileContent)
        {
            try
            {
                fileContent = File.ReadAllText(path);
                return true;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"File not found for: {path}.");
                fileContent = null;
                return false;
            }
        }

        private static void DisplayOutput(IEnumerable<ComparisonMessage> differences, OutputFormat outputFormat)
        {
            if (outputFormat == OutputFormat.Json)
            {
                Console.WriteLine(JsonSerializer.Serialize(differences, new JsonSerializerOptions { WriteIndented = true }));
                return;
            }

            foreach (var change in differences)
            {
                if (outputFormat == OutputFormat.Text)
                {
                    Console.WriteLine(change);
                }
            }
        }
    }
}
