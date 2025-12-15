// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using CommandLine;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;

// Sample: Microsoft.Health.Fhir.Liquid.Converter.Tool list-packages
[Verb("list-packages", HelpText = "List ImplementationGuides, Libraries and StructureMaps from package management persistence.")]
public sealed class PackageManagementListOptions
{
    // Intentionally empty – command has no filters/options.
}
