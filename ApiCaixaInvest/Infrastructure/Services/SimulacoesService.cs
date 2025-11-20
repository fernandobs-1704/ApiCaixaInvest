using ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;
using ApiCaixaInvest.Application.Dtos.Responses.Investimentos;
using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Infrastructure.Services
{
    /// <summary>
    /// Serviço responsável por simular investimentos, consultar histórico de simulações
    /// e orquestrar a operação de simular + contratar em uma única chamada.
    /// </summary>
    public class SimulacoesService : ISimulacoesService
    {
        private readonly ApiCaixaInvestDbContext _db;
        private readonly IClienteService _clienteService;
        private readonly IInvestimentosService _investimentosService;
        private readonly IRiskProfileService _perfilRiscoService;

        public SimulacoesService(
            ApiCaixaInvestDbContext db,
            IClienteService clienteService,
            IInvestimentosService investimentosService,
            IRiskProfileService perfilRiscoService)
        {
            _db = db;
            _clienteService = clienteService;
            _investimentosService = investimentosService;
            _perfilRiscoService = perfilRiscoService;
        }

        public async Task<SimularInvestimentoResponse> SimularAsync(SimularInvestimentoRequest request)
        {
            ValidarRequestSimulacao(request);

            // Garante que o cliente exista
            await _clienteService.GarantirClienteAsync(request.ClienteId);

            // Busca produtos compatíveis
            var tipoNormalizado = request.TipoProduto.Trim().ToLowerInvariant();

            var produtos = await _db.ProdutosInvestimento
                .Where(p =>
                    p.Tipo.ToLower() == tipoNormalizado &&
                    p.PrazoMinimoMeses <= request.PrazoMeses)
                .ToListAsync();

            if (!produtos.Any())
                throw new InvalidOperationException("Nenhum produto compatível encontrado para os parâmetros informados.");

            // Escolhe produto com maior rentabilidade
            var produtoEscolhido = produtos
                .OrderByDescending(p => p.RentabilidadeAnual)
                .First();

            // Cálculo da simulação
            decimal anos = request.PrazoMeses / 12m;
            decimal valorFinal = decimal.Round(
                request.Valor * (1 + produtoEscolhido.RentabilidadeAnual * anos),
                2);

            // Persiste simulação
            var simulacao = new SimulacaoInvestimento
            {
                ClienteId = request.ClienteId,
                ProdutoInvestimentoId = produtoEscolhido.Id,
                ValorInvestido = request.Valor,
                ValorFinal = valorFinal,
                PrazoMeses = request.PrazoMeses,
                DataSimulacao = DateTime.Now
            };

            _db.Simulacoes.Add(simulacao);
            await _db.SaveChangesAsync();

            // Monta DTO de resposta
            return new SimularInvestimentoResponse
            {
                SimulacaoId = simulacao.Id,
                ProdutoValidado = new ProdutoResponse
                {
                    Id = produtoEscolhido.Id,
                    Nome = produtoEscolhido.Nome,
                    Tipo = produtoEscolhido.Tipo,
                    Rentabilidade = produtoEscolhido.RentabilidadeAnual,
                    Risco = produtoEscolhido.Risco
                },
                ResultadoSimulacao = new ResultadoSimulacaoResponse
                {
                    ValorFinal = valorFinal,
                    RentabilidadeEfetiva = produtoEscolhido.RentabilidadeAnual,
                    PrazoMeses = request.PrazoMeses
                },
                DataSimulacao = simulacao.DataSimulacao
            };
        }

        public async Task<SimularEContratarInvestimentoResponse> SimularEContratarAsync(
            SimularInvestimentoRequest request)
        {
            // 1) Simula (já grava a simulação)
            var simulacao = await SimularAsync(request);

            // 2) Efetiva apenas essa simulação
            await _investimentosService.EfetivarSimulacoesAsync(
                request.ClienteId,
                new[] { simulacao.SimulacaoId });

            // 3) Busca investimento mais recente do cliente
            var historico = await _investimentosService.ObterHistoricoAsync(request.ClienteId);
            var investimento = historico
                .OrderByDescending(h => h.Data)
                .FirstOrDefault();

            // 4) Recalcula perfil de risco
            PerfilRiscoResponse perfil = await _perfilRiscoService.CalcularPerfilAsync(request.ClienteId);

            return new SimularEContratarInvestimentoResponse
            {
                Sucesso = true,
                Mensagem = "Simulação realizada e investimento efetivado com sucesso.",
                ClienteId = request.ClienteId,
                Simulacao = simulacao,
                Investimento = investimento,
                PerfilRisco = perfil
            };
        }

        public async Task<IReadOnlyList<SimulacaoHistoricoResponse>> ObterHistoricoAsync()
        {
            var simulacoes = await _db.Simulacoes
                .Include(s => s.ProdutoInvestimento)
                .OrderByDescending(s => s.DataSimulacao)
                .Select(s => new SimulacaoHistoricoResponse
                {
                    Id = s.Id,
                    ClienteId = s.ClienteId,
                    Produto = s.ProdutoInvestimento != null ? s.ProdutoInvestimento.Nome : string.Empty,
                    ValorInvestido = s.ValorInvestido,
                    ValorFinal = s.ValorFinal,
                    PrazoMeses = s.PrazoMeses,
                    DataSimulacao = s.DataSimulacao
                })
                .ToListAsync();

            return simulacoes;
        }

        public async Task<IReadOnlyList<SimulacoesPorProdutoDiaResponse>> ObterResumoPorProdutoDiaAsync()
        {
            var query = await _db.Simulacoes
                .Include(s => s.ProdutoInvestimento)
                .GroupBy(s => new
                {
                    Produto = s.ProdutoInvestimento != null ? s.ProdutoInvestimento.Nome : string.Empty,
                    Data = s.DataSimulacao.Date
                })
                .Select(g => new SimulacoesPorProdutoDiaResponse
                {
                    Produto = g.Key.Produto,
                    Data = g.Key.Data,
                    QuantidadeSimulacoes = g.Count(),
                    MediaValorFinal = g.Average(x => x.ValorFinal)
                })
                .OrderBy(r => r.Produto)
                .ThenBy(r => r.Data)
                .ToListAsync();

            return query;
        }

        private static void ValidarRequestSimulacao(SimularInvestimentoRequest request)
        {
            if (request is null)
                throw new ArgumentException("A requisição não pode ser nula.");

            if (request.ClienteId <= 0)
                throw new ArgumentException("O identificador do cliente deve ser maior que zero.");

            if (request.Valor <= 0)
                throw new ArgumentException("O valor do investimento deve ser maior que zero.");

            if (request.PrazoMeses <= 0)
                throw new ArgumentException("O prazo em meses deve ser maior que zero.");

            if (string.IsNullOrWhiteSpace(request.TipoProduto))
                throw new ArgumentException("O tipo de produto é obrigatório.");
        }
    }
}
