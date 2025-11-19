using ApiCaixaInvest.Data;
using ApiCaixaInvest.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Controllers;

[ApiController]
[Route("api/telemetria")]
public class TelemetriaController : ControllerBase
{
    private readonly ApiCaixaInvestDbContext _db;

    public TelemetriaController(ApiCaixaInvestDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lista os registros individuais de telemetria.
    /// </summary>
    /// <remarks>
    /// Retorna cada chamada monitorada com serviço, tempo de resposta e data/hora.
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TelemetriaRegistroResponse>>> GetTelemetria()
    {
        var registros = await _db.TelemetriaRegistros
            .OrderByDescending(t => t.Data)
            .Select(t => new TelemetriaRegistroResponse
            {
                Id = t.Id,
                Servico = t.Servico,
                TempoRespostaMs = t.TempoRespostaMs,
                Data = t.Data
            })
            .ToListAsync();

        return Ok(registros);
    }

    /// <summary>
    /// Retorna o resumo de telemetria agrupado por serviço e dia.
    /// </summary>
    /// <remarks>
    /// Fornece quantidade de chamadas e tempo médio de resposta por endpoint.
    /// </remarks>
    [HttpGet("resumo")]
    public async Task<ActionResult<IEnumerable<TelemetriaResumoResponse>>> GetTelemetriaResumo()
    {
        var resumo = await _db.TelemetriaRegistros
            .GroupBy(t => new { t.Servico, Data = t.Data.Date })
            .Select(g => new TelemetriaResumoResponse
            {
                Servico = g.Key.Servico,
                Data = g.Key.Data,
                QuantidadeChamadas = g.Count(),
                TempoMedioMs = (long)g.Average(x => x.TempoRespostaMs)
            })
            .OrderBy(r => r.Servico)
            .ThenBy(r => r.Data)
            .ToListAsync();

        return Ok(resumo);
    }
}
