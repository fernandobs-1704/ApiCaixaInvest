using ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;
using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;

namespace ApiCaixaInvest.Application.Interfaces
{
    public interface ISimulacoesService
    {
        Task<SimularInvestimentoResponse> SimularAsync(SimularInvestimentoRequest request);

        Task<SimularEContratarInvestimentoResponse> SimularEContratarAsync(SimularInvestimentoRequest request);

        Task<IReadOnlyList<SimulacaoHistoricoResponse>> ObterHistoricoAsync();

        Task<IReadOnlyList<SimulacoesPorProdutoDiaResponse>> ObterResumoPorProdutoDiaAsync();
    }
}
