using System;
using Microsoft.EntityFrameworkCore;
using HamaraCommerce.Data;

namespace HamaraCommerce.Tests
{
    public static class TestDbContextFactory
    {
        public static ApplicationDbContext CreateInMemoryDbContext(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        public static ApplicationDbContext CreateSqlServerDbContext(string? dbName = null)
        {
            var dbIdentifier = dbName ?? "HamaraCommerce_ConcurrencyTestDb";
            var connStr = GetSqlServerConnectionString(dbIdentifier);
            var sqlOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;
            var sqlContext = new ApplicationDbContext(sqlOptions);

            // Strictly enforce SQL Server: Do NOT silently fall back to InMemory
            sqlContext.Database.EnsureCreated();
            return sqlContext;
        }

        public static string GetSqlServerConnectionString(string? dbName = null)
        {
            var dbIdentifier = dbName ?? "HamaraCommerce_ConcurrencyTestDb";
            var configured = Environment.GetEnvironmentVariable("HAMARA_TEST_SQL_CONNECTION");
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(configured ??
                "Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=15");
            if (!dbIdentifier.StartsWith("HamaraCommerce_", StringComparison.Ordinal))
                dbIdentifier = "HamaraCommerce_" + dbIdentifier;
            builder.InitialCatalog = dbIdentifier;
            return builder.ConnectionString;
        }

        public static (ApplicationDbContext context, bool isSqlServer) CreateTestDbContext(string? dbName = null)
        {
            return (CreateSqlServerDbContext(dbName), true);
        }
    }
}
