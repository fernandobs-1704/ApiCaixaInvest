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
            private readonly bool _throwException;

            public FakeRiskProfileService(PerfilRiscoResponse response, bool throwException = false)
            {
                _response = response;
                _throwException = throwException;
            }

            public Task<PerfilRiscoResponse> CalcularPerfilAsync(int clienteId)
            {
                if (_throwException)
                    throw new Exception("Erro simulado");

                return Task.FromResult(_response);
            }
        }

        #endregion

        [Fact]
        public async Task ObterPerfilRisco_DeveRetornarBadRequest_QuandoClienteIdInvalido()
        {
            var fakeService = new FakeRiskProfileService(
                new PerfilRiscoResponse());

            var controller = new PerfilRiscoController(fakeService);

            var result = await controller.ObterPerfilRisco(0);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task ObterPerfilRisco_DeveRetornarOk_QuandoSucesso()
        {
            var esperado = new PerfilRiscoResponse
            {
                ClienteId = 10,
                Perfil = "Conservador",
                Pontuacao = 80
            };

            var fakeService = new FakeRiskProfileService(esperado);

            var controller = new PerfilRiscoController(fakeService);

            var result = await controller.ObterPerfilRisco(10);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<PerfilRiscoResponse>(ok.Value);
            Assert.Equal(10, value.ClienteId);
            Assert.Equal("Conservador", value.Perfil);
        }

        [Fact]
        public async Task ObterPerfilRisco_DeveRetornarErro500_QuandoServiceLancaExcecao()
        {
            var fakeService = new FakeRiskProfileService(
                new PerfilRiscoResponse(),
                throwException: true);

            var controller = new PerfilRiscoController(fakeService);

            var result = await controller.ObterPerfilRisco(10);

            var status = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, status.StatusCode);
            Assert.NotNull(status.Value);
        }
    }
}
