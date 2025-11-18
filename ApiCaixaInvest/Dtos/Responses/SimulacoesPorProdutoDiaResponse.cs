namespace ApiCaixaInvest.Dtos.Responses;

public class SimulacoesPorProdutoDiaResponse
{
    public string Produto { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public int QuantidadeSimulacoes { get; set; }
    public decimal MediaValorFinal { get; set; }
}
