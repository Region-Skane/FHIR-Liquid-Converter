// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
//
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
//
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool
{
    internal static class ConverterLogicHandler
    {
        private const string MetadataFileName = "metadata.json";
        private static readonly List<string> CcdaExtensions = new List<string> { ".ccda", ".xml" };
        private static readonly ProcessorSettings DefaultProcessorSettings = new ProcessorSettings();

        internal static void Convert(ConverterOptions options)
        {
            if (!IsValidOptions(options))
            {
                throw new InputParameterException("Invalid command-line options.");
            }

            var dataType = GetDataTypes(options.TemplateDirectory);
            var dataProcessor = CreateDataProcessor(dataType);
            var templateProvider = CreateTemplateProvider(dataType, options.TemplateDirectory);
            DefaultProcessorSettings.EnableTelemetryLogger = options.IsVerboseEnabled;
            DefaultProcessorSettings.AllowOutputValidationErrors = options.AllowOutputValidationErrors ?? false;
            DefaultProcessorSettings.Validation.ValidateOutput = options.ValidateOutput ?? false;
            DefaultProcessorSettings.Validation.FhirCacheDirectory = options.FhirCacheDirectory;
            DefaultProcessorSettings.SerializationFormat = ParseSerializationFormat(options.SerializationFormat);

            bool rawOutput = options.RawOutputOnly ?? false;

            if (!string.IsNullOrEmpty(options.InputDataContent))
            {
                ConvertSingleFile(dataProcessor, templateProvider, dataType, options.RootTemplate, options.InputDataContent, options.OutputDataFile, options.IsTraceInfo, rawOutput);
            }
            else if (!string.IsNullOrEmpty(options.InputDataFile))
            {
                var fileContent = File.ReadAllText(options.InputDataFile);
                ConvertSingleFile(dataProcessor, templateProvider, dataType, options.RootTemplate, fileContent, options.OutputDataFile, options.IsTraceInfo, rawOutput);
            }
            else
            {
                bool continueBatchOnError = options.ContinueOnError ?? false;
                ConvertBatchFiles(dataProcessor, templateProvider, dataType, options.RootTemplate, options.InputDataFolder, options.OutputDataFolder, options.IsTraceInfo, rawOutput, continueBatchOnError);
            }

            Console.WriteLine($"Conversion completed!");
        }

        private static void ConvertSingleFile(IFhirConverter dataProcessor, ITemplateProvider templateProvider, DataType dataType, string rootTemplate, string inputContent, string outputFile, bool isTraceInfo, bool rawOutputOnly)
        {
            var traceInfo = CreateTraceInfo(dataType, isTraceInfo);
            ConverterResult result = null;
            string rawResultString = null;

            try
            {
                // We get raw output – can be json or xml
                rawResultString = dataProcessor.Convert(inputContent, rootTemplate, templateProvider, traceInfo);

                if (rawOutputOnly || DefaultProcessorSettings.SerializationFormat == FhirSerializationFormat.Json)
                {
                    // Raw output or json
                    result = new ConverterResult(ProcessStatus.OK, rawResultString, traceInfo);
                }
                else if (DefaultProcessorSettings.SerializationFormat == FhirSerializationFormat.Xml)
                {
                    // Wrap xml in json object to make FhirResource allowed
                    var wrapper = new { format = "xml", resource = rawResultString, };
                    var wrappedJson = JsonConvert.SerializeObject(wrapper);
                    result = new ConverterResult(ProcessStatus.OK, wrappedJson, traceInfo);
                }
            }
            catch (PostprocessException pex) when (DefaultProcessorSettings.AllowOutputValidationErrors || DefaultProcessorSettings.Validation.ValidateOutput) // catch and set a ConverterResult only when AllowOutputValidationErrors==true
            {
                result = new ConverterResult(ProcessStatus.OutputValidationError, pex.RawOutputString, traceInfo, pex.Message);
            }

            if (rawOutputOnly)
            {
                // Save raw output (json or xml)
                SaveRawOutputOnly(outputFile, result, rawResultString);
            }
            else
            {
                // Save json with ConverterResult wrapper
                SaveConverterResult(outputFile, result);
            }
        }

        private static void ConvertBatchFiles(IFhirConverter dataProcessor, ITemplateProvider templateProvider, DataType dataType, string rootTemplate, string inputFolder, string outputFolder, bool isTraceInfo, bool rawOutputOnly, bool continueOnError)
        {
            var files = GetInputFiles(dataType, inputFolder);
            var totalCount = files.Count;
            var logInterval = GetLogInterval(totalCount); // Dynamic log interval depending on number of files

            for (var index = 0; index < totalCount; index++)
            {
                var file = files[index];
                if (ShouldLogProgress(index, totalCount, logInterval))
                {
                    Console.WriteLine($"Processing ({index + 1} av {totalCount}) {Path.GetFullPath(file)}");
                }

            var totalCount = files.Count;
            var logInterval = GetLogInterval(totalCount); // Dynamic log interval depending on number of files

            for (var index = 0; index < totalCount; index++)
            {
                var file = files[index];
                if (ShouldLogProgress(index, totalCount, logInterval))
                {
                    Console.WriteLine($"Processing ({index + 1} av {totalCount}) {Path.GetFullPath(file)}");
                }

                var fileContent = File.ReadAllText(file);
                var outputFileDirectory = Path.Join(outputFolder, Path.GetRelativePath(inputFolder, Path.GetDirectoryName(file)));
                var extension = rawOutputOnly ? "." + DefaultProcessorSettings.SerializationFormat.ToString().ToLower() : ".json"; // Only use SerializationFormat as output file format for raw output. Otherwise all formats will be wrapped in json.
                var outputFilePath = Path.Join(outputFileDirectory, Path.GetFileNameWithoutExtension(file) + extension);
                try
                {
                    ConvertSingleFile(dataProcessor, templateProvider, dataType, rootTemplate, fileContent, outputFilePath, isTraceInfo, rawOutputOnly);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR converting file '{file}': {ex.Message}");
                    if (continueOnError)
                    {
                        var result = new ConverterResult(ProcessStatus.Fail, fhirResource: null, traceInfo: null, errorMessage: ex.Message);
                        SaveRawOutputOnly(outputFilePath, result, resultString: null);
                        continue; // Continue batch
                    }

                    // Stop batch
                    Console.WriteLine($"Batch is stopped on file '{file}'");
                    throw;
                }
            }
        }

        private static DataType GetDataTypes(string templateDirectory)
        {
            if (!Directory.Exists(templateDirectory))
            {
                throw new DirectoryNotFoundException($"Could not find template directory: {templateDirectory}");
            }

            var metadataPath = Path.Join(templateDirectory, MetadataFileName);
            if (!File.Exists(metadataPath))
            {
                throw new FileNotFoundException($"Could not find metadata.json in template directory: {templateDirectory}.");
            }

            var content = File.ReadAllText(metadataPath);
            var metadata = JsonConvert.DeserializeObject<Metadata>(content);
            if (Enum.TryParse<DataType>(metadata?.Type, ignoreCase: true, out var type))
            {
                return type;
            }

            throw new NotImplementedException($"The conversion from data type '{metadata?.Type}' to FHIR is not supported");
        }

        private static IFhirConverter CreateDataProcessor(DataType dataType)
        {
            return dataType switch
            {
                DataType.Hl7v2 => new Hl7v2Processor(DefaultProcessorSettings, ConsoleLoggerFactory.CreateLogger<Hl7v2Processor>()),
                DataType.Ccda => new CcdaProcessor(DefaultProcessorSettings, ConsoleLoggerFactory.CreateLogger<CcdaProcessor>()),
                DataType.Json => new JsonProcessor(DefaultProcessorSettings, ConsoleLoggerFactory.CreateLogger<JsonProcessor>()),
                DataType.Fhir => new FhirProcessor(DefaultProcessorSettings, ConsoleLoggerFactory.CreateLogger<FhirProcessor>()),
                DataType.XmlLeafing => new XmlLeafingProcessor(DefaultProcessorSettings, ConsoleLoggerFactory.CreateLogger<XmlProcessor>()),
                DataType.XmlAlwaysArray => new XmlArrayProcessor(DefaultProcessorSettings, ConsoleLoggerFactory.CreateLogger<XmlProcessor>()),
                _ => throw new NotImplementedException($"The conversion from data type {dataType} to FHIR is not supported")
            };
        }

        private static ITemplateProvider CreateTemplateProvider(DataType dataType, string templateDirectory)
        {
            return new TemplateProvider(templateDirectory, dataType);
        }

        private static TraceInfo CreateTraceInfo(DataType dataType, bool isTraceInfo)
        {
            return isTraceInfo ? (dataType == DataType.Hl7v2 ? new Hl7v2TraceInfo() : new TraceInfo()) : null;
        }

        private static List<string> GetInputFiles(DataType dataType, string inputDataFolder)
        {
            return dataType switch
            {
                DataType.Hl7v2 => Directory.EnumerateFiles(inputDataFolder, "*.hl7", SearchOption.AllDirectories).ToList(),
                DataType.Ccda => Directory.EnumerateFiles(inputDataFolder, "*.*", SearchOption.AllDirectories)
                    .Where(x => CcdaExtensions.Contains(Path.GetExtension(x).ToLower())).ToList(),
                DataType.Json => Directory.EnumerateFiles(inputDataFolder, "*.json", SearchOption.AllDirectories).ToList(),
                DataType.Fhir => Directory.EnumerateFiles(inputDataFolder, "*.json", SearchOption.AllDirectories).ToList(),
                DataType.XmlLeafing => Directory.EnumerateFiles(inputDataFolder, "*.xml", SearchOption.AllDirectories).ToList(),
                DataType.XmlAlwaysArray => Directory.EnumerateFiles(inputDataFolder, "*.xml", SearchOption.AllDirectories).ToList(),
                _ => new List<string>(),
            };
        }

        private static void SaveConverterResult(string outputFilePath, ConverterResult result)
        {
            var outputFileDirectory = Path.GetDirectoryName(outputFilePath);
            Directory.CreateDirectory(outputFileDirectory);

            var content = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            File.WriteAllText(outputFilePath, content);
        }

        private static void SaveRawOutputOnly(string outputFilePath, ConverterResult result, string resultString)
        {
            result ??= new ConverterResult(ProcessStatus.Fail, "ConverterResult was null when RawOutputOnly was requested.", traceInfo: null);
            var outputFileDirectory = Path.GetDirectoryName(outputFilePath);
            Directory.CreateDirectory(outputFileDirectory);

            // On success → write the raw transformationen (json or xml) to output file
            if (result.Status == ProcessStatus.OK)
            {
                File.WriteAllText(outputFilePath, resultString ?? string.Empty);
            }
            else
            {
                // On error → write .error file containing status + error + rawOutput
                var errorPath = outputFilePath + ".error";
                var errorPayload = new { result.Status, result.ErrorMessage, result.RawOutput, };

                var content = JsonConvert.SerializeObject(errorPayload, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                File.WriteAllText(errorPath, content);
            }
        }

        private static bool IsValidOptions(ConverterOptions options)
        {
            var contentToFile = !string.IsNullOrEmpty(options.InputDataContent) &&
                                string.IsNullOrEmpty(options.InputDataFile) &&
                                !string.IsNullOrEmpty(options.OutputDataFile) &&
                                string.IsNullOrEmpty(options.InputDataFolder) &&
                                string.IsNullOrEmpty(options.OutputDataFolder);

            var fileToFile = string.IsNullOrEmpty(options.InputDataContent) &&
                                !string.IsNullOrEmpty(options.InputDataFile) &&
                                !string.IsNullOrEmpty(options.OutputDataFile) &&
                                string.IsNullOrEmpty(options.InputDataFolder) &&
                                string.IsNullOrEmpty(options.OutputDataFolder) &&
                                !IsSameFile(options.InputDataFile, options.OutputDataFile);

            var folderToFolder = string.IsNullOrEmpty(options.InputDataContent) &&
                                 string.IsNullOrEmpty(options.InputDataFile) &&
                                 string.IsNullOrEmpty(options.OutputDataFile) &&
                                 !string.IsNullOrEmpty(options.InputDataFolder) &&
                                 !string.IsNullOrEmpty(options.OutputDataFolder) &&
                                 !IsSameDirectory(options.InputDataFolder, options.OutputDataFolder);

            return contentToFile || fileToFile || folderToFolder;
        }

        private static bool IsSameFile(string inputFile, string outputFile)
        {
            return string.Equals(inputFile, outputFile, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsSameDirectory(string inputFolder, string outputFolder)
        {
            string inputFolderPath = Path.GetFullPath(inputFolder)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string outputFolderPath = Path.GetFullPath(outputFolder)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return string.Equals(inputFolderPath, outputFolderPath, StringComparison.InvariantCultureIgnoreCase);
        }

        private static FhirSerializationFormat ParseSerializationFormat(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return FhirSerializationFormat.Json;
            }

            return format.Trim().ToLowerInvariant() switch
            {
                "json" or "js" or "default" => FhirSerializationFormat.Json,
                "xml" => FhirSerializationFormat.Xml,
                _ => throw new InputParameterException($"Unsupported serialization format '{format}'. Valid values are: json, xml.")
            };
        }

        private static int GetLogInterval(int totalCount)
        {
            return totalCount switch
            {
                < 50 => 1, // Log each if less than 50 files
                <= 500 => 10, // Log every 10th less than 500 files
                _ => 100 // else log every 100th
            };
        }

        private static bool ShouldLogProgress(int index, int totalCount, int logInterval)
        {
            if (index == 0 || index == totalCount - 1)
            {
                return true;
            }

            return (index + 1) % logInterval == 0;
        }

        private static int GetLogInterval(int totalCount)
        {
            return totalCount switch
            {
                < 50 => 1, // Log each if less than 50 files
                <= 500 => 10, // Log every 10th less than 500 files
                _ => 100 // else log every 100th
            };
        }

        private static bool ShouldLogProgress(int index, int totalCount, int logInterval)
        {
            if (index == 0 || index == totalCount - 1)
            {
                return true;
            }

            return (index + 1) % logInterval == 0;
        }
    }
}
