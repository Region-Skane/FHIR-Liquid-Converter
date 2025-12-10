// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
namespace FLC.PackageManagement.Persistence.Models.Custom;

public sealed class ImplementationGuideStructureMapLibraryFlat
{
    public int FlcImplementationGuideId { get; set; }

    public string ImplementationGuideUrl { get; set; } = string.Empty;

    public string ImplementationGuideVersion { get; set; } = string.Empty;

    public int FlcStructureMapId { get; set; }

    public string StructureMapUrl { get; set; } = string.Empty;

    public string StructureMapVersion { get; set; } = string.Empty;

    public int FlcLibraryId { get; set; }

    public string LibraryUrl { get; set; } = string.Empty;

    public string LibraryVersion { get; set; } = string.Empty;
}
