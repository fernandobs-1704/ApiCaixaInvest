namespace ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;

/// <summary>
/// Resumo estatístico das simulações por produto e dia, para uso em dashboards e análises.
/// </summary>
public class SimulacoesPorProdutoDiaResponse
{
    /// <summary>
    /// Nome do produto de investimento.
    /// </summary>
    public string Produto { get; set; } = string.Empty;

    /// <summary>
    /// Data em que as simulações foram realizadas.
    /// </summary>
    public DateTime Data { get; set; }

    /// <summary>
    /// Quantidade total de simulações realizadas para o produto nessa data.
    /// </summary>
    public int QuantidadeSimulacoes { get; set; }

    /// <summary>
    /// Valor médio final das simulações para o produto nessa data.
    /// </summary>
    public decimal MediaValorFinal { get; set; }
}
