namespace ApiCaixaInvest.Domain.Models;

public class ProdutoInvestimento
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // CDB, LCI, LCA, Fundo, Tesouro
    public decimal RentabilidadeAnual { get; set; }  // ex: 0.12 = 12% a.a.
    public string Risco { get; set; } = string.Empty; // Baixo, Médio, Alto
    public int PrazoMinimoMeses { get; set; }
    public int LiquidezDias { get; set; }
}
