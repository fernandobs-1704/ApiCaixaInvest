using ApiCaixaInvest.Dtos.Responses.Investimentos;

namespace ApiCaixaInvest.Interfaces;

public interface IInvestimentosService
{
    Task<IReadOnlyList<InvestimentoHistoricoResponse>> ObterHistoricoClienteAsync(int clienteId);
}
