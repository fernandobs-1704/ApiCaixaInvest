using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiCaixaInvest.Api.Controllers;
using ApiCaixaInvest.Application.Dtos.Responses.Produtos;
using ApiCaixaInvest.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ApiCaixaInvest.Tests.Controllers
{
    public class RecomendacoesControllerTests
    {
        #region Fake Service

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

            var fakeService = new FakeProdutosService(lista);
            var controller = new RecomendacoesController(fakeService);

            var result = await controller.GetProdutosRecomendados("Conservador");

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<ProdutoRecomendadoResponse>>(ok.Value);
            Assert.Single(value);
        }

        [Fact]
        public async Task GetProdutosRecomendados_DeveRetornarBadRequest_QuandoArgumentException()
        {
            var fakeService = new FakeProdutosService(
                new List<ProdutoRecomendadoResponse>(),
                new ArgumentException("Perfil obrigatório."));

            var controller = new RecomendacoesController(fakeService);

            var result = await controller.GetProdutosRecomendados("");

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task GetProdutosRecomendados_DeveRetornarBadRequest_QuandoInvalidOperationException()
        {
            var fakeService = new FakeProdutosService(
                new List<ProdutoRecomendadoResponse>(),
                new InvalidOperationException("Perfil inválido."));

            var controller = new RecomendacoesController(fakeService);

            var result = await controller.GetProdutosRecomendados("XPTO");

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(bad.Value);
        }
    }
}
