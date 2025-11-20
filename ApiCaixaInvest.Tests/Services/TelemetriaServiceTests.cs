using ApiCaixaInvest.Application.Dtos.Responses.Telemetria;
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
    public class TelemetriaServiceTests : TestBase
    {
        [Fact]
        public async Task RegistrarAsync_DevePersistirRegistro()
        {
            using var ctx = CreateContext();
            var service = new TelemetriaService(ctx);

            // Act
            await service.RegistrarAsync("simular-investimento", 250);

            // Assert
            var registros = await ctx.TelemetriaRegistros.ToListAsync();
            Assert.Single(registros);
            Assert.Equal("simular-investimento", registros[0].Servico);
            Assert.Equal(250, registros[0].TempoRespostaMs);
        }

        [Fact]
        public async Task ObterResumoAsync_DeveRetornarResumoPorServicoNoPeriodo()
        {
            using var ctx = CreateContext();

            ctx.TelemetriaRegistros.AddRange(
                new TelemetriaRegistro
                {
                    Servico = "simular-investimento",
                    TempoRespostaMs = 200,
                    Data = new DateTime(2025, 10, 10)
                },
                new TelemetriaRegistro
                {
                    Servico = "simular-investimento",
                    TempoRespostaMs = 300,
                    Data = new DateTime(2025, 10, 11)
                },
                new TelemetriaRegistro
                {
                    Servico = "perfil-risco",
                    TempoRespostaMs = 150,
                    Data = new DateTime(2025, 10, 10)
                },
                // fora do período
                new TelemetriaRegistro
                {
                    Servico = "simular-investimento",
                    TempoRespostaMs = 500,
                    Data = new DateTime(2025, 9, 1)
                }
            );
            await ctx.SaveChangesAsync();

            var service = new TelemetriaService(ctx);

            var inicio = new DateOnly(2025, 10, 1);
            var fim = new DateOnly(2025, 10, 31);

            // Act
            TelemetriaResponse resumo = await service.ObterResumoAsync(inicio, fim);

            // Assert
            Assert.Equal(inicio, resumo.Periodo.Inicio);
            Assert.Equal(fim, resumo.Periodo.Fim);
            Assert.Equal(2, resumo.Servicos.Count);

            var simular = resumo.Servicos.FirstOrDefault(s => s.Nome == "simular-investimento");
            Assert.NotNull(simular);
            Assert.Equal(2, simular!.QuantidadeChamadas);
            Assert.Equal(250, simular.MediaTempoRespostaMs); // média de 200 e 300

            var perfil = resumo.Servicos.FirstOrDefault(s => s.Nome == "perfil-risco");
            Assert.NotNull(perfil);
            Assert.Equal(1, perfil!.QuantidadeChamadas);
            Assert.Equal(150, perfil.MediaTempoRespostaMs);
        }

        [Fact]
        public async Task ObterResumoAsync_SemRegistrosNoPeriodo_DeveRetornarListaVazia()
        {
            using var ctx = CreateContext();
            var service = new TelemetriaService(ctx);

            var inicio = new DateOnly(2025, 1, 1);
            var fim = new DateOnly(2025, 1, 31);

            // Act
            var resumo = await service.ObterResumoAsync(inicio, fim);

            // Assert
            Assert.NotNull(resumo);
            Assert.Empty(resumo.Servicos);
            Assert.Equal(inicio, resumo.Periodo.Inicio);
            Assert.Equal(fim, resumo.Periodo.Fim);
        }
    }
}
