using ApiCaixaInvest.Application.Dtos.Responses.Investimentos;

namespace ApiCaixaInvest.Application.Interfaces;

public interface IInvestimentosService
{
    Task<IReadOnlyList<InvestimentoHistoricoResponse>> ObterHistoricoClienteAsync(int clienteId);
}
