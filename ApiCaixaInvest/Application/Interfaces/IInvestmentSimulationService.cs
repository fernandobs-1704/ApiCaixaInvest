using ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;
using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;

namespace ApiCaixaInvest.Application.Interfaces;

public interface IInvestmentSimulationService
{
    Task<SimularInvestimentoResponse> SimularAsync(SimularInvestimentoRequest request);
}
