// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using FLC.PackageManagement.Models;

namespace FLC.PackageManagement.Services.Interfaces;

public interface IExtractService
{
    Task<PackageResult> ExtractPackage(Stream stream, string igFoldersRootPath, CancellationToken cancellationToken = default);
}
