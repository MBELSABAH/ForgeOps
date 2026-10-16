using ForgeOps.Web.Contracts;
using ForgeOps.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForgeOps.Web.Controllers.Api;

[ApiController]
[Route("api/dashboard")]
public class DashboardApiController(IWorkOrderService service) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> Summary(CancellationToken ct) =>
        Ok(await service.GetSummaryAsync(ct));
}
