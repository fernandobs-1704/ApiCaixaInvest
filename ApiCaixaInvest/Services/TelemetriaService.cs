using ApiCaixaInvest.Data;
using ApiCaixaInvest.Models;

namespace ApiCaixaInvest.Services;

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
            Data = DateTime.UtcNow
        };

        _db.TelemetriaRegistros.Add(registro);
        await _db.SaveChangesAsync();
    }
}
