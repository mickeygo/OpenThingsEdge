using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThingsEdge.Contracts;
using ThingsEdge.Router;
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
    private readonly IExchange _exchange;

    public IoTGatewayController(IExchange exchange)
    {
        _exchange = exchange;
    }

    [HttpGet]
    [Route("start")]
    public async Task<IActionResult> Start()
    {
        await _exchange.StartAsync();
        return Ok("Exchange started.");
    }

    [HttpPost]
    [Route("notice")]
    public IActionResult Notice()
    {
        var ret = new HttpResponseResult();
        return Json(ret);
    }

    [HttpPost]
    [Route("trigger")]
    public IActionResult Trigger(RequestMessage message)
    {
        var ret = new HttpResponseResult();
        return Json(ret);
    }
}
