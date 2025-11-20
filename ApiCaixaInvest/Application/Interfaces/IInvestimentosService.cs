using ApiCaixaInvest.Application.Dtos.Responses.Investimentos;

namespace ApiCaixaInvest.Application.Interfaces;

public interface IInvestimentosService
{
    Task<IReadOnlyList<InvestimentoHistoricoResponse>> ObterHistoricoAsync(int clienteId);

    /// <summary>
    /// Efetiva uma ou mais simulações, transformando-as em investimentos reais.
    /// </summary>
    Task EfetivarSimulacoesAsync(int clienteId, IEnumerable<int> simulacaoIds);
}
