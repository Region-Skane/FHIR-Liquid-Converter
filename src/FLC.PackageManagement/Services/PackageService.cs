// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using FLC.PackageManagement.Services.Interfaces;

namespace FLC.PackageManagement.Services;

public class PackageService(
    IExtractService extractService,
    ILibraryService libraryService,
    IPersistenceService persistenceService)
    : IPackageService
{
    public async Task LoadPackage(Stream stream, bool allowPackageOverwrite, string fhirPackagesRootPath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(fhirPackagesRootPath, "fhirPackagesRoot is missing or not set.");
        var packageResult = await extractService.ExtractPackage(stream, fhirPackagesRootPath, cancellationToken);

        var igFolderPath = Path.Combine(
            fhirPackagesRootPath,
            packageResult.ImplementationGuideItem.PackageId,
            packageResult.ImplementationGuideItem.Version);

        if (Directory.Exists(igFolderPath))
        {
            if (!allowPackageOverwrite)
            {
                throw new InvalidOperationException($"ImplementationGuide already exists for packageId:\"{packageResult.ImplementationGuideItem.PackageId}\", version:\"{packageResult.ImplementationGuideItem.Version}\". Use parameter '-o true' for allowOverwrite=true to force overwriting.");
            }
            else
            {
                // Recursively delete existing folder when overwrite is allowed
                Directory.Delete(igFolderPath, recursive: true);

                // Re-create folder so we start clean
                Directory.CreateDirectory(igFolderPath);
            }
        }
        else
        {
            Directory.CreateDirectory(igFolderPath);
        }

        var libraryRootFolderPath = Path.Combine(igFolderPath, packageResult.LibraryItem.Id);
        await libraryService.Unpack(libraryRootFolderPath, packageResult.LibraryItem.Json);

        await persistenceService.UpsertPackageToDatabase(packageResult);
    }
}