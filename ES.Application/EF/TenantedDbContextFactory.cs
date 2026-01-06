using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace ES.Application.EF
{
    public class TenantedDbContextFactory(
        Dictionary<string, string> options,
        DbConfig dbConfig) : ITenantedDbContextFactory
    {
        public EsDbContext CreateDbContext(string tenantId)
        {
            var databaseName = options.TryGetValue(tenantId, out var dbName)
                ? dbName
                : throw new ArgumentException($"No database configured for tenant {tenantId}", nameof(tenantId));

            var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder(dbConfig.ConnectionString)
            {
                Database = databaseName
            };

            var builder = new DbContextOptionsBuilder<EsDbContext>()
                .UseNpgsql(connectionStringBuilder.ConnectionString);

            return new EsDbContext(builder.Options);
        }
    }
}
