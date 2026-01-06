namespace ES.Application.EF
{
    public interface ITenantedDbContextFactory
    {
        EsDbContext CreateDbContext(string tenantId);
    }
}