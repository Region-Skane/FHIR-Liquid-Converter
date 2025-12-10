// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FLC.PackageManagement.Persistence.Initialization;

public class SqliteInitializer(
                IConfiguration configuration,
                IHostEnvironment hostEnvironment,
                IDatabaseInitializer databaseInitializer,
                ILogger<SqliteInitializer> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        logger.LogInformation("Starting database initialization...");

        SqliteDatabaseFileHelper.EnsureSqliteFolder(configuration, hostEnvironment.ContentRootPath);

        await databaseInitializer.InitializeAsync(ct);
        logger.LogInformation("Database initialization completed.");
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

}
