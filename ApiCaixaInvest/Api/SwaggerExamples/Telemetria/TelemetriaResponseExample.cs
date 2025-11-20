using ApiCaixaInvest.Application.Dtos.Responses.Telemetria;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Api.SwaggerExamples.Telemetria;

[NotMapped]
public class TelemetriaResponseExample : IExamplesProvider<TelemetriaResponse>
{
    public TelemetriaResponse GetExamples()
    {
        return new TelemetriaResponse
        {
            Servicos = new List<TelemetriaServicoResponse>
            {
                new()
                {
                    Nome = "POST /api/simular-investimento",
                    QuantidadeChamadas = 120,
                    MediaTempoRespostaMs = 250
                },
                new()
                {
                    Nome = "GET /api/perfil-risco/{clienteId}",
                    QuantidadeChamadas = 80,
                    MediaTempoRespostaMs = 180
                }
            },
            Periodo = new TelemetriaPeriodoResponse
            {
                Inicio = new DateOnly(2025, 10, 1),
                Fim = new DateOnly(2025, 10, 31)
            }
        };
    }
}
