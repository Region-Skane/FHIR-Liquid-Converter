// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using Dapper;
using Dapper.Contrib.Extensions;
using FLC.PackageManagement.Persistence.DbFactory;
using FLC.PackageManagement.Persistence.Interfaces;
using FLC.PackageManagement.Persistence.Models;
using FLC.PackageManagement.Persistence.Models.Custom;

namespace FLC.PackageManagement.Persistence.Repositories;

public class FlcStructureMapRepository(IDbConnectionFactory factory)
    : DapperRepositoryBase(factory), IFlcStructureMapRepository
{
    public async Task<int> InsertAsync(FlcStructureMap entity)
    {
        ((IHasTimestamps)entity).SetInsertTimes();

        using var connection = OpenConnection();

        return await connection.InsertAsync(entity);
    }

    public async Task InsertAsync(IEnumerable<FlcStructureMap> entities)
    {
        using var connection = OpenConnection();

        using var transaction = connection.BeginTransaction();
        foreach (var entity in entities)
        {
            ((IHasTimestamps)entity).SetInsertTimes();
            await connection.InsertAsync(entity, transaction);
        }

        transaction.Commit();
    }

    public async Task<FlcStructureMap> GetByIdAsync(int id)
    {
        using var connection = OpenConnection();

        return await connection.GetAsync<FlcStructureMap>(id);
    }

    public async Task<bool> UpdateAsync(FlcStructureMap entity)
    {
        ((IHasTimestamps)entity).SetUpdateTimes();

        using var connection = OpenConnection();

        return await connection.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = OpenConnection();

        return await connection.DeleteAsync(new FlcStructureMap { Id = id });
    }

    /// <summary>
    /// Get one FlcStructureMap (filter on URL+Version)
    /// Return null if no match was found.
    /// </summary>
    /// <param name="url">StructureMap URL</param>
    /// <param name="version">StructureMap Version</param>
    public async Task<FlcStructureMap> GetByUrlAndVersionAsync(string url, string version)
    {
        const string sql = """
            SELECT * 
            FROM flcStructureMap sm 
            WHERE sm.Url = @Url AND sm.Version = @Version
        """;

        using var connection = OpenConnection();

        return await connection.QueryFirstOrDefaultAsync<FlcStructureMap>(sql, new { Url = url, Version = version });
    }

    /// <summary>
    /// Get one StructureMap by id with included Ig and Library.
    /// </summary>
    /// <param name="structureMapId">Database id of StructureMap</param>
    public async Task<ImplementationGuideStructureMapLibraryFlat> GetFlatByIdAsync(int structureMapId)
    {
        const string sql = """
            SELECT 
                ig.Id      AS FlcImplementationGuideId,
                ig.Url     AS ImplementationGuideUrl,
                ig.Version AS ImplementationGuideVersion,

                sm.Id      AS FlcStructureMapId,
                sm.Url     AS StructureMapUrl,
                sm.Version AS StructureMapVersion,

                l.Id       AS FlcLibraryId,
                l.Url      AS LibraryUrl,
                l.Version  AS LibraryVersion
            FROM flcStructureMap sm
            INNER JOIN flcImplementationGuide ig ON sm.FlcImplementationGuideId = ig.Id
            INNER JOIN flcLibrary l              ON sm.FlcLibraryId = l.Id
            WHERE sm.Id = @StructureMapId
            LIMIT 1;
        """;

        using var connection = OpenConnection();
        return await connection.QueryFirstOrDefaultAsync<ImplementationGuideStructureMapLibraryFlat>(
            sql, new { StructureMapId = structureMapId });
    }

    /// <summary>
    /// Get one StructureMap (filter on URL+Version) with ImplementationGuide and Library as flat model.
    /// </summary>
    /// <param name="url">StructureMap url</param>
    /// <param name="version">StructureMap version</param>
    public async Task<ImplementationGuideStructureMapLibraryFlat> GetFlatByUrlAndVersionAsync(string url, string version)
    {
        const string sql = """
            SELECT 
                ig.Id   AS FlcImplementationGuideId,
                ig.Url  AS ImplementationGuideUrl,
                ig.Version AS ImplementationGuideVersion,

                sm.Id   AS FlcStructureMapId,
                sm.Url  AS StructureMapUrl,
                sm.Version AS StructureMapVersion,

                l.Id    AS FlcLibraryId,
                l.Url   AS LibraryUrl,
                l.Version AS LibraryVersion
            FROM flcStructureMap sm
            INNER JOIN flcImplementationGuide ig ON sm.FlcImplementationGuideId = ig.Id
            INNER JOIN flcLibrary l ON sm.FlcLibraryId = l.Id
            WHERE sm.Url = @Url AND sm.Version = @Version            
            LIMIT 1;
        """;

        using var connection = OpenConnection();
        return await connection.QueryFirstOrDefaultAsync<ImplementationGuideStructureMapLibraryFlat>(
            sql, new { Url = url, Version = version });
    }

    /// <summary>
    /// Get all StructureMaps for an ImplementationGuide (Url+Version) with Libraries as flat list.
    /// </summary>
    public async Task<IReadOnlyList<ImplementationGuideStructureMapLibraryFlat>> GetAllFlatAsync()
    {
        const string sql = """
            SELECT 
                ig.Id   AS FlcImplementationGuideId,
                ig.Url  AS ImplementationGuideUrl,
                ig.Version AS ImplementationGuideVersion,

                sm.Id   AS FlcStructureMapId,
                sm.Url  AS StructureMapUrl,
                sm.Version AS StructureMapVersion,

                l.Id    AS FlcLibraryId,
                l.Url   AS LibraryUrl,
                l.Version AS LibraryVersion
            FROM flcStructureMap sm
            INNER JOIN flcLibrary l ON sm.FlcLibraryId = l.Id
            INNER JOIN flcImplementationGuide ig ON sm.FlcImplementationGuideId = ig.Id
            ORDER BY sm.Url, sm.Version
        """;

        using var connection = OpenConnection();

        var rows = await connection.QueryAsync<ImplementationGuideStructureMapLibraryFlat>(sql);

        return rows.ToList();
    }

    /// <summary>
    /// Get all StructureMaps for an ImplementationGuide (Url+Version) with Libraries as flat list.
    /// </summary>
    /// <param name="implementationGuideUrl">ImplementationGuide url</param>
    /// <param name="implementationGuideVersion">ImplementationGuide version</param>
    public async Task<IReadOnlyList<ImplementationGuideStructureMapLibraryFlat>> GetFlatByImplementationGuideAsync(
        string implementationGuideUrl, string implementationGuideVersion)
    {
        const string sql = """
            SELECT 
                ig.Id   AS FlcImplementationGuideId,
                ig.Url  AS ImplementationGuideUrl,
                ig.Version AS ImplementationGuideVersion,

                sm.Id   AS FlcStructureMapId,
                sm.Url  AS StructureMapUrl,
                sm.Version AS StructureMapVersion,

                l.Id    AS FlcLibraryId,
                l.Url   AS LibraryUrl,
                l.Version AS LibraryVersion
            FROM flcStructureMap sm
            INNER JOIN flcLibrary l ON sm.FlcLibraryId = l.Id
            INNER JOIN flcImplementationGuide ig ON sm.FlcImplementationGuideId = ig.Id
            WHERE ig.Url = @Url AND ig.Version = @Version
            ORDER BY sm.Url, sm.Version
        """;

        using var connection = OpenConnection();

        var rows = await connection.QueryAsync<ImplementationGuideStructureMapLibraryFlat>(
            sql, new { Url = implementationGuideUrl, Version = implementationGuideVersion });

        return rows.ToList();
    }

    /// <summary>
    /// Retrieves StructureMap template information for the given StructureMap Id.
    /// Joins FlcStructureMap with FlcLibrary to return template-related details.
    /// </summary>
    /// <param name="structureMapId">The Id of the StructureMap.</param>
    /// <returns>A StructureMapTemplateInfo object containing mapping details, or null if not found.</returns>
    public async Task<StructureMapTemplateInfo> GetTemplateInfoByStructureMapIdAsync(int structureMapId)
    {
        const string sql = """
            SELECT 
                sm.Id AS FlcStructureMapId,
                sm.EntryTemplate AS EntryTemplate,
                l.StorageRoot AS TemplatesRoot
            FROM flcStructureMap sm
            INNER JOIN flcLibrary l ON sm.FlcLibraryId = l.Id
            WHERE sm.Id = @StructureMapId
            LIMIT 1;
        """;

        using var connection = OpenConnection();

        return await connection.QueryFirstOrDefaultAsync<StructureMapTemplateInfo>(
            sql,
            new { StructureMapId = structureMapId });
    }
}
