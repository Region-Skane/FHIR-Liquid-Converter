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

public class FlcLibraryRepository(IDbConnectionFactory factory)
    : DapperRepositoryBase(factory), IFlcLibraryRepository
{
    public async Task<int> InsertAsync(FlcLibrary entity)
    {
        ((IHasTimestamps)entity).SetInsertTimes();

        using var connection = OpenConnection();

        return await connection.InsertAsync(entity);
    }

    public async Task<FlcLibrary> GetByIdAsync(int id)
    {
        using var connection = OpenConnection();

        return await connection.GetAsync<FlcLibrary>(id);
    }

    public async Task<bool> UpdateAsync(FlcLibrary entity)
    {
        ((IHasTimestamps)entity).SetUpdateTimes();

        using var connection = OpenConnection();

        return await connection.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = OpenConnection();

        return await connection.DeleteAsync(new FlcLibrary { Id = id });
    }

    /// <summary>
    /// Get one FlcLibrary (filter on URL+Version)
    /// Returnerar null om ingen match hittas.
    /// </summary>
    /// <param name="url">FlcLibrary URL</param>
    /// <param name="version">FlcLibrary Version</param>
    public async Task<FlcLibrary> GetByUrlAndVersionAsync(string url, string version)
    {
        const string sql = """
            SELECT * 
            FROM flcLibrary l
            WHERE l.Url = @Url AND l.Version = @Version
        """;

        using var connection = OpenConnection();

        return await connection.QueryFirstOrDefaultAsync<FlcLibrary>(sql, new { Url = url, Version = version });
    }
}
