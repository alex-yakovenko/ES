using MongoDB.Driver;

namespace ES.Api
{
    public static class Mongo
    {
        public static IMongoDatabase ConfigureMongo(IConfiguration configuration)
        {
            var settings = MongoClientSettings.FromConnectionString(configuration["Mongo:ConnectionString"]);
            //settings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
            return new MongoClient(settings).GetDatabase(configuration["Mongo:Database"]);
        }
    }
}
