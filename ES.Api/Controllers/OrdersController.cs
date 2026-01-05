using ES.Declarations.Orders;
using Eventuous;
using Eventuous.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace ES.Api.Controllers;

[Route("api/orders")]
public class OrdersController(ICommandService<OrderState> service) : CommandHttpApiBase<OrderState>(service)
{
    [HttpPost("v1/order")]
    [ProducesResult<OrderState>]
    [ProducesConflict]
    [ProducesDomainError]
    [ProducesNotFound]
    public async Task<IActionResult?> DraftOrder(
        [FromBody] Commands.DraftOrder cmd,
        CancellationToken cancellationToken
    )
    {
        var result = await Handle(cmd, cancellationToken);
        return result.Result;
    }

    [HttpPost("v1/place")]
    [ProducesResult<OrderState>]
    [ProducesConflict]
    [ProducesDomainError]
    [ProducesNotFound]
    public async Task<IActionResult?> SetOrderPlaced(
        [FromBody] Commands.SetOrderPlaced cmd,
        CancellationToken cancellationToken
    )
    {
        var result = await Handle(cmd, cancellationToken);
        return result.Result;
    }

    [HttpPost("v1/cancel")]
    [ProducesResult<OrderState>]
    [ProducesConflict]
    [ProducesDomainError]
    [ProducesNotFound]
    public async Task<IActionResult?> CancelOrder(
        [FromBody] Commands.CancelOrder cmd,
        CancellationToken cancellationToken
    )
    {
        var result = await Handle(cmd, cancellationToken);
        return result.Result;
    }

    [HttpPost("v1/adjust")]
    [ProducesResult<OrderState>]
    [ProducesConflict]
    [ProducesDomainError]
    [ProducesNotFound]
    public async Task<IActionResult?> AdjustProducts(
        [FromBody] Commands.AdjustProducts cmd,
        CancellationToken cancellationToken
    )
    {
        var result = await Handle(cmd, cancellationToken);
        return result.Result;
    }
}
