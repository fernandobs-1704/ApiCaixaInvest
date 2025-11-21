using ApiCaixaInvest.Api.Controllers;
using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using ApiCaixaInvest.Application.Dtos.Responses.Produtos;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ApiCaixaInvest.Tests.Controllers
{
    public class RecomendacoesControllerTests
    {
        #region Fake Services

        private class FakeProdutosService : IProdutosService
        {
            private readonly IReadOnlyList<ProdutoRecomendadoResponse> _retorno;
            private readonly Exception _exceptionToThrow;

            public FakeProdutosService(
                IReadOnlyList<ProdutoRecomendadoResponse> retorno,
                Exception exceptionToThrow = null)
            {
                _retorno = retorno;
                _exceptionToThrow = exceptionToThrow;
            }

            public Task<IReadOnlyList<ProdutoRecomendadoResponse>> ObterProdutosRecomendadosAsync(string perfil)
            {
                if (_exceptionToThrow != null)
                    throw _exceptionToThrow;

                return Task.FromResult(_retorno);
            }
        }

        private class FakeRiskProfileService : IRiskProfileService
        {
            public Task<PerfilRiscoResponse> CalcularPerfilAsync(int clienteId)
            {
                return Task.FromResult(new PerfilRiscoResponse
                {
                    ClienteId = clienteId,
                    Perfil = "Conservador",
                    PerfilTipo = PerfilRiscoTipoEnum.Conservador,
                    Pontuacao = 10,
                    Descricao = "Cliente de baixo risco",
                    UltimaAtualizacao = DateTime.UtcNow,
                    TendenciaPerfis = new Dictionary<string, double>
            {
                { "Conservador", 0.80 },
                { "Moderado",    0.18 },
                { "Agressivo",   0.02 }
            },
                    ProximoPerfilProvavel = "Conservador"
                });
            }

            public Task<PerfilRiscoIaResponse> GerarExplicacaoIaAsync(int clienteId)
            {
                // Para os testes atuais de RecomendacoesController,
                // provavelmente esse método nem é chamado.
                // Mas precisa existir para satisfazer a interface.
                return Task.FromResult(new PerfilRiscoIaResponse
                {
                    ClienteId = clienteId,
                    Perfil = "Conservador",
                    Pontuacao = 10,
                    Resumo = "O seu perfil é conservador, priorizando segurança e baixa volatilidade.",
                    VisaoComportamentoInvestidor =
                        "Seu perfil foi calculado com base no seu histórico de investimentos, considerando volume aplicado, frequência de movimentações e exposição a risco.",
                    SugestoesEstrategicas =
                        "Como investidor de perfil conservador, você pode fortalecer sua segurança mantendo uma boa reserva de emergência e avaliando, com calma, a inclusão de ativos moderados.",
                    AcoesRecomendadas =
                        "Com a sua pontuação de 10 e o enquadramento no perfil Conservador, uma boa ação é manter foco em ativos de baixo risco e testar aos poucos exposições controladas a produtos um pouco mais arrojados.",
                    AlertasImportantes =
                        "Como investidor conservador, é importante ficar atento ao risco de a rentabilidade ficar abaixo da inflação quando há concentração excessiva em produtos de curtíssimo prazo.",
                    TendenciaPerfis = new Dictionary<string, double>
            {
                { "Conservador", 0.80 },
                { "Moderado",    0.18 },
                { "Agressivo",   0.02 }
            },
                    ProximoPerfilProvavel = "Conservador"
                });
            }
        }


        #endregion

        [Fact]
        public async Task GetProdutosRecomendados_DeveRetornarOk_QuandoSucesso()
        {
            var lista = new List<ProdutoRecomendadoResponse>
            {
                new ProdutoRecomendadoResponse
                {
                    Id = 1,
                    Nome = "CDB Liquidez Diária",
                    Tipo = "CDB",
                    Risco = "Baixo"
                }
            };

            var fakeProdutos = new FakeProdutosService(lista);
            var fakePerfil = new FakeRiskProfileService();

            var controller = new RecomendacoesController(fakeProdutos, fakePerfil);

            var result = await controller.GetProdutosRecomendados("Conservador");

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<ProdutoRecomendadoResponse>>(ok.Value);
            Assert.Single(value);
        }

        [Fact]
        public async Task GetProdutosRecomendados_DeveRetornarBadRequest_QuandoArgumentException()
        {
            var fakeProdutos = new FakeProdutosService(
                new List<ProdutoRecomendadoResponse>(),
                new ArgumentException("Perfil obrigatório.")
            );

            var fakePerfil = new FakeRiskProfileService();

            var controller = new RecomendacoesController(fakeProdutos, fakePerfil);

            var result = await controller.GetProdutosRecomendados("");

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task GetProdutosRecomendados_DeveRetornarBadRequest_QuandoInvalidOperationException()
        {
            var fakeProdutos = new FakeProdutosService(
                new List<ProdutoRecomendadoResponse>(),
                new InvalidOperationException("Perfil inválido.")
            );

            var fakePerfil = new FakeRiskProfileService();

            var controller = new RecomendacoesController(fakeProdutos, fakePerfil);

            var result = await controller.GetProdutosRecomendados("XPTO");

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(bad.Value);
        }
    }
}
