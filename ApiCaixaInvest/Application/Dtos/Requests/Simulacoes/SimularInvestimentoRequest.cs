namespace ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;

/// <summary>
/// Dados de entrada para simulação de um investimento.
/// </summary>
public class SimularInvestimentoRequest
{
    /// <summary>
    /// Identificador do cliente que está realizando a simulação.
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// Valor a ser investido (em reais).
    /// </summary>
    public decimal Valor { get; set; }

    /// <summary>
    /// Prazo da aplicação em meses.
    /// </summary>
    public int PrazoMeses { get; set; }

    /// <summary>
    /// Tipo do produto desejado (ex.: CDB, LCI, LCA, Tesouro, Fundo).
    /// </summary>
    public string TipoProduto { get; set; } = string.Empty;
}
