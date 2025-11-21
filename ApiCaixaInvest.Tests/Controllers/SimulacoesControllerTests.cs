using ApiCaixaInvest.Api.Controllers;
using ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;
using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;
using ApiCaixaInvest.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ApiCaixaInvest.Tests.Controllers
{
    public class SimulacoesControllerTests
    {
        #region Fake Service

        private class FakeSimulacoesService : ISimulacoesService
        {
            public Exception ExSimular { get; set; }
            public Exception ExSimularEContratar { get; set; }

            public SimularInvestimentoResponse RespostaSimular { get; set; } =
                new SimularInvestimentoResponse();

            public SimularEContratarInvestimentoResponse RespostaSimularEContratar { get; set; } =
                new SimularEContratarInvestimentoResponse();

            public IReadOnlyList<SimulacaoHistoricoResponse> Historico { get; set; } =
                new List<SimulacaoHistoricoResponse>();

            public IReadOnlyList<SimulacoesPorProdutoDiaResponse> ResumoProdutoDia { get; set; } =
                new List<SimulacoesPorProdutoDiaResponse>();

            public Task<SimularInvestimentoResponse> SimularAsync(SimularInvestimentoRequest request)
            {
                if (ExSimular != null)
                    throw ExSimular;

                return Task.FromResult(RespostaSimular);
            }

            public Task<SimularEContratarInvestimentoResponse> SimularEContratarAsync(SimularInvestimentoRequest request)
            {
                if (ExSimularEContratar != null)
                    throw ExSimularEContratar;

                return Task.FromResult(RespostaSimularEContratar);
            }

            public Task<IReadOnlyList<SimulacaoHistoricoResponse>> ObterHistoricoAsync()
            {
                return Task.FromResult(Historico);
            }

            public Task<IReadOnlyList<SimulacoesPorProdutoDiaResponse>> ObterResumoPorProdutoDiaAsync()
            {
                return Task.FromResult(ResumoProdutoDia);
            }
        }

        #endregion

        [Fact]
        public async Task SimularInvestimento_DeveRetornarOk_QuandoSucesso()
        {
            // Arrange
            var fake = new FakeSimulacoesService
            {
                RespostaSimular = new SimularInvestimentoResponse
                {
                    SimulacaoId = 1,
                    ProdutoValidado = new ProdutoResponse
                    {
                        Id = 10,
                        Nome = "CDB Premium",
                        Tipo = "CDB",
                        Rentabilidade = 0.12m,
                        Risco = "Baixo"
                    },
                    ResultadoSimulacao = new ResultadoSimulacaoResponse
                    {
                        ValorFinal = 1120.50m,
                        PrazoMeses = 12,
                        RentabilidadeEfetiva = 0.12m
                    },
                    DataSimulacao = DateTime.UtcNow
                }
            };

            var controller = new SimulacoesController(fake);

            var request = new SimularInvestimentoRequest
            {
                ClienteId = 1,
                Valor = 1000,
                PrazoMeses = 12,
                TipoProduto = "CDB"
            };

            // Act
            var result = await controller.SimularInvestimento(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            // Esperamos o DTO público, não o interno
            var value = Assert.IsType<SimularInvestimentoPublicResponse>(ok.Value);

            Assert.NotNull(value.ProdutoValidado);
            Assert.NotNull(value.ResultadoSimulacao);

            // Garante que o mapeamento veio do serviço corretamente
            Assert.Equal(fake.RespostaSimular.ProdutoValidado.Nome, value.ProdutoValidado.Nome);
            Assert.Equal(fake.RespostaSimular.ResultadoSimulacao.ValorFinal, value.ResultadoSimulacao.ValorFinal);
        }


        [Fact]
        public async Task SimularInvestimento_DeveRetornarBadRequest_QuandoArgumentException()
        {
            var fake = new FakeSimulacoesService
            {
                ExSimular = new ArgumentException("Erro de validação.")
            };

            var controller = new SimulacoesController(fake);

            var request = new SimularInvestimentoRequest();

            var result = await controller.SimularInvestimento(request);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task SimularInvestimento_DeveRetornarBadRequest_QuandoInvalidOperationException()
        {
            var fake = new FakeSimulacoesService
            {
                ExSimular = new InvalidOperationException("Nenhum produto compatível.")
            };

            var controller = new SimulacoesController(fake);

            var request = new SimularInvestimentoRequest
            {
                ClienteId = 1,
                Valor = 1000,
                PrazoMeses = 12,
                TipoProduto = "XYZ"
            };

            var result = await controller.SimularInvestimento(request);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task SimularInvestimento_DeveRetornarErro500_QuandoExcecaoGenerica()
        {
            var fake = new FakeSimulacoesService
            {
                ExSimular = new Exception("Erro inesperado.")
            };

            var controller = new SimulacoesController(fake);

            var request = new SimularInvestimentoRequest
            {
                ClienteId = 1,
                Valor = 1000,
                PrazoMeses = 12,
                TipoProduto = "CDB"
            };

            var result = await controller.SimularInvestimento(request);

            var status = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, status.StatusCode);
        }

        [Fact]
        public async Task SimularEContratarInvestimento_DeveRetornarOk_QuandoSucesso()
        {
            var fake = new FakeSimulacoesService
            {
                RespostaSimularEContratar = new SimularEContratarInvestimentoResponse
                {
                    Sucesso = true,
                    ClienteId = 1
                }
            };

            var controller = new SimulacoesController(fake);

            var request = new SimularInvestimentoRequest
            {
                ClienteId = 1,
                Valor = 1000,
                PrazoMeses = 12,
                TipoProduto = "CDB"
            };

            var result = await controller.SimularEContratarInvestimento(request);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<SimularEContratarInvestimentoResponse>(ok.Value);
            Assert.True(value.Sucesso);
            Assert.Equal(1, value.ClienteId);
        }

        [Fact]
        public async Task SimularEContratarInvestimento_DeveRetornarBadRequest_QuandoArgumentException()
        {
            var fake = new FakeSimulacoesService
            {
                ExSimularEContratar = new ArgumentException("Erro de validação.")
            };

            var controller = new SimulacoesController(fake);

            var request = new SimularInvestimentoRequest();

            var result = await controller.SimularEContratarInvestimento(request);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task SimularEContratarInvestimento_DeveRetornarBadRequest_QuandoInvalidOperationException()
        {
            var fake = new FakeSimulacoesService
            {
                ExSimularEContratar = new InvalidOperationException("Erro de negócio.")
            };

            var controller = new SimulacoesController(fake);

            var request = new SimularInvestimentoRequest
            {
                ClienteId = 1,
                Valor = 1000,
                PrazoMeses = 12,
                TipoProduto = "CDB"
            };

            var result = await controller.SimularEContratarInvestimento(request);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task SimularEContratarInvestimento_DeveRetornarErro500_QuandoExcecaoGenerica()
        {
            var fake = new FakeSimulacoesService
            {
                ExSimularEContratar = new Exception("Erro inesperado.")
            };

            var controller = new SimulacoesController(fake);

            var request = new SimularInvestimentoRequest
            {
                ClienteId = 1,
                Valor = 1000,
                PrazoMeses = 12,
                TipoProduto = "CDB"
            };

            var result = await controller.SimularEContratarInvestimento(request);

            var status = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, status.StatusCode);
        }

        [Fact]
        public async Task GetSimulacoes_DeveRetornarOk_ComLista()
        {
            var fake = new FakeSimulacoesService
            {
                Historico = new List<SimulacaoHistoricoResponse>
                {
                    new SimulacaoHistoricoResponse { Id = 1, ClienteId = 1 }
                }
            };

            var controller = new SimulacoesController(fake);

            var result = await controller.GetSimulacoes();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var lista = Assert.IsAssignableFrom<IEnumerable<SimulacaoHistoricoResponse>>(ok.Value);
            Assert.Single(lista);
        }

        [Fact]
        public async Task GetSimulacoesPorProdutoDia_DeveRetornarOk_ComResumo()
        {
            var fake = new FakeSimulacoesService
            {
                ResumoProdutoDia = new List<SimulacoesPorProdutoDiaResponse>
                {
                    new SimulacoesPorProdutoDiaResponse
                    {
                        Produto = "CDB Banco X"
                    }
                }
            };

            var controller = new SimulacoesController(fake);

            var result = await controller.GetSimulacoesPorProdutoDia();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var lista = Assert.IsAssignableFrom<IEnumerable<SimulacoesPorProdutoDiaResponse>>(ok.Value);
            Assert.Single(lista);
        }
    }
}
