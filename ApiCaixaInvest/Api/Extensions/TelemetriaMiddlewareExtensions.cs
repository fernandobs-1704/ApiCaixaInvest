using ApiCaixaInvest.Api.Middleware;

namespace ApiCaixaInvest.Api.Extensions;

public static class TelemetriaMiddlewareExtensions
{
    public static IApplicationBuilder UseApiTelemetria(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TelemetriaMiddleware>();
    }
}
