using ES.Core;
using ES.Test.Aggregates.Inventory;
using ES.Test.Aggregates.Orders;
using ES.Test.Sagas.PaceOrder;
using ES.Test.Sagas.UpdateOrder;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using System;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ES.Test.Scenarios
{
    [Binding]
    public class StepDefinitions
        : IntegrationTestBase
    {
        public const string Tenant = "Tenant-123";

        private readonly IServiceProvider services;
        private readonly List<(string consumer, string kind, Func<Task<int>> func)> consumingTasks;
        private readonly ICommandQueue commandSender;
        private readonly IEsEventStorage eventStorage;

        public StepDefinitions(ITestOutputHelper outputHelper)
        {
            services = GetServiceProvider(outputHelper)!;
            consumingTasks = CreateConsumingTasks(services, Tenant);
            commandSender = services.GetRequiredService<ICommandQueue>();
            eventStorage = services.GetRequiredService<IEsEventStorage>();
        }

        [Given("All flows are configured")]
        public void GivenAllFlowsAreConfigured()
        {

        }


        [Given("product with SKU {string} and available quantity {int} is present in inventory")]
        public async Task GivenProductWithSKUAndAvailableQuantityIsPresentInInventory(string sku, int quantity)
        {
            await commandSender.SendCommand(new InventoryItem.Commands.CreateInventoryItemCommand(sku, sku, quantity)
            {
                TenantId = Tenant,
                StreamType = InventoryItem.Stream
            });

            await ProcessQueue(consumingTasks);
        }

        [Given("having placed order for {string}, date {string}, ID {string} and products as follows:")]
        [When( "placing order for {string}, date {string}, ID {string} and products as follows:")]
        public async Task WhenPlacingOrderForDateIDAndProductsAsFollows(string customer, string date, string orderId, DataTable dataTable)
        {
            var command = new PlaceOrderSaga.Commands.Start($"place-order-{orderId}", orderId, customer, DateOnly.Parse(date), [])
            {
                TenantId = Tenant,
                StreamType = InventoryItem.Stream
            };

            foreach (var row in dataTable.Rows)
                command.Items.Add(new()
                {
                    ProductId = row[0],
                    Quantity = int.Parse(row[1])
                });

            await commandSender.SendCommand(command);

            await ProcessQueue(consumingTasks);
        }

        [When("updating order {string} product changes as follows:")]
        public async Task WhenUpdatingOrderProductChangesAsFollows(string orderId, DataTable dataTable)
        {
            var command = new UpdateOrderSaga.Commands.Start($"update-order-{orderId}", orderId, [])
            {
                TenantId = Tenant,
                StreamType = InventoryItem.Stream
            };

            foreach (var row in dataTable.Rows)
                command.Changes.Add(new(row[0], int.Parse(row[1])));

            await commandSender.SendCommand(command);

            await ProcessQueue(consumingTasks);
        }


        [Then("product {string} available quantity becomes {int}")]
        public async Task ThenProductAvailableQuantityBecomes(string sku, int expectedQuantity)
        {
            var product = await eventStorage.Load<InventoryItem>(sku);

            Assert.Equal(expectedQuantity, product.AvailableQuantity);
        }

        [Then("order {string} has status {string} with items as follows:")]
        public async Task ThenOrderHasStatus(string orderId, string expectedStatus, DataTable dataTable)
        {
            var order = await eventStorage.Load<Order>(orderId);
            Assert.Equal(dataTable.RowCount, order.Items.Count);

            for (var i = 0; i < order.Items.Count; i++)
            {
                Assert.Equal(dataTable.Rows[i][0], order.Items[i].ProductId);
                Assert.Equal(int.Parse(dataTable.Rows[i][1]), order.Items[i].Quantity);
            }

            Assert.Equal(expectedStatus, order.Status);
        }

    }
}
