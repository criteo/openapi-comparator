// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                Console.Error.WriteLine("Exiting.");
                return 1;
            }

            var differences = OpenApiComparator.Compare(
                oldOpenApiSpecification, newOpenApiSpecification, out var parsingErrors, options.StrictMode);

            if (parsingErrors.Any())
            {
                Console.Error.WriteLine("Errors occurred while parsing the OpenAPI specifications:");
                foreach (var error in parsingErrors)
                {
                    Console.Error.WriteLine(error);
                }
            }

            DisplayOutput(differences, options.OutputFormat);

            return 0;
        }

        private static bool TryReadFile(string path, out string fileContent)
        {
            try
            {
                // In case `path` is detected as an absolute or remote URI, `System.Uri` will ignore the `cwdUri`.
                var cwdUri = new Uri(Directory.GetCurrentDirectory());
                var uri = new Uri(cwdUri, path);
                if (uri.IsFile)
                {
                    return TryReadLocalFile(path, out fileContent);
                }
                else
                {
                    return TryReadRemoteFile(path, out fileContent);
                }
            }
            catch (UriFormatException)
            {
                Console.Error.WriteLine($"Failed to interpret the provided URI: {path}.");
                fileContent = null;
            }
            return false;
        }

        private static bool TryReadLocalFile(string path, out string fileContent)
        {
            try
            {
                fileContent = File.ReadAllText(path);
                return true;
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine($"File not found for: {path}.");
                fileContent = null;
                return false;
            }
        }

        private static bool TryReadRemoteFile(string path, out string fileContent)
        {
            try
            {
                using var client = new HttpClient();
                fileContent = client.GetStringAsync(path).Result;
                return true;
            }
            catch (HttpRequestException exception)
            {
                Console.Error.WriteLine($"Http request failed on {path}: {exception.Message}");
            }
            catch (AggregateException exception)
            {
                var stringWriter = new StringWriter();
                stringWriter.WriteLine($"Exception caught while waiting for the Http request result from {path}:");
                exception.Handle((innerException) =>
                {
                    stringWriter.Write($"- {innerException.GetType()}: {exception.Message}");
                    return true;
                });
                Console.Error.WriteLine(stringWriter.ToString());
            }
            fileContent = null;
            return false;
        }

        private static void DisplayOutput(IEnumerable<ComparisonMessage> differences, OutputFormat outputFormat)
        {
            if (outputFormat == OutputFormat.Json)
            {
                var serializerOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() },
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                };
                Console.WriteLine(JsonSerializer.Serialize(differences, serializerOptions));
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
