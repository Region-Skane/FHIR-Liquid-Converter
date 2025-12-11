// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
namespace FLC.PackageManagement.Persistence.Interfaces;

public interface IHasTimestamps
{
    DateTime CreatedAt { get; set; }

    DateTime UpdatedAt { get; set; }

    // Default implementation
    public void SetInsertTimes() => CreatedAt = UpdatedAt = DateTime.UtcNow;

    public void SetUpdateTimes() => UpdatedAt = DateTime.UtcNow;
}
