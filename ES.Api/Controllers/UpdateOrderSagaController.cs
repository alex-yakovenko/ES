using ES.Declarations.UpdateOrderSaga;
using Eventuous;
using Eventuous.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace ES.Api.Controllers;

[Route("api/saga/update-order")]
public class UpdateOrderSagaController(ICommandService<UpdateOrderSagaState> service) : CommandHttpApiBase<UpdateOrderSagaState>(service)
{
    [HttpPost("v1/start")]
    [ProducesResult<UpdateOrderSagaState>]
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

    [HttpPost("v1/mark-reserved-released")]
    [ProducesResult<UpdateOrderSagaState>]
    [ProducesConflict]
    [ProducesDomainError]
    [ProducesNotFound]
    public async Task<IActionResult?> MarkProductReservedOrReleased(
        [FromBody] Commands.MarkProductReservedOrReleased cmd,
        CancellationToken cancellationToken
    )
    {
        var result = await Handle(cmd, cancellationToken);
        return result.Result;
    }

    [HttpPost("v1/mark-failed")]
    [ProducesResult<UpdateOrderSagaState>]
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

    [HttpPost("v1/update-status")]
    [ProducesResult<UpdateOrderSagaState>]
    [ProducesConflict]
    [ProducesDomainError]
    [ProducesNotFound]
    public async Task<IActionResult?> UpdateStatus(
        [FromBody] Commands.UpdateStatus cmd,
        CancellationToken cancellationToken
    )
    {
        var result = await Handle(cmd, cancellationToken);
        return result.Result;
    }
}
