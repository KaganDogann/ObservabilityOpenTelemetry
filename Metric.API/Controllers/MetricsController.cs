using Metric.API.OpenTelemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Metric.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        [HttpGet]
        public IActionResult CounterMetric() // Her kullanıcı üye olduğunda bir değer tutmak istiyorum.
        {
            OpenTelemetryMetric.OrderCreatedEventCounter.Add(1, new KeyValuePair<string, object?>("event", "add"),
                                                                new KeyValuePair<string, object?>("queue.name", "event.created.queue"));
            return Ok();
        }
    }
}
