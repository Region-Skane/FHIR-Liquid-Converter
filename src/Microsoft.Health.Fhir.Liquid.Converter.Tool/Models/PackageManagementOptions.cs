// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using CommandLine;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;

// Sample: Microsoft.Health.Fhir.Liquid.Converter.Tool import-package C:\Data\Import\package_251127_01.tgz -d C:\Data\PackageRoot -o false
[Verb("import-package", HelpText = "Import a FHIR package (.tgz) into the local package store")]
public sealed class PackageManagementOptions
{
    [Value(0, MetaName = "FhirPackagePath", Required = true, HelpText = "Path to the FHIR package (.tgz) file to import")]
    public string InputFhirPackageFilePath { get; set; } = string.Empty;

    [Option('d', "FhirPackagesDirectory", Required = true, HelpText = "Root directory where FHIR packages will be extracted")]
    public string FhirPackagesRootPath { get; set; } = string.Empty;

    [Option('o', "AllowOverwrite", Required = false, HelpText = "Allow overwriting an existing ImplementationGuide folder (same packageId + version). Default: false.")]
    public bool? AllowPackageOverwrite { get; set; }

    [Option('p', "UsePersistence", Required = false, HelpText = "Use sqllite database persistence for package meta when importing the FHIR package. Default: true.")]
    public bool? UsePersistence { get; set; }
}
