// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using FLC.PackageManagement.Persistence.Initialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FLC.PackageManagement.Persistence.Extensions;

public static class DatabaseInitializationExtensions
{
    // Initializes the database by resolving IDatabaseInitializer from a scoped service provider.
    // Intended for CLI applications that do not use IHostedService.
    public static async Task InitializePackageManagementDatabaseAsync(
        this IHost host,
        CancellationToken cancellationToken = default)
    {
        using var scope = host.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();

        await initializer.InitializeAsync(cancellationToken);
    }
}
