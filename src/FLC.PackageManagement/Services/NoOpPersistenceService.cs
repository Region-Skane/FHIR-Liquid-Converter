// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using FLC.PackageManagement.Models;
using FLC.PackageManagement.Services.Interfaces;

namespace FLC.PackageManagement.Services;

public class NoOpPersistenceService : IPersistenceService
{
    public Task UpsertPackageToDatabase(PackageResult packageResult)
    {
        // Empty to ignore persistence when not activated
        return Task.CompletedTask;
    }
}
