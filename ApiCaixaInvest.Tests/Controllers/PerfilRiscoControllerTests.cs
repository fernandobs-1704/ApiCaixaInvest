using System;
using System.Threading.Tasks;
using ApiCaixaInvest.Api.Controllers;
using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using ApiCaixaInvest.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ApiCaixaInvest.Tests.Controllers
{
    public class PerfilRiscoControllerTests
    {
        #region Fakes

        private class FakeRiskProfileService : IRiskProfileService
        {
            private readonly PerfilRiscoResponse _response;
            private readonly bool _throwExceptionOnCalcular;

            public FakeRiskProfileService(
                PerfilRiscoResponse response,
                bool throwException = false)
            {
                _response = response;
                _throwExceptionOnCalcular = throwException;
            }

            public Task<PerfilRiscoResponse> CalcularPerfilAsync(int clienteId)
            {
                if (_throwExceptionOnCalcular)
                    throw new Exception("Erro simulado");

                // Para os testes atuais, o clienteId não altera o retorno;
                // usamos o _response fornecido no construtor.
                return Task.FromResult(_response);
            }

            public Task<PerfilRiscoIaResponse> GerarExplicacaoIaAsync(int clienteId)
            {
                // Para os testes do endpoint IA, devolvemos um objeto simples,
                // mas consistente com o PerfilRiscoResponse de base.
                var ia = new PerfilRiscoIaResponse
                {
                    ClienteId = clienteId,
                    Perfil = _response.Perfil ?? "Conservador",
                    Pontuacao = _response.Pontuacao,
                    Resumo = "O seu perfil foi calculado com base no seu histórico de investimentos.",
                    VisaoComportamentoInvestidor =
                        "Seu perfil foi calculado a partir do seu histórico de investimentos, considerando volume, frequência e exposição a risco.",
                    SugestoesEstrategicas =
                        "Uma estratégia interessante é manter diversificação e revisar periodicamente sua carteira.",
                    AcoesRecomendadas =
                        $"Com a sua pontuação de {_response.Pontuacao} e o perfil {_response.Perfil ?? "Conservador"}, uma boa ação é alinhar seus investimentos aos seus objetivos e revisar a alocação em intervalos regulares.",
                    AlertasImportantes =
                        "Mudanças relevantes na sua situação financeira ou objetivos podem exigir uma revisão completa do seu perfil.",
                    TendenciaPerfis = null,
                    ProximoPerfilProvavel = null
                };

                return Task.FromResult(ia);
            }
        }

        #endregion

        // ==========================
        //  ENDPOINT DETALHADO
        // ==========================

        [Fact]
        public async Task ObterPerfilRisco_Detalhado_DeveRetornarBadRequest_QuandoClienteIdInvalido()
        {
            var fakeService = new FakeRiskProfileService(
                new PerfilRiscoResponse());

            var controller = new PerfilRiscoController(fakeService);

            var result = await controller.ObterPerfilRiscoDetalhado(0);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task ObterPerfilRisco_Detalhado_DeveRetornarOk_QuandoSucesso()
        {
            var esperado = new PerfilRiscoResponse
            {
                ClienteId = 10,
                Perfil = "Conservador",
                Pontuacao = 80,
                Descricao = "Cliente de baixo risco."
            };

            var fakeService = new FakeRiskProfileService(esperado);

            var controller = new PerfilRiscoController(fakeService);

            var result = await controller.ObterPerfilRiscoDetalhado(10);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<PerfilRiscoResponse>(ok.Value);
            Assert.Equal(10, value.ClienteId);
            Assert.Equal("Conservador", value.Perfil);
            Assert.Equal(80, value.Pontuacao);
        }

        [Fact]
        public async Task ObterPerfilRisco_Detalhado_DeveRetornarErro500_QuandoServiceLancaExcecao()
        {
            var fakeService = new FakeRiskProfileService(
                new PerfilRiscoResponse(),
                throwException: true);

            var controller = new PerfilRiscoController(fakeService);

            var result = await controller.ObterPerfilRiscoDetalhado(10);

            var status = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, status.StatusCode);
            Assert.NotNull(status.Value);
        }

        // ==========================
        //  ENDPOINT BÁSICO
        // ==========================

        [Fact]
        public async Task ObterPerfilRisco_Basico_DeveRetornarBadRequest_QuandoClienteIdInvalido()
        {
            var fakeService = new FakeRiskProfileService(
                new PerfilRiscoResponse());

            var controller = new PerfilRiscoController(fakeService);

            var result = await controller.ObterPerfilRiscoBasico(0);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task ObterPerfilRisco_Basico_DeveRetornarOk_QuandoSucesso()
        {
            var esperado = new PerfilRiscoResponse
            {
                ClienteId = 5,
                Perfil = "Moderado",
                Pontuacao = 120,
                Descricao = "Perfil moderado teste."
            };

            var fakeService = new FakeRiskProfileService(esperado);

            var controller = new PerfilRiscoController(fakeService);

            var result = await controller.ObterPerfilRiscoBasico(5);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<PerfilRiscoBasicoResponse>(ok.Value);

            Assert.Equal(5, value.ClienteId);
            Assert.Equal("Moderado", value.Perfil);
            Assert.Equal(120, value.Pontuacao);
            Assert.Equal("Perfil moderado teste.", value.Descricao);
        }

        // ==========================
        //  ENDPOINT IA
        // ==========================

        [Fact]
        public async Task ObterPerfilRisco_Ia_DeveRetornarBadRequest_QuandoClienteIdInvalido()
        {
            var fakeService = new FakeRiskProfileService(
                new PerfilRiscoResponse());

            var controller = new PerfilRiscoController(fakeService);

            var result = await controller.ObterPerfilRiscoIa(0);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task ObterPerfilRisco_Ia_DeveRetornarOk_QuandoSucesso()
        {
            var baseResponse = new PerfilRiscoResponse
            {
                ClienteId = 20,
                Perfil = "Conservador",
                Pontuacao = 60,
                Descricao = "Perfil conservador teste."
            };

            var fakeService = new FakeRiskProfileService(baseResponse);

            var controller = new PerfilRiscoController(fakeService);

            var result = await controller.ObterPerfilRiscoIa(20);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<PerfilRiscoIaResponse>(ok.Value);

            Assert.Equal(20, value.ClienteId);
            Assert.Equal("Conservador", value.Perfil);
            Assert.Equal(60, value.Pontuacao);
            Assert.False(string.IsNullOrWhiteSpace(value.Resumo));
            Assert.False(string.IsNullOrWhiteSpace(value.AcoesRecomendadas));
        }
    }
}
