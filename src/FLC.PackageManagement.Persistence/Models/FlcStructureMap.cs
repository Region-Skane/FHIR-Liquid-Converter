// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using Dapper.Contrib.Extensions;
using FLC.PackageManagement.Persistence.Interfaces;

namespace FLC.PackageManagement.Persistence.Models;

[Table("flcStructureMap")]
public sealed class FlcStructureMap : IHasTimestamps
{
    [Key]
    public int Id { get; set; }

    public string StructureMapId { get; set; }

    public string Url { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public int FlcImplementationGuideId { get; set; }

    public string Source { get; set; }

    public string SourceVersion { get; set; }

    public string Target { get; set; }

    public string TargetVersion { get; set; }

    public int FlcLibraryId { get; set; }

    public string EntryTemplate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string ResourceJson { get; set; }
}
