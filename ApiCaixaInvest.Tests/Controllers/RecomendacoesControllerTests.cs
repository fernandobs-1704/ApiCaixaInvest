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
                    UltimaAtualizacao = DateTime.UtcNow
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
