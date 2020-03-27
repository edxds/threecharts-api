using Microsoft.AspNetCore.Mvc;

namespace ThreeChartsAPI.Controllers
{
    [Route("api/ping")]
    [ApiController]
    public class PingController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Get() => "pong";
    }
}