// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using System.Data;
using FLC.PackageManagement.Persistence.DbFactory;

namespace FLC.PackageManagement.Persistence.Repositories;

public abstract class DapperRepositoryBase(IDbConnectionFactory factory)
{
    protected IDbConnection OpenConnection()
    {
        var connection = factory.CreateConnection();
        connection.Open();
        return connection;
    }
}
