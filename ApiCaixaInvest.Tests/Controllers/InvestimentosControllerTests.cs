using ApiCaixaInvest.Api.Controllers;
using ApiCaixaInvest.Application.Dtos.Requests.Investimentos;
using ApiCaixaInvest.Application.Dtos.Responses.Investimentos;
using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using ApiCaixaInvest.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ApiCaixaInvest.Tests.Controllers
{
    public class InvestimentosControllerTests
    {
        #region Fake Service

        private class FakeInvestimentosService : IInvestimentosService
        {
            public int UltimoClienteIdHistorico { get; private set; }
            public int UltimoClienteIdEfetivar { get; private set; }
            public IEnumerable<int> UltimosIdsSimulacao { get; private set; } =
                new List<int>();

            private readonly IReadOnlyList<InvestimentoHistoricoResponse> _historico;
            private readonly EfetivarSimulacoesResultadoResponse _resultadoEfetivar;

            public FakeInvestimentosService(
                IReadOnlyList<InvestimentoHistoricoResponse> historico,
                EfetivarSimulacoesResultadoResponse resultadoEfetivar)
            {
                _historico = historico;
                _resultadoEfetivar = resultadoEfetivar;
            }

            public Task<IReadOnlyList<InvestimentoHistoricoResponse>> ObterHistoricoAsync(int clienteId)
            {
                UltimoClienteIdHistorico = clienteId;
                return Task.FromResult(_historico);
            }

            public Task EfetivarSimulacoesAsync(int clienteId, IEnumerable<int> simulacaoIds)
            {
                UltimoClienteIdEfetivar = clienteId;
                UltimosIdsSimulacao = simulacaoIds;
                return Task.CompletedTask;
            }

            public Task<EfetivarSimulacoesResultadoResponse> EfetivarSimulacoesEAtualizarPerfilAsync(
                int clienteId,
                IEnumerable<int> simulacaoIds)
            {
                UltimoClienteIdEfetivar = clienteId;
                UltimosIdsSimulacao = simulacaoIds;
                return Task.FromResult(_resultadoEfetivar);
            }
        }

        #endregion

        [Fact]
        public async Task GetInvestimentos_DeveRetornarBadRequest_QuandoClienteIdInvalido()
        {
            var fakeService = new FakeInvestimentosService(
                new List<InvestimentoHistoricoResponse>(),
                new EfetivarSimulacoesResultadoResponse());

            var controller = new InvestimentosController(fakeService);

            var result = await controller.GetInvestimentos(0);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task GetInvestimentos_DeveRetornarOk_ComLista()
        {
            var lista = new List<InvestimentoHistoricoResponse>
            {
                new InvestimentoHistoricoResponse { Id = 1, Tipo = "CDB" }
            };

            var fakeService = new FakeInvestimentosService(
                lista,
                new EfetivarSimulacoesResultadoResponse());

            var controller = new InvestimentosController(fakeService);

            var result = await controller.GetInvestimentos(10);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<InvestimentoHistoricoResponse>>(ok.Value);
            Assert.Single(value);
        }

        [Fact]
        public async Task EfetivarSimulacoes_DeveRetornarBadRequest_QuandoClienteIdInvalido()
        {
            var fakeService = new FakeInvestimentosService(
                new List<InvestimentoHistoricoResponse>(),
                new EfetivarSimulacoesResultadoResponse());

            var controller = new InvestimentosController(fakeService);

            var request = new EfetivarSimulacoesRequest
            {
                ClienteId = 0,
                SimulacaoIds = new List<int> { 1, 2 }
            };

            var result = await controller.EfetivarSimulacoes(request);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task EfetivarSimulacoes_DeveRetornarBadRequest_QuandoListaVazia()
        {
            var fakeService = new FakeInvestimentosService(
                new List<InvestimentoHistoricoResponse>(),
                new EfetivarSimulacoesResultadoResponse());

            var controller = new InvestimentosController(fakeService);

            var request = new EfetivarSimulacoesRequest
            {
                ClienteId = 1,
                SimulacaoIds = new List<int>()
            };

            var result = await controller.EfetivarSimulacoes(request);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task EfetivarSimulacoes_DeveRetornarOk_ComResultado()
        {
            var resultadoEsperado = new EfetivarSimulacoesResultadoResponse
            {
                Sucesso = true,
                Mensagem = "Simulações efetivadas com sucesso.",
                ClienteId = 1,
                SimulacoesEfetivadas = new List<int> { 5 },
                PerfilRisco = null
            };

            var fakeService = new FakeInvestimentosService(
                new List<InvestimentoHistoricoResponse>(),
                resultadoEsperado);

            var controller = new InvestimentosController(fakeService);

            var request = new EfetivarSimulacoesRequest
            {
                ClienteId = 1,
                SimulacaoIds = new List<int> { 5 }
            };

            var result = await controller.EfetivarSimulacoes(request);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<EfetivarSimulacoesResultadoResponse>(ok.Value);
            Assert.True(value.Sucesso);
            Assert.Equal(1, value.ClienteId);
            Assert.Single(value.SimulacoesEfetivadas);
        }
    }
}
