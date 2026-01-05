using ES.Declarations.PlaceOrderSaga;
using Eventuous;
using Eventuous.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace ES.Api.Controllers;

[Route("api/saga/place-order")]
public class PlaceOrderSagaController(ICommandService<PlaceOrderSagaState> service) : CommandHttpApiBase<PlaceOrderSagaState>(service)
{
    [HttpPost("v1/start")]
    [ProducesResult<PlaceOrderSagaState>]
    [ProducesConflict]
    [ProducesDomainError]
    [ProducesNotFound]
    public async Task<IActionResult?> Start(
        [FromBody] Commands.Start cmd,
        CancellationToken cancellationToken
    )
    {
        var result = await Handle(cmd, cancellationToken);
        return result.Result;
    }

    [HttpPost("v1/mark-reserved")]
    [ProducesResult<PlaceOrderSagaState>]
    [ProducesConflict]
    [ProducesDomainError]
    [ProducesNotFound]
    public async Task<IActionResult?> MarkProductReserved(
        [FromBody] Commands.MarkProductReserved cmd,
        CancellationToken cancellationToken
    )
    {
        var result = await Handle(cmd, cancellationToken);
        return result.Result;
    }

    [HttpPost("v1/mark-failed")]
    [ProducesResult<PlaceOrderSagaState>]
    [ProducesConflict]
    [ProducesDomainError]
    [ProducesNotFound]
    public async Task<IActionResult?> MarkProductReservationFailed(
        [FromBody] Commands.MarkProductReservationFailed cmd,
        CancellationToken cancellationToken
    )
    {
        var result = await Handle(cmd, cancellationToken);
        return result.Result;
    }
}
