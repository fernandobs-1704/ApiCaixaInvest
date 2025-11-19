using ApiCaixaInvest.Data;
using ApiCaixaInvest.Dtos.Responses.Telemetria;
using ApiCaixaInvest.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Services;

public class TelemetriaQueryService : ITelemetriaQueryService
{
    private readonly ApiCaixaInvestDbContext _db;

    public TelemetriaQueryService(ApiCaixaInvestDbContext db)
    {
        _db = db;
    }

    public async Task<TelemetriaResponse> ObterResumoAsync(DateOnly inicio, DateOnly fim)
    {
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

        return response;
    }
}
