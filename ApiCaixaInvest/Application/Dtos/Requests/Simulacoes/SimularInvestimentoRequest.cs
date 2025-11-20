namespace ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;

public class SimularInvestimentoRequest
{
    public int ClienteId { get; set; }
    public decimal Valor { get; set; }
    public int PrazoMeses { get; set; }
    public string TipoProduto { get; set; } = string.Empty;
}
