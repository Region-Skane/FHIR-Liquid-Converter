// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
namespace FLC.PackageManagement.Persistence.Initialization;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken ct);
}
