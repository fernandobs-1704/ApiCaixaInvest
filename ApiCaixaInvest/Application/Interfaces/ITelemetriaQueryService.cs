using ApiCaixaInvest.Application.Dtos.Responses.Telemetria;

namespace ApiCaixaInvest.Application.Interfaces;

public interface ITelemetriaQueryService
{
    Task<TelemetriaResponse> ObterResumoAsync(DateOnly inicio, DateOnly fim);
}
