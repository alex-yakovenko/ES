using ES.Declarations.Inventory;
using Eventuous;
using Eventuous.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace ES.Api.Controllers
{
    public class InventoryController(ICommandService<InventoryState> service)
        : CommandHttpApiBase<InventoryState>(service)
    {
        [HttpPost("v1/create")]
        [ProducesResult<InventoryState>]
        [ProducesConflict]
        [ProducesDomainError]
        [ProducesNotFound]
        public async Task<IActionResult?> RegisterPayment(
            [FromBody] Commands.CreateInventoryItem cmd,
            CancellationToken cancellationToken
        )
        {
            var result = await Handle(cmd, cancellationToken);

            return result.Result;
        }

        [HttpPost("v1/reserve")]
        [ProducesResult<InventoryState>]
        [ProducesConflict]
        [ProducesDomainError]
        [ProducesNotFound]
        public async Task<IActionResult?> ReserveProduct(
            [FromBody] Commands.ReserveProduct cmd,
            CancellationToken cancellationToken
        )
        {
            var result = await Handle(cmd, cancellationToken);
            return result.Result;
        }

        [HttpPost("v1/cancel-reservation")]
        [ProducesResult<InventoryState>]
        [ProducesConflict]
        [ProducesDomainError]
        [ProducesNotFound]
        public async Task<IActionResult?> CancelProductReservation(
            [FromBody] Commands.CancelProductReservation cmd,
            CancellationToken cancellationToken
        )
        {
            var result = await Handle(cmd, cancellationToken);
            return result.Result;
        }

        [HttpPost("v1/release")]
        [ProducesResult<InventoryState>]
        [ProducesConflict]
        [ProducesDomainError]
        [ProducesNotFound]
        public async Task<IActionResult?> ReleaseProduct(
            [FromBody] Commands.ReleaseProduct cmd,
            CancellationToken cancellationToken
        )
        {
            var result = await Handle(cmd, cancellationToken);
            return result.Result;
        }
    }
}
