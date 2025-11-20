using ApiCaixaInvest.Application.Dtos.Responses.Telemetria;

namespace ApiCaixaInvest.Application.Interfaces;

public interface ITelemetriaService
{
    /// <summary>
    /// Registra o tempo de resposta de um serviço/end-point.
    /// </summary>
    Task RegistrarAsync(string servico, long tempoRespostaMs);

    /// <summary>
    /// Retorna o resumo de telemetria dos serviços da API em um período.
    /// </summary>
    Task<TelemetriaResponse> ObterResumoAsync(DateOnly inicio, DateOnly fim);
}
