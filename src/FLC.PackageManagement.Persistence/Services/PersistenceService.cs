// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using FLC.PackageManagement.Models;
using FLC.PackageManagement.Persistence.Exceptions;
using FLC.PackageManagement.Persistence.Interfaces;
using FLC.PackageManagement.Persistence.Mappers;
using FLC.PackageManagement.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FLC.PackageManagement.Persistence.Services;

public class PersistenceService(
    IFlcImplementationGuideRepository implementationGuideRepository,
    IFlcLibraryRepository libraryRepository,
    IFlcStructureMapRepository structureMapRepository,
    ILogger<PersistenceService> logger)
    : IPersistenceService
{
    public async Task UpsertPackageToDatabase(PackageResult packageResult)
    {
        try
        {
            var igEntity = PackageToEntityMapper.MapToFlcImplementationGuide(packageResult);
            var existingIg = await implementationGuideRepository.GetByUrlAndVersionAsync(igEntity.Url, igEntity.Version);
            if (existingIg == null)
            {
                var igId = await implementationGuideRepository.InsertAsync(igEntity);
                var libraryId = await libraryRepository.InsertAsync(PackageToEntityMapper.MapToFlcLibrary(packageResult, igId));

                // TODO: If more than one Library will be used in the future, we somehow need to resolve which libraryId to use for each StructureMap.

                await structureMapRepository.InsertAsync(PackageToEntityMapper.MapToFlcStructureMaps(packageResult, igId, libraryId));
            }
            else
            {
                // ToDo: Fix performance for update flow - only update when needed
                int igId = existingIg.Id;
                igEntity.Id = igId;
                await implementationGuideRepository.UpdateAsync(igEntity);

                var libraryEntity = PackageToEntityMapper.MapToFlcLibrary(packageResult, igId);
                var existingLibrary = await libraryRepository.GetByUrlAndVersionAsync(libraryEntity.Url, libraryEntity.Version);
                int libraryId;
                if (existingLibrary == null)
                {
                    // New Library
                    libraryId = await libraryRepository.InsertAsync(libraryEntity);
                }
                else
                {
                    // Update Library
                    libraryId = existingLibrary.Id;
                    libraryEntity.Id = libraryId;
                    await libraryRepository.UpdateAsync(libraryEntity);
                }

                var structureMapEntities = PackageToEntityMapper.MapToFlcStructureMaps(packageResult, igId, libraryId);
                foreach (var structureMapEntity in structureMapEntities)
                {
                    var existingStructureMap = await structureMapRepository.GetByUrlAndVersionAsync(structureMapEntity.Url, structureMapEntity.Version);
                    if (existingStructureMap == null)
                    {
                        // New structure map
                        await structureMapRepository.InsertAsync(structureMapEntity);
                    }
                    else
                    {
                        // Update structure map
                        structureMapEntity.Id = existingStructureMap.Id;
                        await structureMapRepository.UpdateAsync(structureMapEntity);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed insert package into database.");
            throw new DatabaseException(ex.Message, ex);
        }
    }
}