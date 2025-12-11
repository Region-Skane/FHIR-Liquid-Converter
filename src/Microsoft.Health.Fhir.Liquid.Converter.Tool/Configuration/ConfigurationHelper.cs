// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Health.Fhir.Liquid.Converter.Tool.Configuration;

public static class ConfigurationHelper
{
    /// <summary>
    /// Builds a configuration object using:
    /// - appsettings.json
    /// - appsettings.{Environment}.json
    /// - environment variables
    /// If the "PackageManagement" section is missing completely,
    /// default values will be injected automatically.
    /// </summary>
    /// <param name="environmentVariableName">
    /// Name of the environment variable that holds the environment name.
    /// Default is DOTNET_ENVIRONMENT.
    /// </param>
    public static IConfiguration BuildConfiguration(string environmentVariableName = "DOTNET_ENVIRONMENT")
    {
        var environment = Environment.GetEnvironmentVariable(environmentVariableName);

        // Step 1: load base config (json + env)
        var baseConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        // Step 2: detect if ANY PackageManagement entries exist
        bool hasPackageManagementConfig =
            baseConfig.GetSection("PackageManagement").Exists() &&
            baseConfig.GetSection("PackageManagement").GetChildren().Any();

        if (hasPackageManagementConfig)
        {
            return baseConfig; // Use user-supplied config as-is
        }

        // Step 3: inject fallback defaults because section is missing entirely
        var defaultValues = new Dictionary<string, string>
        {
            ["PackageManagement:Database:Provider"] = "SQLite",
            ["PackageManagement:Database:InitializeAtStartup"] = "true",
            ["PackageManagement:ConnectionStrings:SQLite"] = "Data Source=./database/flc-transformer.db;Cache=Shared;Foreign Keys=True",
        };

        return new ConfigurationBuilder()
            .AddConfiguration(baseConfig)
            .AddInMemoryCollection(defaultValues) // fallback config
            .Build();
    }
}
