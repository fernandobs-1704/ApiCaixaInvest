using ApiCaixaInvest.Application.Dtos.Responses.Investimentos;
using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Domain.Models;
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
            var resultadoInterno = await EfetivarSimulacoesInternalAsync(clienteId, simulacaoIds);

            // Caso NÃO exista nenhuma simulação válida
            if (!resultadoInterno.Efetivadas.Any() &&
                !resultadoInterno.JaEfetivadas.Any() &&
                resultadoInterno.NaoEncontradas.Any())
            {
                return new EfetivarSimulacoesResultadoResponse
                {
                    Sucesso = false,
                    Mensagem = "Nenhuma simulação válida foi encontrada para efetivar.",
                    ClienteId = clienteId,
                    SimulacoesEfetivadas = new List<int>(),
                    SimulacoesJaEfetivadas = resultadoInterno.JaEfetivadas,
                    SimulacoesNaoEncontradas = resultadoInterno.NaoEncontradas
                };
            }

            // Se existem simulações, mas TODAS já estavam efetivadas
            if (!resultadoInterno.Efetivadas.Any() &&
                resultadoInterno.JaEfetivadas.Any())
            {
                return new EfetivarSimulacoesResultadoResponse
                {
                    Sucesso = false,
                    Mensagem = "As simulações informadas já foram efetivadas anteriormente.",
                    ClienteId = clienteId,
                    SimulacoesEfetivadas = new List<int>(),
                    SimulacoesJaEfetivadas = resultadoInterno.JaEfetivadas,
                    SimulacoesNaoEncontradas = resultadoInterno.NaoEncontradas
                };
            }

            // Efetivou pelo menos uma!
            var perfil = await _perfilRiscoService.CalcularPerfilAsync(clienteId);

            return new EfetivarSimulacoesResultadoResponse
            {
                Sucesso = true,
                Mensagem = "Simulações efetivadas com sucesso.",
                ClienteId = clienteId,
                SimulacoesEfetivadas = resultadoInterno.Efetivadas,
                SimulacoesJaEfetivadas = resultadoInterno.JaEfetivadas,
                SimulacoesNaoEncontradas = resultadoInterno.NaoEncontradas,
                PerfilRisco = perfil
            };
        }


        /// <summary>
        /// Lógica interna para efetivar simulações.
        /// Retorna a lista de IDs efetivados de fato.
        /// </summary>
        private async Task<EfetivarSimulacoesInternalResultado> EfetivarSimulacoesInternalAsync(
     int clienteId,
     IEnumerable<int> ids)
        {
            var resultado = new EfetivarSimulacoesInternalResultado();

            foreach (var id in ids)
            {
                var simulacao = await _db.Simulacoes
                    .Include(s => s.ProdutoInvestimento)
                    .FirstOrDefaultAsync(s => s.ClienteId == clienteId && s.Id == id);

                if (simulacao == null)
                {
                    resultado.NaoEncontradas.Add(id);
                    continue;
                }

                if (simulacao.Efetivada)
                {
                    resultado.JaEfetivadas.Add(id);
                    continue;
                }

                // 1) Marca simulação como efetivada
                simulacao.Efetivada = true;
                resultado.Efetivadas.Add(id);

                var produto = simulacao.ProdutoInvestimento!;

                // 2) Cria registro no histórico de investimentos
                var investimento = new InvestimentoHistorico
                {
                    ClienteId = simulacao.ClienteId,
                    ProdutoInvestimentoId = produto.Id,
                    Tipo = produto.Tipo,                   // snapshot do tipo na época
                    Valor = simulacao.ValorInvestido,      // valor aplicado
                    Rentabilidade = produto.RentabilidadeAnual,
                    Data = DateTime.UtcNow                 // data da efetivação
                };

                _db.InvestimentosHistorico.Add(investimento);
            }

            await _db.SaveChangesAsync();

            return resultado;
        }


    }
}
