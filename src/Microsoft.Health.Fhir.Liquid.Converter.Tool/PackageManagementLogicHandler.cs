// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FLC.PackageManagement.Extensions;
using FLC.PackageManagement.Persistence.Extensions;
using FLC.PackageManagement.Persistence.Initialization;
using FLC.PackageManagement.Persistence.Interfaces;
using FLC.PackageManagement.Persistence.Models.Custom;
using FLC.PackageManagement.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Configuration;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool;

internal static class PackageManagementLogicHandler
{
    internal static async Task ImportPackage(PackageManagementOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.InputFhirPackageFilePath))
        {
            throw new InputParameterException("Input FHIR package file path is required.");
        }

        if (!File.Exists(options.InputFhirPackageFilePath))
        {
            throw new InputParameterException($"Input FHIR package file '{options.InputFhirPackageFilePath}' does not exist.");
        }

        if (string.IsNullOrWhiteSpace(options.FhirPackagesRootPath))
        {
            throw new InputParameterException("FhirPackagesRootPath is required.");
        }

        // Load appsettings and persistence default values
        var configuration = ConfigurationHelper.BuildConfiguration();

        var usePersistence = options.UsePersistence ?? true;
        var allowOverwrite = options.AllowPackageOverwrite ?? false;

        // Build a service provider with PackageManagement
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddPackageManagement();

        if (usePersistence)
        {
            services.AddPackageManagementPersistence(configuration);
        }

        using var serviceProvider = services.BuildServiceProvider();

        if (usePersistence)
        {
            var contentRootPath = Directory.GetCurrentDirectory();
            SqliteDatabaseFileHelper.EnsureSqliteFolder(configuration, contentRootPath);
            var dbInitializer = serviceProvider.GetService<IDatabaseInitializer>();
            if (dbInitializer is not null)
            {
                await dbInitializer.InitializeAsync(CancellationToken.None);
            }
        }

        var packageService = serviceProvider.GetRequiredService<IPackageService>();

        await using var stream = File.OpenRead(options.InputFhirPackageFilePath);

        await packageService.LoadPackage(
                stream,
                allowOverwrite,
                options.FhirPackagesRootPath,
                CancellationToken.None);
    }

    internal static async Task ListPackages(PackageManagementListOptions listOptions)
    {
        _ = listOptions;

        // Build configuration with possible PackageManagement defaults
        IConfiguration configuration = ConfigurationHelper.BuildConfiguration();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddPackageManagement();
        services.AddPackageManagementPersistence(configuration);

        using var serviceProvider = services.BuildServiceProvider();

        // ensure SQLite folder exists + run schema initializer if configured
        var contentRootPath = Directory.GetCurrentDirectory();
        SqliteDatabaseFileHelper.EnsureSqliteFolder(configuration, contentRootPath);
        var dbInitializer = serviceProvider.GetService<IDatabaseInitializer>();
        if (dbInitializer is not null)
        {
            await dbInitializer.InitializeAsync(CancellationToken.None);
        }
        else
        {
            Console.WriteLine("Could not resolve IDatabaseInitializer which is required for package listing.");
            return;
        }

        var repository = serviceProvider.GetRequiredService<IFlcStructureMapRepository>();
        IReadOnlyList<ImplementationGuideStructureMapLibraryFlat> rows = await repository.GetAllFlatAsync();

        if (rows.Count == 0)
        {
            Console.WriteLine("No ImplementationGuides / StructureMaps found in package management persistence.");
            return;
        }

        // Group by IG → Library → StructureMap
        var igGroups = rows
            .GroupBy(r => new { r.ImplementationGuideUrl, r.ImplementationGuideVersion })
            .OrderBy(g => g.Key.ImplementationGuideUrl)
            .ThenBy(g => g.Key.ImplementationGuideVersion);

        foreach (var igGroup in igGroups)
        {
            var igUrlWithVersion = $"{igGroup.Key.ImplementationGuideUrl}|{igGroup.Key.ImplementationGuideVersion}";
            Console.WriteLine($"ImplementationGuide: {igUrlWithVersion}");

            var libraryGroups = igGroup
                .GroupBy(r => new { r.LibraryUrl, r.LibraryVersion })
                .OrderBy(g => g.Key.LibraryUrl)
                .ThenBy(g => g.Key.LibraryVersion);

            foreach (var libGroup in libraryGroups)
            {
                var libUrlWithVersion = $"{libGroup.Key.LibraryUrl}|{libGroup.Key.LibraryVersion}";
                Console.WriteLine($"  Library:           {libUrlWithVersion}");

                foreach (var row in libGroup.OrderBy(r => r.StructureMapUrl).ThenBy(r => r.StructureMapVersion))
                {
                    var smUrlWithVersion = $"{row.StructureMapUrl}|{row.StructureMapVersion}";
                    Console.WriteLine($"    StructureMap:    {smUrlWithVersion}");
                }

                Console.WriteLine();
            }
        }
    }
}
