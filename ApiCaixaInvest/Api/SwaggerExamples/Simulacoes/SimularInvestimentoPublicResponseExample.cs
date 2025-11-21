using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;
using Swashbuckle.AspNetCore.Filters;

namespace ApiCaixaInvest.Api.SwaggerExamples.Simulacoes
{
    public class SimularInvestimentoPublicResponseExample
        : IExamplesProvider<SimularInvestimentoPublicResponse>
    {
        public SimularInvestimentoPublicResponse GetExamples()
        {
            return new SimularInvestimentoPublicResponse
            {
                ProdutoValidado = new ProdutoResponse
                {
                    Id = 101,
                    Nome = "CDB Caixa Liquidez Diária",
                    Tipo = "CDB",
                    Rentabilidade = 0.105m,
                    Risco = "Baixo"
                },
                ResultadoSimulacao = new ResultadoSimulacaoResponse
                {
                    ValorFinal = 11234.56m,
                    RentabilidadeEfetiva = 0.105m,
                    PrazoMeses = 12
                },
                DataSimulacao = DateTime.Now
            };
        }
    }
}
