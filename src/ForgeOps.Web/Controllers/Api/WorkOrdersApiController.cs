using ForgeOps.Web.Contracts;
using ForgeOps.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForgeOps.Web.Controllers.Api;

[ApiController]
[Route("api/workorders")]
[Produces("application/json")]
public class WorkOrdersApiController(IWorkOrderService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkOrderDto>>> List([FromQuery] WorkOrderQuery query, CancellationToken ct) =>
        Ok(await service.ListAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkOrderDto>> Get(int id, CancellationToken ct)
    {
        var order = await service.GetAsync(id, ct);
        return order is null ? NotFound(new ProblemDetails { Status = 404, Title = "Work order not found." }) : Ok(order);
    }

    [HttpPost, Consumes("application/json")]
    [ProducesResponseType<WorkOrderDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<WorkOrderDto>> Create(WorkOrderInput input, CancellationToken ct)
    {
        var order = await service.CreateAsync(input, ct);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpPatch("{id:int}/status"), Consumes("application/json")]
    public async Task<ActionResult<WorkOrderDto>> ChangeStatus(int id, StatusChangeRequest input, CancellationToken ct) =>
        Ok(await service.ChangeStatusAsync(id, input, ct));
}
