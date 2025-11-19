using System.Diagnostics;
using ApiCaixaInvest.Services;

namespace ApiCaixaInvest.Middleware;

/// <summary>
/// Middleware responsável por medir o tempo de resposta das chamadas à API
/// e registrar os dados de telemetria no banco.
/// </summary>
public class TelemetriaMiddleware
{
    private readonly RequestDelegate _next;

    public TelemetriaMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Só mede telemetria de chamadas da API (ignora Swagger, arquivos estáticos etc.)
        var path = context.Request.Path.Value ?? string.Empty;
        var isApi = path.StartsWith("/api", StringComparison.OrdinalIgnoreCase);
        var isSwagger = path.Contains("swagger", StringComparison.OrdinalIgnoreCase);

        if (!isApi || isSwagger)
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();
        long elapsedMs = stopwatch.ElapsedMilliseconds;

        // Monta o nome do "serviço" para fins de telemetria
        string servico = $"{context.Request.Method} {path}";

        try
        {
            // Resolve o serviço a partir do RequestServices (escopo por requisição)
            var telemetriaService = context.RequestServices.GetService<TelemetriaService>();
            if (telemetriaService != null)
            {
                await telemetriaService.RegistrarAsync(servico, elapsedMs);
            }
        }
        catch
        {
            // Em telemetria, falha de log não deve quebrar o request principal.
            // Em cenário real, aqui poderíamos logar em um logger separado.
        }
    }
}
