using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations.Schema;

[NotMapped]
public class SimulacoesPorProdutoDiaExample : IExamplesProvider<IEnumerable<SimulacoesPorProdutoDiaResponse>>
{
    public IEnumerable<SimulacoesPorProdutoDiaResponse> GetExamples()
    {
        return new List<SimulacoesPorProdutoDiaResponse>
        {
            new SimulacoesPorProdutoDiaResponse
            {
                Produto = "CDB Caixa Liquidez Diária",
                Data = new DateTime(2025, 10, 30),
                QuantidadeSimulacoes = 15,
                MediaValorFinal = 11050.00m
            },
            new SimulacoesPorProdutoDiaResponse
            {
                Produto = "Fundo Multimercado XPTO",
                Data = new DateTime(2025, 10, 30),
                QuantidadeSimulacoes = 8,
                MediaValorFinal = 5700.00m
            }
        };
    }
}
