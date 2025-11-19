using ApiCaixaInvest.Dtos.Requests.Simulacoes;
using ApiCaixaInvest.Dtos.Responses.Simulacoes;

namespace ApiCaixaInvest.Interfaces;

public interface IInvestmentSimulationService
{
    Task<SimularInvestimentoResponse> SimularAsync(SimularInvestimentoRequest request);
}
