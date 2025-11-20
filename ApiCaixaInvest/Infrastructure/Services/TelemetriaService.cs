using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Data;

namespace ApiCaixaInvest.Infrastructure.Services;

/// <summary>
/// Serviço responsável por registrar métricas de telemetria da API.
/// </summary>
public class TelemetriaService
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
}
