using System;
using System.Threading.Tasks;
using ApiCaixaInvest.Api.Controllers;
using ApiCaixaInvest.Application.Dtos.Responses.Telemetria;
using ApiCaixaInvest.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ApiCaixaInvest.Tests.Controllers
{
    public class TelemetriaControllerTests
    {
        #region Fake Service

        private class FakeTelemetriaService : ITelemetriaService
        {
            private readonly TelemetriaResponse _response;

            public FakeTelemetriaService(TelemetriaResponse response)
            {
                _response = response;
            }

            public Task RegistrarAsync(string servico, long tempoRespostaMs)
            {
                // Não utilizado nos testes de controller
                return Task.CompletedTask;
            }

            public Task<TelemetriaResponse> ObterResumoAsync(DateOnly inicio, DateOnly fim)
            {
                return Task.FromResult(_response);
            }
        }

        #endregion

        [Fact]
        public async Task GetTelemetria_DeveRetornarBadRequest_QuandoFimMenorQueInicio()
        {
            var fake = new FakeTelemetriaService(new TelemetriaResponse());
            var controller = new TelemetriaController(fake);

            var inicio = new DateOnly(2025, 1, 10);
            var fim = new DateOnly(2025, 1, 5);

            var result = await controller.GetTelemetria(inicio, fim);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task GetTelemetria_DeveRetornarOk_QuandoPeriodoValido()
        {
            var esperado = new TelemetriaResponse
            {
                Periodo = new TelemetriaPeriodoResponse
                {
                    Inicio = new DateOnly(2025, 1, 1),
                    Fim = new DateOnly(2025, 1, 31)
                }
            };

            var fake = new FakeTelemetriaService(esperado);
            var controller = new TelemetriaController(fake);

            var result = await controller.GetTelemetria(
                new DateOnly(2025, 1, 1),
                new DateOnly(2025, 1, 31));

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<TelemetriaResponse>(ok.Value);
            Assert.Equal(esperado.Periodo.Inicio, value.Periodo.Inicio);
            Assert.Equal(esperado.Periodo.Fim, value.Periodo.Fim);
        }
    }
}
