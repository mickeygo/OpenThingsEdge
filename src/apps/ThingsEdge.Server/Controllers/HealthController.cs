using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace ThingsEdge.Server.Controllers;

/// <summary>
/// 健康检测接口
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HealthController : Controller
{
    [HttpGet, HttpPost]
    [Route("")]
    public IActionResult Index()
    {
        return Ok();
    }
}
