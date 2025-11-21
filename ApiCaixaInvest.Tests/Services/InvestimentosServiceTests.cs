using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Domain.Enums;
using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiCaixaInvest.Tests.Services
{
    public class FakeRiskProfileService : IRiskProfileService
    {
        public int UltimoClienteId { get; private set; }

        public Task<PerfilRiscoResponse> CalcularPerfilAsync(int clienteId)
        {
            UltimoClienteId = clienteId;

            var resp = new PerfilRiscoResponse
            {
                ClienteId = clienteId,
                Perfil = "Moderado",
                PerfilTipo = PerfilRiscoTipoEnum.Moderado,
                Pontuacao = 100,
                UltimaAtualizacao = DateTime.Now,
                Descricao = "Perfil moderado (fake para testes).",
                TendenciaPerfis = new Dictionary<string, double>
                {
                    { "Conservador", 0.18 },
                    { "Moderado",    0.70 },
                    { "Agressivo",   0.12 }
                },
                ProximoPerfilProvavel = "Moderado"
            };

            return Task.FromResult(resp);
        }

        public Task<PerfilRiscoIaResponse> GerarExplicacaoIaAsync(int clienteId)
        {
            UltimoClienteId = clienteId;

            var ia = new PerfilRiscoIaResponse
            {
                ClienteId = clienteId,
                Perfil = "Moderado",
                Pontuacao = 100,
                Resumo = "O seu perfil é moderado, buscando equilíbrio entre proteção do capital e oportunidade de ganhos superiores ao longo do tempo.",
                VisaoComportamentoInvestidor =
                    "Seu perfil foi calculado a partir do seu histórico de investimentos, levando em conta volume, frequência de movimentações, liquidez e exposição a risco.",
                SugestoesEstrategicas =
                    "Como investidor de perfil moderado, você pode obter bons resultados equilibrando sua carteira entre renda fixa e ativos de maior risco, enquanto revisa periodicamente sua alocação para evitar concentrações excessivas.",
                AcoesRecomendadas =
                    "A partir da sua pontuação de 100 e do perfil Moderado, é recomendado estabelecer faixas-alvo de alocação entre renda fixa e renda variável, revisando sua carteira semestralmente para garantir que ela continue alinhada aos seus objetivos.",
                AlertasImportantes =
                    "Com seu perfil moderado, é essencial acompanhar a parcela mais exposta ao risco, já que oscilações maiores podem ocorrer se a carteira não for revisada periodicamente. Mudanças na sua vida financeira ou nos seus objetivos podem exigir uma reavaliação completa do seu perfil.",
                TendenciaPerfis = new Dictionary<string, double>
                {
                    { "Conservador", 0.18 },
                    { "Moderado",    0.70 },
                    { "Agressivo",   0.12 }
                },
                ProximoPerfilProvavel = "Moderado"
            };

            return Task.FromResult(ia);
        }
    }

    public class InvestimentosServiceTests : TestBase
    {
        [Fact]
        public async Task ObterHistoricoAsync_DeveRetornarInvestimentosDoCliente()
        {
            using var ctx = CreateContext();

            ctx.InvestimentosHistorico.AddRange(
                new InvestimentoHistorico
                {
                    ClienteId = 1,
                    Tipo = "CDB",
                    Valor = 1000m,
                    Rentabilidade = 0.12m,
                    Data = new DateTime(2025, 1, 10)
                },
                new InvestimentoHistorico
                {
                    ClienteId = 1,
                    Tipo = "Fundo",
                    Valor = 2000m,
                    Rentabilidade = 0.15m,
                    Data = new DateTime(2025, 2, 5)
                },
                new InvestimentoHistorico
                {
                    ClienteId = 2,
                    Tipo = "LCI",
                    Valor = 3000m,
                    Rentabilidade = 0.10m,
                    Data = new DateTime(2025, 3, 1)
                }
            );
            await ctx.SaveChangesAsync();

            var fakePerfil = new FakeRiskProfileService();
            var service = new InvestimentosService(ctx, fakePerfil);

            // Act
            var historico = await service.ObterHistoricoAsync(1);

            // Assert
            Assert.Equal(2, historico.Count);
            Assert.True(historico.All(h => h.Tipo == "CDB" || h.Tipo == "Fundo"));
        }

        [Fact]
        public async Task EfetivarSimulacoesAsync_DeveCriarInvestimentoEMarcarSimulacao()
        {
            using var ctx = CreateContext();

            var produto = new ProdutoInvestimento
            {
                Nome = "CDB Teste",
                Tipo = "CDB",
                RentabilidadeAnual = 0.12m,
                Risco = "Baixo",
                PrazoMinimoMeses = 6,
                LiquidezDias = 30
            };
            ctx.ProdutosInvestimento.Add(produto);
            await ctx.SaveChangesAsync();

            var simul = new SimulacaoInvestimento
            {
                ClienteId = 1,
                ProdutoInvestimentoId = produto.Id,
                ValorInvestido = 1000m,
                ValorFinal = 1120m,
                PrazoMeses = 12,
                DataSimulacao = DateTime.Now,
                Efetivada = false
            };
            ctx.Simulacoes.Add(simul);
            await ctx.SaveChangesAsync();

            var fakePerfil = new FakeRiskProfileService();
            var service = new InvestimentosService(ctx, fakePerfil);

            // Act
            await service.EfetivarSimulacoesAsync(1, new[] { simul.Id });

            // Assert
            var simNoDb = await ctx.Simulacoes.FindAsync(simul.Id);
            Assert.NotNull(simNoDb);
            Assert.True(simNoDb!.Efetivada);

            var investimentos = await ctx.InvestimentosHistorico
                .Where(i => i.ClienteId == 1)
                .ToListAsync();

            Assert.Single(investimentos);
            Assert.Equal(simul.ValorInvestido, investimentos[0].Valor);
        }

        [Fact]
        public async Task EfetivarSimulacoesEAtualizarPerfilAsync_DeveRetornarPerfilCalculado()
        {
            using var ctx = CreateContext();

            var produto = new ProdutoInvestimento
            {
                Nome = "CDB Teste",
                Tipo = "CDB",
                RentabilidadeAnual = 0.12m,
                Risco = "Baixo",
                PrazoMinimoMeses = 6,
                LiquidezDias = 30
            };
            ctx.ProdutosInvestimento.Add(produto);
            await ctx.SaveChangesAsync();

            var simul = new SimulacaoInvestimento
            {
                ClienteId = 1,
                ProdutoInvestimentoId = produto.Id,
                ValorInvestido = 1000m,
                ValorFinal = 1120m,
                PrazoMeses = 12,
                DataSimulacao = DateTime.Now,
                Efetivada = false
            };
            ctx.Simulacoes.Add(simul);
            await ctx.SaveChangesAsync();

            var fakePerfil = new FakeRiskProfileService();
            var service = new InvestimentosService(ctx, fakePerfil);

            // Act
            var resultado = await service.EfetivarSimulacoesEAtualizarPerfilAsync(1, new[] { simul.Id });

            // Assert
            Assert.True(resultado.Sucesso);
            Assert.Equal(1, resultado.ClienteId);
            Assert.Single(resultado.SimulacoesEfetivadas);
            Assert.NotNull(resultado.PerfilRisco);
            Assert.Equal(1, fakePerfil.UltimoClienteId);
        }
    }
}
