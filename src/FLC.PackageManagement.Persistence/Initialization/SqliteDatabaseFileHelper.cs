using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace FLC.PackageManagement.Persistence.Initialization;

public static class SqliteDatabaseFileHelper
{
    public static void EnsureSqliteFolder(IConfiguration configuration, string contentRootPath)
    {
        var connectionString = configuration["PackageManagement:ConnectionStrings:SQLite"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("SQLite connection string not configured (PackageManagement:ConnectionStrings:SQLite).");
        }

        var sb = new SqliteConnectionStringBuilder(connectionString);
        var dataSource = (sb.DataSource ?? string.Empty).Trim();

        // ignore in-memory databases
        if (dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string resolvedPath;
        if (dataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            // strip "file:" and any query
            var pathPart = dataSource.Substring(5).Split('?', '#')[0];
            resolvedPath = Path.IsPathRooted(pathPart)
                ? pathPart
                : Path.GetFullPath(Path.Combine(contentRootPath, pathPart));
        }
        else
        {
            // plain path
            resolvedPath = Path.IsPathRooted(dataSource)
                ? dataSource
                : Path.GetFullPath(Path.Combine(contentRootPath, dataSource));
        }

        var dir = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}
