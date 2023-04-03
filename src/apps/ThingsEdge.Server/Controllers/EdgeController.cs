using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace ThingsEdge.Server.Controllers;

[Route("/api/{template}")]
public class EdgeController : Controller
{
    [Route("")]
    public IActionResult Post()
    {
        return Ok();
    }
}
