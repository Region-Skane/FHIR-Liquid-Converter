// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using Dapper;
using Dapper.Contrib.Extensions;
using FLC.PackageManagement.Persistence.DbFactory;
using FLC.PackageManagement.Persistence.Interfaces;
using FLC.PackageManagement.Persistence.Models;

namespace FLC.PackageManagement.Persistence.Repositories;

public class FlcImplementationGuideRepository(IDbConnectionFactory factory)
    : DapperRepositoryBase(factory), IFlcImplementationGuideRepository
{
    public async Task<int> InsertAsync(FlcImplementationGuide entity)
    {
        ((IHasTimestamps)entity).SetInsertTimes();

        using var connection = OpenConnection();

        return await connection.InsertAsync(entity);
    }

    public async Task<FlcImplementationGuide> GetByIdAsync(int id)
    {
        using var connection = OpenConnection();

        return await connection.GetAsync<FlcImplementationGuide>(id);
    }

    public async Task<bool> UpdateAsync(FlcImplementationGuide entity)
    {
        ((IHasTimestamps)entity).SetUpdateTimes();

        using var connection = OpenConnection();

        return await connection.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = OpenConnection();

        return await connection.DeleteAsync(new FlcImplementationGuide { Id = id });
    }

    /// <summary>
    /// Get one FlcImplementationGuide (filter on URL+Version)
    /// Returnerar null om ingen match hittas.
    /// </summary>
    /// <param name="url">Implementation Guide URL</param>
    /// <param name="version">Implementation Guide Version</param>
    public async Task<FlcImplementationGuide> GetByUrlAndVersionAsync(string url, string version)
    {
        const string sql = """
            SELECT * 
            FROM flcImplementationGuide ig 
            WHERE ig.Url = @Url AND ig.Version = @Version
        """;

        using var connection = OpenConnection();

        return await connection.QueryFirstOrDefaultAsync<FlcImplementationGuide>(sql, new { Url = url, Version = version });
    }
}
