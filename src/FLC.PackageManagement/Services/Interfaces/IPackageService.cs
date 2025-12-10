// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
namespace FLC.PackageManagement.Services.Interfaces;

public interface IPackageService
{
    Task LoadPackage(Stream stream, bool allowPackageOverwrite, string fhirPackagesRootPath, CancellationToken cancellationToken = default);
}
