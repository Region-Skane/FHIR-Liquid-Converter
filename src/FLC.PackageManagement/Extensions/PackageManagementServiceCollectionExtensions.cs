// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using FLC.PackageManagement.Services;
using FLC.PackageManagement.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FLC.PackageManagement.Extensions
{
    public static class PackageManagementServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the core PackageManagement services required for extracting,
        /// unpacking and loading FHIR packages to disk.
        ///
        /// A persistence implementation can be added via
        /// <c>AddPackageManagementPersistence()</c> for database persistence of the FHIR package contents, including Implementation Guides, Libraries, and StructureMaps.
        /// </summary>
        /// <param name="services">The service collection to register PackageManagement services into.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public static IServiceCollection AddPackageManagement(this IServiceCollection services)
        {
            services.AddScoped<IPackageService, PackageService>();
            services.AddScoped<IExtractService, ExtractService>();
            services.AddScoped<ILibraryService, LibraryService>();

            // Always register a default NoOp-implementation of IPersistenceService.
            // If a real persistence service is registered later it willbe used
            services.AddSingleton<IPersistenceService, NoOpPersistenceService>();
            return services;
        }
    }
}
