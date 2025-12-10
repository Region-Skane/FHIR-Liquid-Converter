// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using System.Data;
using Microsoft.Data.SqlClient;

namespace FLC.PackageManagement.Persistence.DbFactory;

public class SqlServerConnectionFactory(string connectionString)
    : IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        return new SqlConnection(connectionString);
    }
}