// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
namespace FLC.PackageManagement.Persistence.Models.Custom;

public sealed class StructureMapTemplateInfo
{
    public int FlcStructureMapId { get; set; }

    public string EntryTemplate { get; set; } = string.Empty;

    public string TemplatesRoot { get; set; } = string.Empty;
}
