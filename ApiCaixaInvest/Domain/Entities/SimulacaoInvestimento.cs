namespace ApiCaixaInvest.Domain.Models;

public class SimulacaoInvestimento
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int ProdutoInvestimentoId { get; set; }
    public decimal ValorInvestido { get; set; }
    public decimal ValorFinal { get; set; }
    public int PrazoMeses { get; set; }
    public DateTime DataSimulacao { get; set; }

    public ProdutoInvestimento? ProdutoInvestimento { get; set; }
}
