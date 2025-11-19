using ApiCaixaInvest.Dtos.Responses.Simulacoes;

namespace ApiCaixaInvest.Interfaces;

public interface ISimulacoesConsultaService
{
    Task<IReadOnlyList<SimulacaoHistoricoResponse>> ObterHistoricoAsync();
    Task<IReadOnlyList<SimulacoesPorProdutoDiaResponse>> ObterResumoPorProdutoDiaAsync();
}
