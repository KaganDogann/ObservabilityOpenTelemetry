using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Logging.Shared;

public class OpenTelemetryTraceIdMiddleware //Bu middleware serilog ile alakalı değil. 
{
    private readonly RequestDelegate _next;

    public OpenTelemetryTraceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {


        var logger = context.RequestServices.GetRequiredService<ILogger<OpenTelemetryTraceIdMiddleware>>();

        var traceId = Activity.Current?.TraceId.ToString();

        using (logger.BeginScope("{@traceId}", traceId))  //BeginScope seri log a özel değil. 
        {
            await _next(context); // bunadn sonrakşi tüm işlemlerde Ilogger üzerinden log üretirsek hepsine TraceIdd'yi ekleyecek. "{$traceId}" -> al bu datayı özel oalrka indexle. 
        }
    }
}
