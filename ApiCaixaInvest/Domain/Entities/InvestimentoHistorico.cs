namespace ApiCaixaInvest.Domain.Models;

public class InvestimentoHistorico
{
    public int Id { get; set; }
    public int ClienteId { get; set; }

    /// <summary>
    /// Snapshot do tipo na data da aplicação (ex.: CDB, LCI, Fundo Multimercado).
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Valor aplicado.
    /// </summary>
    public decimal Valor { get; set; }

    /// <summary>
    /// Rentabilidade esperada no momento da aplicação (anual).
    /// </summary>
    public decimal Rentabilidade { get; set; }

    /// <summary>
    /// Data em que o investimento foi realizado/efetivado.
    /// </summary>
    public DateTime Data { get; set; }

    /// <summary>
    /// FK para o produto de investimento usado na época da aplicação.
    /// </summary>
    public int ProdutoInvestimentoId { get; set; }
    public ProdutoInvestimento? ProdutoInvestimento { get; set; }
}
