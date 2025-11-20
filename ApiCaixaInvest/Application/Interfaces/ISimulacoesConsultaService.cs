using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;

namespace ApiCaixaInvest.Application.Interfaces;

public interface ISimulacoesConsultaService
{
    Task<IReadOnlyList<SimulacaoHistoricoResponse>> ObterHistoricoAsync();
    Task<IReadOnlyList<SimulacoesPorProdutoDiaResponse>> ObterResumoPorProdutoDiaAsync();
}
