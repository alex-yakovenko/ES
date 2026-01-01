using ES.Core;
using ES.Declarations;
using ES.Declarations.Inventory;
using ES.Declarations.Orders;
using ES.Declarations.PlaceOrderSaga;
using ES.Declarations.UpdateOrderSaga;
using Eventuous;
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
        private readonly Func<Task> runHandlers;
        private readonly Dictionary<string, int> readPositions = [];
        private readonly ICommandService<PlaceOrderSagaState> placeOrderSagaService;
        private readonly ICommandService<UpdateOrderSagaState> updateOrderSagaService;
        private readonly ICommandService<InventoryState> inventoryService;
        private readonly IEventReader eventReader;

        public StepDefinitions(ITestOutputHelper outputHelper)
        {
            services = GetServiceProvider(outputHelper)!;

            runHandlers = CreateHandlerRunner(services, readPositions, Tenant);

            placeOrderSagaService = services.GetRequiredService<ICommandService<PlaceOrderSagaState>>();
            updateOrderSagaService = services.GetRequiredService<ICommandService<UpdateOrderSagaState>>();
            inventoryService = services.GetRequiredService<ICommandService<InventoryState>>();
            eventReader = services.GetRequiredService<IEventReader>();

        }
         
        [Given("All flows are configured")]
        public void GivenAllFlowsAreConfigured()
        {

        }


        [Given("product with SKU {string} and available quantity {int} is present in inventory")]
        public async Task GivenProductWithSKUAndAvailableQuantityIsPresentInInventory(string sku, int quantity)
        {
            await inventoryService.Handle(new Declarations.Inventory.Commands.CreateInventoryItem(sku, sku, quantity, new MessageContext 
            { 
                TenantId = Tenant
            }), CancellationToken.None);

            await runHandlers();
        }

        [Given("having placed order for {string}, date {string}, ID {string} and products as follows:")]
        [When( "placing order for {string}, date {string}, ID {string} and products as follows:")]
        public async Task WhenPlacingOrderForDateIDAndProductsAsFollows(string customer, string date, string orderId, DataTable dataTable)
        {
            var command = new Declarations.PlaceOrderSaga.Commands.Start($"place-order-{orderId}", orderId, 
                customer, DateOnly.Parse(date), [], new MessageContext(Tenant));

            foreach (var row in dataTable.Rows)
                command.Items.Add(new()
                {
                    ProductId = row[0],
                    Quantity = int.Parse(row[1])
                });

            await placeOrderSagaService.Handle(command, CancellationToken.None);

            await runHandlers();
        }

        [When("updating order {string} product changes as follows:")]
        public async Task WhenUpdatingOrderProductChangesAsFollows(string orderId, DataTable dataTable)
        {
            var command = new Declarations.UpdateOrderSaga.Commands.Start($"update-order-{orderId}", 
                orderId, [], new MessageContext(Tenant));

            foreach (var row in dataTable.Rows)
                command.Changes.Add(new(row[0], int.Parse(row[1])));

            await updateOrderSagaService.Handle(command, CancellationToken.None);

            await runHandlers();
        }


        [Then("product {string} available quantity becomes {int}")]
        public async Task ThenProductAvailableQuantityBecomes(string sku, int expectedQuantity)
        {
            var product = await eventReader.LoadState<InventoryState>(new StreamName($"{nameof(InventoryAggregate)}-{sku}"));

            Assert.Equal(expectedQuantity, product.State.AvailableQuantity);
        }

        [Then("order {string} has status {string} with items as follows:")]
        public async Task ThenOrderHasStatus(string orderId, string expectedStatus, DataTable dataTable)
        {
            var order = (await eventReader.LoadState<OrderState>(new StreamName($"{nameof(OrderAggregate)}-{orderId}"))).State;
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
