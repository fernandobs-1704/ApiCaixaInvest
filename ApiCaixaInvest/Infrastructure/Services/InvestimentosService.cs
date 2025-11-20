using ApiCaixaInvest.Application.Dtos.Responses.Investimentos;
using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Infrastructure.Services
{
    public class InvestimentosService : IInvestimentosService
    {
        private readonly ApiCaixaInvestDbContext _db;
        private readonly IRiskProfileService _perfilRiscoService;

        public InvestimentosService(
            ApiCaixaInvestDbContext db,
            IRiskProfileService perfilRiscoService)
        {
            _db = db;
            _perfilRiscoService = perfilRiscoService;
        }

        public async Task<IReadOnlyList<InvestimentoHistoricoResponse>> ObterHistoricoAsync(int clienteId)
        {
            var query = await _db.InvestimentosHistorico
                .Where(i => i.ClienteId == clienteId)
                .OrderByDescending(i => i.Data)
                .Select(i => new InvestimentoHistoricoResponse
                {
                    Id = i.Id,
                    Tipo = i.Tipo,
                    Valor = i.Valor,
                    Rentabilidade = i.Rentabilidade,
                    Data = i.Data
                })
                .ToListAsync();

            return query;
        }

        public async Task EfetivarSimulacoesAsync(int clienteId, IEnumerable<int> simulacaoIds)
        {
            // Reaproveita a lógica interna, mas ignora o retorno
            await EfetivarSimulacoesInternalAsync(clienteId, simulacaoIds);
        }

        public async Task<EfetivarSimulacoesResultadoResponse> EfetivarSimulacoesEAtualizarPerfilAsync(
            int clienteId,
            IEnumerable<int> simulacaoIds)
        {
            // 1) Efetiva simulações e obtém a lista de IDs realmente processados
            var idsEfetivados = await EfetivarSimulacoesInternalAsync(clienteId, simulacaoIds);

            if (!idsEfetivados.Any())
            {
                return new EfetivarSimulacoesResultadoResponse
                {
                    Sucesso = false,
                    Mensagem = "Nenhuma simulação válida foi encontrada para efetivar.",
                    ClienteId = clienteId,
                    SimulacoesEfetivadas = new List<int>()
                };
            }

            // 2) Recalcula perfil de risco
            var perfil = await _perfilRiscoService.CalcularPerfilAsync(clienteId);

            return new EfetivarSimulacoesResultadoResponse
            {
                Sucesso = true,
                Mensagem = "Simulações efetivadas com sucesso.",
                ClienteId = clienteId,
                SimulacoesEfetivadas = idsEfetivados,
                PerfilRisco = perfil
            };
        }

        /// <summary>
        /// Lógica interna para efetivar simulações.
        /// Retorna a lista de IDs efetivados de fato.
        /// </summary>
        private async Task<List<int>> EfetivarSimulacoesInternalAsync(
            int clienteId,
            IEnumerable<int> simulacaoIds)
        {
            var idsLista = simulacaoIds?
                .Distinct()
                .Where(id => id > 0)
                .ToList() ?? new List<int>();

            if (!idsLista.Any())
                return new List<int>();

            // Busca as simulações do cliente que ainda não foram efetivadas
            var simulacoes = await _db.Simulacoes
                .Include(s => s.ProdutoInvestimento)
                .Where(s => s.ClienteId == clienteId
                            && idsLista.Contains(s.Id)
                            && !s.Efetivada)
                .ToListAsync();

            if (!simulacoes.Any())
                return new List<int>();

            var idsEfetivados = new List<int>();

            foreach (var sim in simulacoes)
            {
                var produto = sim.ProdutoInvestimento;

                // Cria registro de investimento realizado
                var investimento = new Domain.Models.InvestimentoHistorico
                {
                    ClienteId = sim.ClienteId,
                    ProdutoInvestimentoId = sim.ProdutoInvestimentoId,
                    Tipo = produto?.Tipo ?? "Desconhecido",
                    Valor = sim.ValorInvestido,
                    Rentabilidade = produto?.RentabilidadeAnual ?? 0m,
                    Data = DateTime.Now
                };

                _db.InvestimentosHistorico.Add(investimento);

                // Marca simulação como efetivada para evitar duplicidade
                sim.Efetivada = true;

                idsEfetivados.Add(sim.Id);
            }

            await _db.SaveChangesAsync();

            return idsEfetivados;
        }
    }
}
