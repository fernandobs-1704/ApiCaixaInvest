using ApiCaixaInvest.Application.Dtos.Requests.Investimentos;
using Swashbuckle.AspNetCore.Filters;

namespace ApiCaixaInvest.Api.SwaggerExamples.Investimentos;

public class EfetivarSimulacoesRequestExample : IExamplesProvider<EfetivarSimulacoesRequest>
{
    public EfetivarSimulacoesRequest GetExamples()
    {
        return new EfetivarSimulacoesRequest
        {
            ClienteId = 123,
            SimulacaoIds = new List<int> { 1, 2, 3 }
        };
    }
}
