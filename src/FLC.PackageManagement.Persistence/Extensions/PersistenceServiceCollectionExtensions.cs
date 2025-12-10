// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using FLC.PackageManagement.Persistence.DbFactory;
using FLC.PackageManagement.Persistence.Initialization;
using FLC.PackageManagement.Persistence.Interfaces;
using FLC.PackageManagement.Persistence.Repositories;
using FLC.PackageManagement.Persistence.Services;
using FLC.PackageManagement.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FLC.PackageManagement.Persistence.Extensions;

public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Registers database persistence for PackageManagement (connection factory, repositories and IDatabaseInitializer).
    /// The PackageManagement.Persistence module adds database persistence for FHIR package contents, including Implementation Guides, Libraries, and StructureMaps.
    /// When a package is imported, meta data from artifacts are extracted and written to the database so that conversion operations can access them without beeing depending on path to disk for Liquid templates.
    /// This enables consistent, centralized storage of all FHIR transformation assets and improves runtime reliability and deployment flexibility.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This extension assumes that <c>AddPackageManagement()</c> has already been called.
    /// It reads its configuration from the <c>PackageManagement</c> section and expects at least:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <c>PackageManagement:Database:Provider</c> – database provider name, e.g. <c>SQLite</c> or <c>SQLServer</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>PackageManagement:ConnectionStrings:&lt;Provider&gt;</c> – matching connection string, e.g.
    /// <c>PackageManagement:ConnectionStrings:SQLite</c>.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// Example <c>appsettings.json</c> section:
    /// </para>
    /// <code language="json">
    /// "PackageManagement": {
    ///   "Database": {
    ///     "Provider": "SQLite",
    ///     "InitializeAtStartup": true
    ///   },
    ///   "ConnectionStrings": {
    ///     "SQLite": "Data Source=./database/flc-transformer.db;Cache=Shared;Foreign Keys=True"
    ///   }
    /// }
    /// </code>
    /// <para>
    /// Equivalent configuration using environment variables:
    /// </para>
    /// <code>
    /// PackageManagement__Database__Provider=SQLite
    /// PackageManagement__Database__InitializeAtStartup=true
    /// PackageManagement__ConnectionStrings__SQLite=Data Source=./database/flc-transformer.db;Cache=Shared;Foreign Keys=True
    /// </code>
    /// </remarks>
    /// <param name="services">The service collection to add persistence services to.</param>
    /// <param name="config">Application configuration used to resolve PackageManagement settings.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required configuration values (provider or connection string) are missing or invalid.
    /// </exception>
    public static IServiceCollection AddPackageManagementPersistence(this IServiceCollection services, IConfiguration config)
    {
        var provider = config["PackageManagement:Database:Provider"];
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new InvalidOperationException("Missing database provider (PackageManagement:Database:Provider).");
        }

        var connectionString = config[$"PackageManagement:ConnectionStrings:{provider}"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Missing connection string (PackageManagement:ConnectionStrings:{provider}).");
        }

        bool initializeAtStartup = config.GetValue("PackageManagement:Database:InitializeAtStartup", true);
        if (provider.Equals("SQLServer", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IDbConnectionFactory>(new SqlServerConnectionFactory(connectionString));

            // ToDo: Add SqlServerDatabaseInitializer
            if (initializeAtStartup)
            {
                throw new InvalidOperationException($"Provider '{provider}' cannot be initialized at startup. Create database and tables manually, then set PackageManagement:Database:InitializeAtStartup = false.");
            }
        }
        else if (provider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IDbConnectionFactory>(new SqliteConnectionFactory(connectionString));
            services.AddSingleton<IDatabaseInitializer, SqliteDatabaseInitializer>();

            if (initializeAtStartup)
            {
                services.AddHostedService<SqliteInitializer>();
            }
        }
        else
        {
            throw new InvalidOperationException($"Invalid database provider {provider}. Allowed are SQLite and SQLServer");
        }

        services.AddScoped<IFlcImplementationGuideRepository, FlcImplementationGuideRepository>();
        services.AddScoped<IFlcStructureMapRepository, FlcStructureMapRepository>();
        services.AddScoped<IFlcLibraryRepository, FlcLibraryRepository>();
        services.AddScoped<IPersistenceService, PersistenceService>();

        return services;
    }
}
