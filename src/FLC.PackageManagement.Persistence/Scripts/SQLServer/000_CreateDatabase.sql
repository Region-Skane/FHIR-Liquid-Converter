IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'flc-transformer')
BEGIN
    CREATE DATABASE [flc-transformer] COLLATE SQL_Latin1_General_CP1_CI_AS;
END
GO