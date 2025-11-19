namespace ApiCaixaInvest.Dtos.Responses.Simulacoes;

public class SimulacaoHistoricoResponse
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Produto { get; set; } = string.Empty;
    public decimal ValorInvestido { get; set; }
    public decimal ValorFinal { get; set; }
    public int PrazoMeses { get; set; }
    public DateTime DataSimulacao { get; set; }
}
