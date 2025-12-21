using ES.Core;
using ES.Test.Aggregates.Inventory;
using ES.Test.Aggregates.Inventory.Commands;
using ES.Test.Aggregates.Orders;
using ES.Test.Aggregates.Orders.Commands;
using ES.Test.EventStorage;
using ES.Test.Sagas.PaceOrder;
using ES.Test.Sagas.PaceOrder.Commands;
using ES.Test.Sagas.PaceOrder.EventCatchers;
using ES.Test.Sagas.PaceOrder.Steps;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ES.Test;

public class UnitTest1(ITestOutputHelper outputHelper) : IntegrationTestBase
{
    public const string Tenant = "Tenant-123";

    [Fact]
    public async Task Test1()
    {
        var services = GetServiceProvider(outputHelper);

        var consumingTasks = CreateConsumingTasks(services, Tenant);

        var serviceProvider = services. CreateScope().ServiceProvider;
        var commandSender = serviceProvider.GetRequiredService<ICommandQueue>();
        var eventStorage = serviceProvider.GetRequiredService<IEsEventStorage>();

        var product1Id = Guid.NewGuid().ToString("N");
        var product2Id = Guid.NewGuid().ToString("N");

        await commandSender.SendCommand(new InventoryCommands.CreateInventoryItemCommand(product1Id, "Product 1", 100)
        {
            TenantId = Tenant,
            StreamType = InventoryEvents.Stream
        });

        await ProcessQueue(consumingTasks);

        await commandSender.SendCommand(new InventoryCommands.CreateInventoryItemCommand(product2Id, "Product 2", 200)
        {
            TenantId = Tenant,
            StreamType = InventoryEvents.Stream
        });

        await ProcessQueue(consumingTasks);

        var sagaId = Guid.NewGuid().ToString("N");
        var customerId = Guid.NewGuid().ToString("N");
        var orderId = Guid.NewGuid().ToString("N");

        await commandSender.SendCommand(new PlaceOrderSaga.Commands.Start(sagaId, orderId, customerId, new DateOnly(2025, 12, 3),
            [
                new() { ProductId = product1Id, Quantity = 50},
                new() { ProductId = product2Id, Quantity = 170},
            ])
        {
            TenantId = Tenant,
            StreamType = InventoryEvents.Stream
        });

        await ProcessQueue(consumingTasks);

        var prod1 = await eventStorage.Load<InventoryItem>(product1Id);
        var prod2 = await eventStorage.Load<InventoryItem>(product2Id);

        var saga = await eventStorage.Load<PlaceOrderSaga>(sagaId);

        var order = await eventStorage.Load<Order>(orderId);

        Assert.Equal(50, prod1.AvailableQuantity);
        Assert.Equal(30, prod2.AvailableQuantity);

        Assert.Equal(OrderStatus.Placed, order.Status);
    }
}
