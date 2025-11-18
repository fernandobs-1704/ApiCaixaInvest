namespace ApiCaixaInvest.Models;

public class InvestimentoHistorico
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Rentabilidade { get; set; }
    public DateTime Data { get; set; }
}
