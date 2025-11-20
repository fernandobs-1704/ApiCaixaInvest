using ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;
using ApiCaixaInvest.Application.Dtos.Responses.Investimentos;
using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiCaixaInvest.Tests.Services
{
    #region Fakes para SimulacoesService

    public class FakeClienteService : IClienteService
    {
        public int UltimoClienteId { get; private set; }

        public Task<Cliente> GarantirClienteAsync(int clienteId)
        {
            UltimoClienteId = clienteId;

            var cliente = new Cliente
            {
                Id = clienteId,
                DataCriacao = DateTime.Now
            };

            return Task.FromResult(cliente);
        }
    }

    public class FakeInvestimentosService : IInvestimentosService
    {
        public int UltimoClienteIdEfetivado { get; private set; }
        public List<int> UltimosIdsSimulacoes { get; private set; } = new();

        public List<InvestimentoHistoricoResponse> HistoricoFake { get; set; } = new();

        public Task<IReadOnlyList<InvestimentoHistoricoResponse>> ObterHistoricoAsync(int clienteId)
        {
            return Task.FromResult((IReadOnlyList<InvestimentoHistoricoResponse>)HistoricoFake);
        }

        public Task EfetivarSimulacoesAsync(int clienteId, IEnumerable<int> simulacaoIds)
        {
            UltimoClienteIdEfetivado = clienteId;
            UltimosIdsSimulacoes = simulacaoIds.ToList();
            return Task.CompletedTask;
        }

        public Task<EfetivarSimulacoesResultadoResponse> EfetivarSimulacoesEAtualizarPerfilAsync(
            int clienteId,
            IEnumerable<int> simulacaoIds)
        {
            throw new NotImplementedException("Não utilizado neste teste.");
        }
    }

    #endregion

    public class SimulacoesServiceTests : TestBase
    {
        [Fact]
        public async Task SimularAsync_DeveCriarSimulacaoERetornarResponse()
        {
            using var ctx = CreateContext();

            ctx.ProdutosInvestimento.Add(new ProdutoInvestimento
            {
                Nome = "CDB Caixa 2026",
                Tipo = "CDB",
                RentabilidadeAnual = 0.12m,
                Risco = "Baixo",
                PrazoMinimoMeses = 6,
                LiquidezDias = 30
            });
            await ctx.SaveChangesAsync();

            var fakeCliente = new FakeClienteService();
            var fakeInvest = new FakeInvestimentosService();
            var fakePerfil = new FakeRiskProfileService();

            var service = new SimulacoesService(ctx, fakeCliente, fakeInvest, fakePerfil);

            var request = new SimularInvestimentoRequest
            {
                ClienteId = 123,
                Valor = 10000m,
                PrazoMeses = 12,
                TipoProduto = "CDB"
            };

            // Act
            var resposta = await service.SimularAsync(request);

            // Assert
            Assert.NotNull(resposta);
            Assert.True(resposta.SimulacaoId > 0);
            Assert.NotNull(resposta.ProdutoValidado);
            Assert.Equal("CDB Caixa 2026", resposta.ProdutoValidado.Nome);

            var simulNoDb = await ctx.Simulacoes.FindAsync(resposta.SimulacaoId);
            Assert.NotNull(simulNoDb);
            Assert.Equal(request.ClienteId, simulNoDb!.ClienteId);
        }

        [Fact]
        public async Task SimularAsync_DeveLancarArgumentException_QuandoClienteIdInvalido()
        {
            using var ctx = CreateContext();
            var fakeCliente = new FakeClienteService();
            var fakeInvest = new FakeInvestimentosService();
            var fakePerfil = new FakeRiskProfileService();

            var service = new SimulacoesService(ctx, fakeCliente, fakeInvest, fakePerfil);

            var request = new SimularInvestimentoRequest
            {
                ClienteId = 0,
                Valor = 10000m,
                PrazoMeses = 12,
                TipoProduto = "CDB"
            };

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.SimularAsync(request));
        }

        [Fact]
        public async Task SimularAsync_DeveLancarInvalidOperation_QuandoNaoHaProdutosCompativeis()
        {
            using var ctx = CreateContext();
            var fakeCliente = new FakeClienteService();
            var fakeInvest = new FakeInvestimentosService();
            var fakePerfil = new FakeRiskProfileService();

            var service = new SimulacoesService(ctx, fakeCliente, fakeInvest, fakePerfil);

            var request = new SimularInvestimentoRequest
            {
                ClienteId = 123,
                Valor = 10000m,
                PrazoMeses = 12,
                TipoProduto = "CDB"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SimularAsync(request));
        }

        [Fact]
        public async Task SimularEContratarAsync_DeveChamarEfetivarERecalcularPerfil()
        {
            using var ctx = CreateContext();

            ctx.ProdutosInvestimento.Add(new ProdutoInvestimento
            {
                Nome = "CDB Caixa 2026",
                Tipo = "CDB",
                RentabilidadeAnual = 0.12m,
                Risco = "Baixo",
                PrazoMinimoMeses = 6,
                LiquidezDias = 30
            });
            await ctx.SaveChangesAsync();

            var fakeCliente = new FakeClienteService();
            var fakeInvest = new FakeInvestimentosService
            {
                HistoricoFake = new List<InvestimentoHistoricoResponse>
                {
                    new InvestimentoHistoricoResponse
                    {
                        Id = 1,
                        Tipo = "CDB",
                        Valor = 10000m,
                        Rentabilidade = 0.12m,
                        Data = DateTime.Now
                    }
                }
            };
            var fakePerfil = new FakeRiskProfileService();

            var service = new SimulacoesService(ctx, fakeCliente, fakeInvest, fakePerfil);

            var request = new SimularInvestimentoRequest
            {
                ClienteId = 123,
                Valor = 10000m,
                PrazoMeses = 12,
                TipoProduto = "CDB"
            };

            // Act
            var resposta = await service.SimularEContratarAsync(request);

            // Assert
            Assert.True(resposta.Sucesso);
            Assert.Equal(123, resposta.ClienteId);
            Assert.NotNull(resposta.Simulacao);
            Assert.NotNull(resposta.Investimento);
            Assert.NotNull(resposta.PerfilRisco);
            Assert.Equal(123, fakeInvest.UltimoClienteIdEfetivado);
            Assert.True(fakeInvest.UltimosIdsSimulacoes.Any());
            Assert.Equal(123, fakePerfil.UltimoClienteId);
        }
    }
}
