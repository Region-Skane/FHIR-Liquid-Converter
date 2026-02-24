// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using CommandLine;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;

[Verb("flc-convert", HelpText = "Convert input data to FHIR resources using FLC package management (StructureMap + FHIR packages).")]
public class FlcConvertOptions
{
    [Option('m', "StructureMapUrl", Required = true, HelpText = "Canonical StructureMap URL including version (e.g. 'http://example.org/fhir/StructureMap/MyMap|1.0.0'). The corresponding templates will be resolved via FLC.PackageManagement.Persistence.")]
    public string StructureMapUrl { get; set; } = string.Empty;

    [Option('c', "InputDataContent", Required = false, HelpText = "Input data content. Please specify OutputDataFile to get the results.")]
    public string InputDataContent { get; set; }

    [Option('n', "InputDataFile", Required = false, HelpText = "Input data file. Please specify OutputDataFile to get the results.")]
    public string InputDataFile { get; set; }

    [Option('f', "OutputDataFile", Required = false, HelpText = "Output data file")]
    public string OutputDataFile { get; set; }

    [Option('i', "InputDataFolder", Required = false, HelpText = "Input data folder. Please specify OutputDataFolder to get the results.")]
    public string InputDataFolder { get; set; }

    [Option('o', "OutputDataFolder", Required = false, HelpText = "Output data folder")]
    public string OutputDataFolder { get; set; }

    [Option('t', "IsTraceInfo", Required = false, HelpText = "Provide trace information in the output")]
    public bool IsTraceInfo { get; set; }

    [Option('v', "Verbose", Required = false, HelpText = "Output detailed processor diagnostics and performance data.")]
    public bool IsVerboseEnabled { get; set; }

    [Option('a', "AllowOutputValidationErrors", Required = false, HelpText = "Allow output validation errors and return the raw output if validation fails. By default, validation errors will cause the process to fail.")]
    public bool AllowOutputValidationErrors { get; set; } = false;

    [Option('u', "ValidateOutput", Required = false, HelpText = "Validate output FHIR resources against their declared profiles. Default is false.")]
    public bool ValidateOutput { get; set; } = false;

    [Option('k', "FhirCacheDirectory", Required = false, HelpText = "Directory to use for caching FHIR resources. If not set, the default FHIR cache directory is used.")]
    public string FhirCacheDirectory { get; set; } = null;

    [Option('s', "SerializationFormat", Required = false, HelpText = "FHIR serialization format: json or xml. Default is json.")]
    public string SerializationFormat { get; set; } = "json";

    [Option("RawOutputOnly", Required = false, HelpText = "If set (--RawOutputOnly true), save the raw transformed FHIR payload on success. On error, save a '.error' file containing status, error message and raw output. Default is false.")]
    public bool? RawOutputOnly { get; set; }

    [Option("ContinueOnError", Required = false, HelpText = "If set (--ContinueOnError true), batch conversion will continue even if individual files fail. Errors are logged and written to .error files. Default is false.")]
    public bool? ContinueOnError { get; set; }

    /// <summary>
    /// Converts FLC-specific converter options into a standard ConverterOptions used by ConverterLogicHandler.
    /// </summary>
    /// <param name="templatesRoot">Templates root directory</param>
    /// <param name="entryTemplate">Entry template name</param>
    public ConverterOptions ToConverterOptions(string templatesRoot, string entryTemplate)
    {
        return new ConverterOptions
        {
            TemplateDirectory = templatesRoot,
            RootTemplate = entryTemplate,
            InputDataContent = InputDataContent,
            InputDataFile = InputDataFile,
            OutputDataFile = OutputDataFile,
            InputDataFolder = InputDataFolder,
            OutputDataFolder = OutputDataFolder,
            IsTraceInfo = IsTraceInfo,
            IsVerboseEnabled = IsVerboseEnabled,
            AllowOutputValidationErrors = AllowOutputValidationErrors,
            ValidateOutput = ValidateOutput,
            FhirCacheDirectory = FhirCacheDirectory,
            SerializationFormat = SerializationFormat,
            RawOutputOnly = RawOutputOnly,
            ContinueOnError = ContinueOnError,
        };
    }
}
