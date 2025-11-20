using ApiCaixaInvest.Application.Dtos.Responses.Investimentos;
using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;

namespace ApiCaixaInvest.Application.Interfaces
{
    public interface IInvestimentosService
    {
        /// <summary>
        /// Retorna o histórico de investimentos efetivados do cliente.
        /// </summary>
        Task<IReadOnlyList<InvestimentoHistoricoResponse>> ObterHistoricoAsync(int clienteId);

        /// <summary>
        /// Efetiva simulações em investimentos reais, sem retornar detalhes.
        /// Mantida para cenários em que o chamador não precisa de resposta rica.
        /// </summary>
        Task EfetivarSimulacoesAsync(int clienteId, IEnumerable<int> simulacaoIds);

        /// <summary>
        /// Efetiva simulações e já retorna informações consolidadas,
        /// incluindo o novo perfil de risco do cliente.
        /// </summary>
        Task<EfetivarSimulacoesResultadoResponse> EfetivarSimulacoesEAtualizarPerfilAsync(
            int clienteId,
            IEnumerable<int> simulacaoIds);
    }
}
