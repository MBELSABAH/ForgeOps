using ForgeOps.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForgeOps.Web.Controllers;

public class DashboardController(IWorkOrderService service) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await service.GetSummaryAsync(ct));
}
