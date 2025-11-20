using ApiCaixaInvest.Application.Dtos.Responses.Telemetria;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Infrastructure.Services;

/// <summary>
/// Serviço responsável por registrar e consultar métricas de telemetria da API.
/// </summary>
public class TelemetriaService : ITelemetriaService
{
    private readonly ApiCaixaInvestDbContext _db;

    public TelemetriaService(ApiCaixaInvestDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Registra o tempo de resposta de um serviço/end-point.
    /// </summary>
    public async Task RegistrarAsync(string servico, long tempoRespostaMs)
    {
        var registro = new TelemetriaRegistro
        {
            Servico = servico,
            TempoRespostaMs = tempoRespostaMs,
            Data = DateTime.Now
        };

        _db.TelemetriaRegistros.Add(registro);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Retorna o resumo de telemetria dos serviços da API em um período.
    /// </summary>
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

        return new TelemetriaResponse
        {
            Servicos = servicos,
            Periodo = new TelemetriaPeriodoResponse
            {
                Inicio = inicio,
                Fim = fim
            }
        };
    }
}
