// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using FLC.PackageManagement.Persistence.Models;
using FLC.PackageManagement.Persistence.Models.Custom;

namespace FLC.PackageManagement.Persistence.Interfaces;

public interface IFlcStructureMapRepository
{
    Task<int> InsertAsync(FlcStructureMap entity);

    Task InsertAsync(IEnumerable<FlcStructureMap> entities);

    Task<FlcStructureMap> GetByIdAsync(int id);

    Task<FlcStructureMap> GetByUrlAndVersionAsync(string url, string version);

    Task<bool> UpdateAsync(FlcStructureMap entity);

    Task<bool> DeleteAsync(int id);

    // Resolve templates by structureMapId
    Task<StructureMapTemplateInfo> GetTemplateInfoByStructureMapIdAsync(int structureMapId);

    // For Api
    Task<ImplementationGuideStructureMapLibraryFlat> GetFlatByIdAsync(int structureMapId);

    Task<ImplementationGuideStructureMapLibraryFlat> GetFlatByUrlAndVersionAsync(string url, string version);

    Task<IReadOnlyList<ImplementationGuideStructureMapLibraryFlat>> GetFlatByImplementationGuideAsync(string implementationGuideUrl, string implementationGuideVersion);

    Task<IReadOnlyList<ImplementationGuideStructureMapLibraryFlat>> GetAllFlatAsync();
}
