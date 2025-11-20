using ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;
using Swashbuckle.AspNetCore.Filters;

namespace ApiCaixaInvest.Api.SwaggerExamples.Simulacoes;

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
