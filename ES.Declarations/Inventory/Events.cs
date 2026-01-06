using ES.Core;
using ES.Declarations;
using Eventuous;
using System;

namespace ES.Declarations.Inventory
{
    public static class Events
    {
        public const string AggregateName = "Inventory";

        [EventType($"V1.{AggregateName}.{nameof(InventoryItemCreated)}")]
        public record InventoryItemCreated(
            string ProductId,
            int InitialQuantity,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(ProductReserved)}")]
        public record ProductReserved(
            string ProductId,
            int Quantity,
            string OrderId,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(ProductReservationFailed)}")]
        public record ProductReservationFailed(
            string ProductId,
            int Quantity,
            string OrderId,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(ProductReservationCanceled)}")]
        public record ProductReservationCanceled(
            string ProductId,
            string OrderId,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;

        [EventType($"V1.{AggregateName}.{nameof(ProductReleased)}")]
        public record ProductReleased(
            string ProductId,
            int Quantity,
            string OrderId,
            string TenantId,
            string? CorrelationId = null
        ) : IMessageContext;
    }
}
