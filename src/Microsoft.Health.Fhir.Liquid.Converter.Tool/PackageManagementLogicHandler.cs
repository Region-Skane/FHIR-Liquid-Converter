// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FLC.PackageManagement.Extensions;
using FLC.PackageManagement.Persistence.Extensions;
using FLC.PackageManagement.Persistence.Initialization;
using FLC.PackageManagement.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Configuration;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool;

internal static class PackageManagementLogicHandler
{
    internal static async Task ImportPackageAsync(PackageManagementOptions options)
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
}
