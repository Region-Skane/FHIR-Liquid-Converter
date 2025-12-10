// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using FLC.PackageManagement.Models;
using FLC.PackageManagement.Persistence.Models;

namespace FLC.PackageManagement.Persistence.Mappers;

public static class PackageToEntityMapper
{
    public static FlcImplementationGuide MapToFlcImplementationGuide(PackageResult package)
    {
        return new FlcImplementationGuide
        {
            ImplementationGuideId = package.ImplementationGuideItem.Id,
            Url = package.ImplementationGuideItem.Url,
            Version = package.ImplementationGuideItem.Version,
            PackageId = package.ImplementationGuideItem.PackageId,
            ResourceJson = package.ImplementationGuideItem.Json,
        };
    }

    public static FlcLibrary MapToFlcLibrary(PackageResult package, int implementationGuidId)
    {
        return new FlcLibrary
        {
            LibraryId = package.LibraryItem.Id,
            Url = package.LibraryItem.Url,
            Version = package.LibraryItem.Version,
            FlcImplementationGuideId = implementationGuidId,
            StorageRoot = package.LibraryItem.StorageRoot,

            // Checksum = // TODO: Implement this if we want to use a checksum on the folder
            ResourceJson = package.LibraryItem.Json,
        };
    }

    public static IEnumerable<FlcStructureMap> MapToFlcStructureMaps(PackageResult package, int implementationGuidId, int libraryId)
    {
        return package.StructureMapItems.Select(sm => new FlcStructureMap
        {
            StructureMapId = sm.Id,
            Url = sm.Url,
            Version = sm.Version,
            FlcImplementationGuideId = implementationGuidId,
            Source = sm.Source,
            SourceVersion = sm.SourceVersion,
            Target = sm.Target,
            TargetVersion = sm.TargetVersion,
            FlcLibraryId = libraryId,
            EntryTemplate = sm.EntryTemplate,
            ResourceJson = sm.Json,
        });
    }
}
