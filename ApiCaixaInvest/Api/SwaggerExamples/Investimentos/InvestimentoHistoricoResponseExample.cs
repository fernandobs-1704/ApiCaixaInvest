using ApiCaixaInvest.Application.Dtos.Responses.Investimentos;
using Swashbuckle.AspNetCore.Filters;

public class InvestimentoHistoricoResponseExample : IExamplesProvider<IEnumerable<InvestimentoHistoricoResponse>>
{
    public IEnumerable<InvestimentoHistoricoResponse> GetExamples()
    {
        return new List<InvestimentoHistoricoResponse>
        {
            new InvestimentoHistoricoResponse
            {
                Id = 1,
                Tipo = "CDB",
                Valor = 10000.00m,
                Rentabilidade = 0.12m,
                Data = new DateTime(2025,10,31)
            },
            new InvestimentoHistoricoResponse
            {
                Id = 2,
                Tipo = "Fundo Multimercado",
                Valor = 5000.00m,
                Rentabilidade = 0.08m,
                Data = new DateTime(2025,09,15)
            }
        };
    }
}
