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
    private static readonly ILogger Logger = ConsoleLoggerFactory.CreateLogger(typeof(PackageManagementLogicHandler));

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

        using var serviceProvider = await BuildServiceProviderAsync(configuration, usePersistence);

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

        using var serviceProvider = await BuildServiceProviderAsync(
            configuration,
            usePersistence: true,
            requireInitializer: true,
            initializerRequiredMessage: "Could not resolve IDatabaseInitializer which is required for package listing.");
        if (serviceProvider is null)
        {
            return;
        }

        var repository = serviceProvider.GetRequiredService<IFlcStructureMapRepository>();
        IReadOnlyList<ImplementationGuideStructureMapLibraryFlat> rows = await repository.GetAllFlatAsync();

        if (rows.Count == 0)
        {
            Logger.LogInformation("No ImplementationGuides / StructureMaps found in package management persistence.");
            return;
        }

        PrintImplementationGuideHierarchy(rows, string.Empty);
    }

    internal static async Task<int> ValidatePackage(PackageManagementValidateOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.StructureMapUrl) && string.IsNullOrWhiteSpace(options.ImplementationGuideUrl))
        {
            throw new InputParameterException("Either StructureMapUrl or ImplementationGuideUrl must be provided.");
        }

        // Build configuration with possible PackageManagement defaults
        IConfiguration configuration = ConfigurationHelper.BuildConfiguration();

        using var serviceProvider = await BuildServiceProviderAsync(
            configuration,
            usePersistence: true,
            requireInitializer: true,
            initializerRequiredMessage: "Could not resolve IDatabaseInitializer which is required for package validation.");
        if (serviceProvider is null)
        {
            return 1;
        }

        var repository = serviceProvider.GetRequiredService<IFlcStructureMapRepository>();
        IReadOnlyList<ImplementationGuideStructureMapLibraryFlat> rows = await repository.GetAllFlatAsync();

        List<ImplementationGuideStructureMapLibraryFlat> matching = new ();

        if (!string.IsNullOrWhiteSpace(options.StructureMapUrl))
        {
            var parts = options.StructureMapUrl.Split('|');
            var structureMapUrl = parts[0];
            var structureMapVersion = parts.Length > 1 ? parts[1] : null;

            matching = rows
                .Where(r => r.StructureMapUrl == structureMapUrl)
                .Where(r => structureMapVersion == null || r.StructureMapVersion == structureMapVersion)
                .ToList();

            if (matching.Count == 0)
            {
                Logger.LogError("Package validation failed: StructureMap not found: {StructureMapUrl}", options.StructureMapUrl);
                return 1;
            }

            Console.WriteLine($"StructureMap found: {options.StructureMapUrl}");
            PrintImplementationGuideHierarchy(matching, "  ");

            return 0;
        }

        if (!string.IsNullOrWhiteSpace(options.ImplementationGuideUrl))
        {
            var parts = options.ImplementationGuideUrl.Split('|');
            var igUrl = parts[0];
            var igVersion = parts.Length > 1 ? parts[1] : null;

            matching = rows
                .Where(r => r.ImplementationGuideUrl == igUrl)
                .Where(r => igVersion == null || r.ImplementationGuideVersion == igVersion)
                .ToList();

            if (matching.Count == 0)
            {
                Logger.LogError("Package validation failed: ImplementationGuide not found: {ImplementationGuideUrl}", options.ImplementationGuideUrl);
                return 1;
            }

            Console.WriteLine($"ImplementationGuide found: {options.ImplementationGuideUrl}");

            PrintImplementationGuideHierarchy(matching, "  ");

            return 0;
        }

        return 1;
    }

    private static void PrintImplementationGuideHierarchy(
        IEnumerable<ImplementationGuideStructureMapLibraryFlat> rows,
        string implementationGuideIndent)
    {
        var libraryIndent = implementationGuideIndent + "  ";
        var structureMapIndent = implementationGuideIndent + "    ";

        var igGroups = rows
            .GroupBy(r => new { r.ImplementationGuideUrl, r.ImplementationGuideVersion })
            .OrderBy(g => g.Key.ImplementationGuideUrl)
            .ThenBy(g => g.Key.ImplementationGuideVersion);

        foreach (var igGroup in igGroups)
        {
            var igUrlWithVersion = $"{igGroup.Key.ImplementationGuideUrl}|{igGroup.Key.ImplementationGuideVersion}";
            Console.WriteLine($"{implementationGuideIndent}ImplementationGuide: {igUrlWithVersion}");

            var libraryGroups = igGroup
                .GroupBy(r => new { r.LibraryUrl, r.LibraryVersion })
                .OrderBy(g => g.Key.LibraryUrl)
                .ThenBy(g => g.Key.LibraryVersion);

            foreach (var libGroup in libraryGroups)
            {
                var libUrlWithVersion = $"{libGroup.Key.LibraryUrl}|{libGroup.Key.LibraryVersion}";
                Console.WriteLine($"{libraryIndent}Library:           {libUrlWithVersion}");

                foreach (var row in libGroup.OrderBy(r => r.StructureMapUrl).ThenBy(r => r.StructureMapVersion))
                {
                    var smUrlWithVersion = $"{row.StructureMapUrl}|{row.StructureMapVersion}";
                    Console.WriteLine($"{structureMapIndent}StructureMap:    {smUrlWithVersion}");
                }

                Console.WriteLine();
            }
        }
    }

    private static async Task<ServiceProvider> BuildServiceProviderAsync(
        IConfiguration configuration,
        bool usePersistence,
        bool requireInitializer = false,
        string initializerRequiredMessage = null)
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddPackageManagement();

        if (usePersistence)
        {
            services.AddPackageManagementPersistence(configuration);
        }

        var serviceProvider = services.BuildServiceProvider();

        if (usePersistence)
        {
            var contentRootPath = Directory.GetCurrentDirectory();
            SqliteDatabaseFileHelper.EnsureSqliteFolder(configuration, contentRootPath);
            var dbInitializer = serviceProvider.GetService<IDatabaseInitializer>();
            if (dbInitializer is not null)
            {
                await dbInitializer.InitializeAsync(CancellationToken.None);
            }
            else if (requireInitializer)
            {
                Logger.LogError(initializerRequiredMessage ?? "Could not resolve IDatabaseInitializer.");
                return null;
            }
        }

        return serviceProvider;
    }
}
