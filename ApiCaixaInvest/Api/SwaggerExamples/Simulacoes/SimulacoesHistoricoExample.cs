using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations.Schema;

[NotMapped]
public class SimulacoesHistoricoExample : IExamplesProvider<IEnumerable<SimulacaoHistoricoResponse>>
{
    public IEnumerable<SimulacaoHistoricoResponse> GetExamples()
    {
        return new List<SimulacaoHistoricoResponse>
        {
            new SimulacaoHistoricoResponse
            {
                Id = 1,
                ClienteId = 123,
                Produto = "CDB Caixa Liquidez Diária",
                ValorInvestido = 10000.00m,
                ValorFinal = 11100.00m,
                PrazoMeses = 12,
                DataSimulacao = new DateTime(2025, 10, 31, 14, 0, 0, DateTimeKind.Utc)
            },
            new SimulacaoHistoricoResponse
            {
                Id = 2,
                ClienteId = 123,
                Produto = "Fundo Multimercado XPTO",
                ValorInvestido = 5000.00m,
                ValorFinal = 5800.00m,
                PrazoMeses = 6,
                DataSimulacao = new DateTime(2025, 9, 15, 10, 30, 0, DateTimeKind.Utc)
            }
        };
    }
}
