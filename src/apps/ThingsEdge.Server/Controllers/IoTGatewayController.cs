using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace ThingsEdge.Server.Controllers;

/// <summary>
/// IOT 网关接口。
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class IoTGatewayController : Controller
{
    [HttpPost]
    [Route("")]
    public IActionResult Post()
    {
        return Ok();
    }
}
