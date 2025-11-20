using ApiCaixaInvest.Api.Middleware;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Api.Extensions;

[NotMapped]
public static class TelemetriaMiddlewareExtensions
{
    public static IApplicationBuilder UseApiTelemetria(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TelemetriaMiddleware>();
    }
}
