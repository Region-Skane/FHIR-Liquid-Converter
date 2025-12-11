// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using FLC.PackageManagement.Persistence.Models;

namespace FLC.PackageManagement.Persistence.Interfaces;

public interface IFlcImplementationGuideRepository
{
    Task<int> InsertAsync(FlcImplementationGuide entity);

    Task<FlcImplementationGuide> GetByIdAsync(int id);

    Task<bool> UpdateAsync(FlcImplementationGuide entity);

    Task<bool> DeleteAsync(int id);

    Task<FlcImplementationGuide> GetByUrlAndVersionAsync(string url, string version);
}
