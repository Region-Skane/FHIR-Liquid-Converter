// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
namespace FLC.PackageManagement.Models;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable SA1513 // Closing brace should be followed by blank line
public record PackageResult(
    ImplementationGuideItem ImplementationGuideItem,
    LibraryItem LibraryItem,
    List<StructureMapItem> StructureMapItems);

public record ImplementationGuideItem(
    string Id,
    string Url,
    string Version,
    string PackageId,
    string Json);

public record LibraryItem(
    string Id,
    string Url,
    string Version,
    string StorageRoot,
    string Json)
{
    public static LibraryItem CreateFrom(LibraryItem src, string storageRoot)
        => src with { StorageRoot = storageRoot };
};

public record StructureMapItem(
    string Id,
    string Url,
    string Version,
    string Source,
    string SourceVersion,
    string Target,
    string TargetVersion,
    string EntryTemplate,
    string Json)
{
    public static StructureMapItem CreateFrom(StructureMapItem src, string entryTemplate)
        => src with { EntryTemplate = entryTemplate };
};

#pragma warning restore SA1313
#pragma warning restore SA1513
