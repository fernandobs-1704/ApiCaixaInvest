using ApiCaixaInvest.Data;
using ApiCaixaInvest.Dtos.Responses;
using ApiCaixaInvest.Dtos.Responses.Telemetria;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Controllers;

[ApiController]
[Route("api")]
public class TelemetriaController : ControllerBase
{
    private readonly ApiCaixaInvestDbContext _db;

    public TelemetriaController(ApiCaixaInvestDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Retorna o resumo de telemetria dos serviços da API em um período.
    /// </summary>
    /// <remarks>
    /// Informe as datas de início e fim no formato yyyy-MM-dd via query string.
    /// Exemplo: GET /api/telemetria?inicio=2025-10-01&amp;fim=2025-10-31
    /// </remarks>
    [HttpGet("telemetria")]
    public async Task<ActionResult<TelemetriaResponse>> GetTelemetria(
        [FromQuery] DateOnly inicio,
        [FromQuery] DateOnly fim)
    {
        if (fim < inicio)
        {
            return BadRequest(new { message = "A data de fim deve ser maior ou igual à data de início." });
        }

        var servicos = await _db.TelemetriaRegistros
     .Where(t =>
         DateOnly.FromDateTime(t.Data) >= inicio &&
         DateOnly.FromDateTime(t.Data) <= fim)
             .GroupBy(t => t.Servico)
            .Select(g => new TelemetriaServicoResponse
            {
                Nome = g.Key,
                QuantidadeChamadas = g.Count(),
                MediaTempoRespostaMs = g.Any()
                    ? (long)g.Average(x => x.TempoRespostaMs)
                    : 0
            })
            .OrderBy(s => s.Nome)
            .ToListAsync();

        var response = new TelemetriaResponse
        {
            Servicos = servicos,
            Periodo = new TelemetriaPeriodoResponse
            {
                Inicio = inicio,
                Fim = fim
            }
        };

        return Ok(response);
    }
}
