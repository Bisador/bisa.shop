using Microsoft.AspNetCore.Builder; 

namespace Shared.Telemetry;

public static class TelemetryExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}