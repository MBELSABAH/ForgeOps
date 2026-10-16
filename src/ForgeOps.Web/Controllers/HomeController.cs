using Microsoft.AspNetCore.Mvc;

namespace ForgeOps.Web.Controllers;

public class HomeController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult HttpError(int code)
    {
        Response.StatusCode = code;
        return View(code);
    }
}
