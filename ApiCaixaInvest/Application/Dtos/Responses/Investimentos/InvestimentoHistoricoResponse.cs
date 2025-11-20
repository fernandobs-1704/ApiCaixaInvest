using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Application.Dtos.Responses.Investimentos;

/// <summary>
/// Item do histórico de investimentos efetivados pelo cliente.
/// </summary>
[NotMapped]
public class InvestimentoHistoricoResponse
{
    /// <summary>
    /// Identificador único do investimento.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tipo do produto (ex.: CDB, Tesouro, Fundo Multimercado).
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Valor aplicado (em reais).
    /// </summary>
    public decimal Valor { get; set; }

    /// <summary>
    /// Rentabilidade contratada ou observada (a.a.).
    /// </summary>
    public decimal Rentabilidade { get; set; }

    /// <summary>
    /// Data em que o investimento foi efetivado.
    /// </summary>
    public DateTime Data { get; set; }
}
