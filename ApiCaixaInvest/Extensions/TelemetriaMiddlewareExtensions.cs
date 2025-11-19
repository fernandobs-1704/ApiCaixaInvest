using ApiCaixaInvest.Middleware;

namespace ApiCaixaInvest.Extensions;

public static class TelemetriaMiddlewareExtensions
{
    public static IApplicationBuilder UseApiTelemetria(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TelemetriaMiddleware>();
    }
}
