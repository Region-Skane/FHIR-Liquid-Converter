// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using System.Data;

namespace FLC.PackageManagement.Persistence.DbFactory;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}