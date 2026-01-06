using ES.Api;
using ES.Api.Config;
using ES.Application;
using ES.Application.EF;
using ES.Declarations.Inventory;
using ES.Declarations.Orders;
using ES.Declarations.PlaceOrderSaga;
using ES.Declarations.UpdateOrderSaga;
using Eventuous;
using Eventuous.EventStore;
using Eventuous.EventStore.Subscriptions;
using Eventuous.Postgresql;
using Eventuous.Postgresql.Subscriptions;
using Eventuous.Projections.MongoDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using MongoDB.Driver.Core.Configuration;
using Npgsql.TypeMapping;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();



builder.Services.AddEventStoreClient("esdb://localhost:2113?tls=false");
// builder.Services.AddEventuous(esdb => { esdb.AddEventStore<EsdbEventStore>(); });
builder.Services.AddEventStore<EsdbEventStore>();

builder.Services.AddCommandService<OrderService, OrderState>();
builder.Services.AddCommandService<InventoryService, InventoryState>();
builder.Services.AddCommandService<PlaceOrderSagaService, PlaceOrderSagaState>();
builder.Services.AddCommandService<UpdateOrderSagaService, UpdateOrderSagaState>();

builder.Services.AddSingleton(Mongo.ConfigureMongo(builder.Configuration));

builder.Services.AddSubscription<AllStreamSubscription, AllStreamSubscriptionOptions>(
    "SagaSubscription",
    builder => builder
        .UseCheckpointStore<MongoCheckpointStore>()
        .AddEventHandler<PlaceOrderSaga>()
        .AddEventHandler<UpdateOrderSaga>()
        .AddEventHandler<OrdersProjectionBuilder>()
        .AddEventHandler<InventoryProjectionBuilder>()
);

TypeMap.RegisterKnownEventTypes(typeof(ES.Declarations.Orders.Events).Assembly);

builder.Services.Configure<DbConfig>(builder.Configuration.GetSection("PostgreSQL"));

var tenants = new Dictionary<string, string>();
builder.Configuration.GetSection("Tenants").Bind(tenants);

var dbConfig = builder.Configuration.GetSection("PostgreSQL").Get<DbConfig>();
var dbContextFactory = new TenantedDbContextFactory(tenants, dbConfig);
builder.Services.AddSingleton<ITenantedDbContextFactory>(dbContextFactory);

foreach (var tenant in tenants.Keys)
{
    using var dbContext = dbContextFactory.CreateDbContext(tenant);
    dbContext.Database.Migrate();
}

var app = builder.Build();

// Enable Swagger with OpenAPI 3.1
app.UseSwagger(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
});
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();


// TODO: Map commands using correct Eventuous API for version 0.15.1
// app.MapDiscoveredCommands<OrderState>(typeof(OrderService).Assembly);
// app.MapDiscoveredCommands<InventoryState>(typeof(InventoryService).Assembly);
// app.MapDiscoveredCommands<PlaceOrderSagaState>(typeof(PlaceOrderSagaService).Assembly);
// app.MapDiscoveredCommands<UpdateOrderSagaState>(typeof(UpdateOrderSagaService).Assembly);

app.Run();

