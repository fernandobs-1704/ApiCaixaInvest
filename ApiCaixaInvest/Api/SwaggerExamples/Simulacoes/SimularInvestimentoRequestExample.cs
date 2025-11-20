using ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Api.SwaggerExamples.Simulacoes;

[NotMapped]
public class SimularInvestimentoRequestExample : IExamplesProvider<SimularInvestimentoRequest>
{
    public SimularInvestimentoRequest GetExamples()
    {
        return new SimularInvestimentoRequest
        {
            ClienteId = 123,
            Valor = 10000.00m,
            PrazoMeses = 12,
            TipoProduto = "CDB"
        };
    }
}
