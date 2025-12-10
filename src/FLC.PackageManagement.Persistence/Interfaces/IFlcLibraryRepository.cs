// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using FLC.PackageManagement.Persistence.Models;

namespace FLC.PackageManagement.Persistence.Interfaces;

public interface IFlcLibraryRepository
{
    Task<int> InsertAsync(FlcLibrary entity);

    Task<FlcLibrary> GetByIdAsync(int id);

    Task<bool> UpdateAsync(FlcLibrary entity);

    Task<bool> DeleteAsync(int id);

    Task<FlcLibrary> GetByUrlAndVersionAsync(string url, string version);
}
