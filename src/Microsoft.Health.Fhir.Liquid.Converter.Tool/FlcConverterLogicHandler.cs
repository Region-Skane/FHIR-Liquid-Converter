// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FLC.PackageManagement.Extensions;
using FLC.PackageManagement.Persistence.Extensions;
using FLC.PackageManagement.Persistence.Initialization;
using FLC.PackageManagement.Persistence.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Configuration;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool;

internal static class FlcConverterLogicHandler
{
    internal static async Task FlcConvert(FlcConvertOptions flcConvertOptions)
    {
        ValidateInput(flcConvertOptions);

        // Load package-management-enabled configuration
        var configuration = ConfigurationHelper.BuildConfiguration();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddPackageManagement();
        services.AddPackageManagementPersistence(configuration);

        using var provider = services.BuildServiceProvider();

        // Ensure SQLite exists + initialized
        var contentRoot = Directory.GetCurrentDirectory();
        SqliteDatabaseFileHelper.EnsureSqliteFolder(configuration, contentRoot);
        var dbInitializer = provider.GetService<IDatabaseInitializer>();
        if (dbInitializer is not null)
        {
            await dbInitializer.InitializeAsync(CancellationToken.None);
        }

        // Resolve StructureMap
        var structureMapRepo = provider.GetRequiredService<IFlcStructureMapRepository>();
        var (url, version) = SplitUrlAndVersion(flcConvertOptions.StructureMapUrl);

        var dbStructureMap = await structureMapRepo.GetByUrlAndVersionAsync(url, version);
        if (dbStructureMap is null)
        {
            throw new InvalidOperationException($"Could not find StructureMap '{flcConvertOptions.StructureMapUrl}' in sqlite persistence.");
        }

        // Resolve TemplateInfo
        var templateInfo = await structureMapRepo.GetTemplateInfoByStructureMapIdAsync(dbStructureMap.Id);
        if (templateInfo is null)
        {
            throw new InvalidOperationException($"Could not resolve template information for StructureMap '{flcConvertOptions.StructureMapUrl}'.");
        }

        var templatesRoot = templateInfo.TemplatesRoot;
        var entryTemplate = templateInfo.EntryTemplate;

        var converterOptions = flcConvertOptions.ToConverterOptions(templateInfo.TemplatesRoot, templateInfo.EntryTemplate);

        // Use existing converter logic for validation and conversion
        ConverterLogicHandler.Convert(converterOptions);
    }

    private static void ValidateInput(FlcConvertOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.StructureMapUrl))
        {
            throw new InputParameterException("--StructureMapUrl must be provided in format 'url|version'.");
        }

        if (string.IsNullOrWhiteSpace(options.OutputDataFile)
            && string.IsNullOrWhiteSpace(options.OutputDataFolder))
        {
            throw new InputParameterException("Please specify OutputDataFile or OutputDataFolder to collect results.");
        }
    }

    private static (string url, string version) SplitUrlAndVersion(string urlWithVersion)
    {
        var parts = urlWithVersion.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            throw new ArgumentException($"Invalid StructureMapUrl value '{urlWithVersion}'. Expected format: url|version");
        }

        return (parts[0], parts[1]);
    }
}
