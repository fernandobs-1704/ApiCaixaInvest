using ApiCaixaInvest.Dtos.Responses.Telemetria;

namespace ApiCaixaInvest.Interfaces;

public interface ITelemetriaQueryService
{
    Task<TelemetriaResponse> ObterResumoAsync(DateOnly inicio, DateOnly fim);
}
