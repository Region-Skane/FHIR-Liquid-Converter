// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using CommandLine;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;

// Sample: Microsoft.Health.Fhir.Liquid.Converter.Tool validate-package -n "http://example.com/StructureMap/Patient|0.1.1"
// Sample: Microsoft.Health.Fhir.Liquid.Converter.Tool validate-package -i "http://example.com/ImplementationGuide/Patient|1.0.0"
[Verb("validate-package", HelpText = "Validate that a StructureMap or ImplementationGuide exists in the package management persistence.")]
public sealed class PackageManagementValidateOptions
{
    [Option('n', "StructureMapUrl", Required = false, HelpText = "The StructureMap URL to validate. Can include version in format: url|version (e.g., http://example.com/StructureMap/Patient|0.1.1). If version is omitted, all versions will be searched.")]
    public string StructureMapUrl { get; set; }

    [Option('i', "ImplementationGuideUrl", Required = false, HelpText = "The ImplementationGuide URL to validate. Can include version in format: url|version (e.g., http://example.com/ImplementationGuide/Patient|1.0.0). If version is omitted, all versions will be searched.")]
    public string ImplementationGuideUrl { get; set; }
}