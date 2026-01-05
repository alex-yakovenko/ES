using ES.Core;
using ES.Declarations;
using ES.Declarations.Inventory;
using ES.Declarations.Orders;
using ES.Declarations.PlaceOrderSaga;
using ES.Test.EventStorage;
using Eventuous;
using Eventuous.Subscriptions;
using Eventuous.Subscriptions.Context;
using Eventuous.Subscriptions.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ES.Test;

public class UnitTest1(ITestOutputHelper outputHelper) : IntegrationTestBase
{
    public const string Tenant = "Tenant-123";

    [Fact]
    public async Task Test1()
    {
        TypeMap.RegisterKnownEventTypes(typeof(Declarations.Inventory.Events).Assembly);

        var services = GetServiceProvider(outputHelper);

        var placeOrderSagaService = services.GetRequiredService<ICommandService<PlaceOrderSagaState>>();

        var inventoryService = services.GetRequiredService<ICommandService<InventoryState>>();
        var eventReader = services.GetRequiredService<IEventReader>();

        var readPositions = new Dictionary<string, int>();

        var runHandlers = CreateHandlerRunner(services, readPositions, Tenant);

        await inventoryService.Handle(new Declarations.Inventory.Commands.CreateInventoryItem(
            "BOOK-1", "Book #1", 300, Tenant), CancellationToken.None);

        await runHandlers();

        await inventoryService.Handle(new Declarations.Inventory.Commands.ReserveProduct(
            "BOOK-1", 20, "Order #1", Tenant), CancellationToken.None);

        await runHandlers();

        await inventoryService.Handle(new Declarations.Inventory.Commands.ReserveProduct(
            "BOOK-1", 15, "Order #2", Tenant), CancellationToken.None);

        await placeOrderSagaService.Handle(new Declarations.PlaceOrderSaga.Commands.Start(
            "Saga-1", "A-2025-078", "John Doe", new DateOnly(2025, 12, 29),
            [new() { ProductId = "BOOK-1", Quantity = 35 }]
            , Tenant), CancellationToken.None);

        await runHandlers();

        var inventoryState = await eventReader.LoadState<InventoryState>(new StreamName($"Products-BOOK-1"));
        var orderState = await eventReader.LoadState<OrderState>(new StreamName($"Order-A-2025-078"));
    }
}
