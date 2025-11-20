namespace ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;

/// <summary>
/// Representa uma simulação de investimento já realizada e armazenada no histórico.
/// </summary>
public class SimulacaoHistoricoResponse
{
    /// <summary>
    /// Identificador da simulação.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identificador do cliente que realizou a simulação.
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// Nome do produto utilizado na simulação.
    /// </summary>
    public string Produto { get; set; } = string.Empty;

    /// <summary>
    /// Valor investido na simulação, em reais.
    /// </summary>
    public decimal ValorInvestido { get; set; }

    /// <summary>
    /// Valor final simulado ao término do prazo.
    /// </summary>
    public decimal ValorFinal { get; set; }

    /// <summary>
    /// Prazo da aplicação, em meses.
    /// </summary>
    public int PrazoMeses { get; set; }

    /// <summary>
    /// Data e hora em que a simulação foi realizada.
    /// </summary>
    public DateTime DataSimulacao { get; set; }
}
