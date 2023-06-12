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
    private readonly ILogger _logger;

    public IoTGatewayController(ILogger<IoTGatewayController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [Route("")]
    public IActionResult Post(RequestMessage message)
    {
        var ret = new HttpResponseResult();
        return Json(ret);
    }
}
