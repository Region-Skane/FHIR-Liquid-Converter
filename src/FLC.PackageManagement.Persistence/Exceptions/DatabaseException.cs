// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
namespace FLC.PackageManagement.Persistence.Exceptions;

public class DatabaseException(string message, Exception inner = null)
    : Exception(message, inner)
{
}
