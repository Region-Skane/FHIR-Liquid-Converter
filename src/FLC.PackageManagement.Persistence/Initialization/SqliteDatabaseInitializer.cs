// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using System.Data.Common;
using System.Reflection;
using Dapper;
using FLC.PackageManagement.Persistence.DbFactory;
using Microsoft.Extensions.Logging;

namespace FLC.PackageManagement.Persistence.Initialization;

public sealed class SqliteDatabaseInitializer : IDatabaseInitializer
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<SqliteDatabaseInitializer> _logger;

    public SqliteDatabaseInitializer(IDbConnectionFactory dbConnectionFactory, ILogger<SqliteDatabaseInitializer> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken ct)
    {
        _logger.LogDebug("Initializing SQLite database...");

        using var connection = _dbConnectionFactory.CreateConnection();

        if (connection is DbConnection dbConn)
        {
            await dbConn.OpenAsync(ct);
        }
        else
        {
            connection.Open();
        }

        // Pragmas recommended on open
        await connection.ExecuteAsync("PRAGMA foreign_keys=ON;", commandTimeout: 30);
        await connection.ExecuteAsync("PRAGMA journal_mode=WAL;", commandTimeout: 30);

        // Load schema SQL (embedded resource)
        var sql = await LoadEmbeddedSqlAsync("Scripts.SQLite.001_CreateTables.sql", ct);

        await connection.ExecuteAsync(sql, commandTimeout: 60);

        _logger.LogDebug("SQLite database initialized.");
    }

    private static async Task<string> LoadEmbeddedSqlAsync(string relativePath, CancellationToken ct)
    {
        var asm = Assembly.GetExecutingAssembly();
        var root = asm.GetName().Name; // Dynamiskt "FLC.PackageManagement.Persistence"

        var fullResourceName = $"{root}.{relativePath}";

        await using var stream = asm.GetManifestResourceStream(fullResourceName)
            ?? throw new FileNotFoundException($"Embedded SQL not found: {fullResourceName}");

        using var reader = new StreamReader(stream);
        var sql = await reader.ReadToEndAsync();

        ct.ThrowIfCancellationRequested();
        return sql;
    }
}
