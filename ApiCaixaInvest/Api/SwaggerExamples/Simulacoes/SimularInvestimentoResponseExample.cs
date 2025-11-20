using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Api.SwaggerExamples.Simulacoes;

[NotMapped]
public class SimularInvestimentoResponseExample : IExamplesProvider<SimularInvestimentoResponse>
{
    public SimularInvestimentoResponse GetExamples()
    {
        return new SimularInvestimentoResponse
        {
            ProdutoValidado = new ProdutoResponse
            {
                Id = 101,
                Nome = "CDB Caixa 2026",
                Tipo = "CDB",
                Rentabilidade = 0.12m,
                Risco = "Baixo"
            },
            ResultadoSimulacao = new ResultadoSimulacaoResponse
            {
                ValorFinal = 11200.00m,
                RentabilidadeEfetiva = 0.12m,
                PrazoMeses = 12
            },
            DataSimulacao = new DateTime(2025, 10, 31, 14, 0, 0)
        };
    }
}
