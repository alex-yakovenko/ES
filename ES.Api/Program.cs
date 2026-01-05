using ES.Application.Inventory;
using ES.Application.Orders;
using ES.Application.Sagas;
using ES.Declarations.Inventory;
using ES.Declarations.Orders;
using ES.Declarations.PlaceOrderSaga;
using ES.Declarations.UpdateOrderSaga;
using Eventuous;
using Eventuous.EventStore;
using Eventuous.EventStore.Subscriptions;
using Eventuous.Projections.MongoDB;
using Microsoft.OpenApi;
using ES.Api;

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
// .UseCheckpointStore(...) // Missing checkpoint store for now, will start from beginning or head
);

TypeMap.RegisterKnownEventTypes(typeof(ES.Declarations.Orders.Events).Assembly);

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

