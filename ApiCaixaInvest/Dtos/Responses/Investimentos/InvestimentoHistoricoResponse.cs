namespace ApiCaixaInvest.Dtos.Responses.Investimentos;

public class InvestimentoHistoricoResponse
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Rentabilidade { get; set; }
    public DateTime Data { get; set; }
}
