using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Domain.Models;

/// <summary>
/// Representa uma simulação de investimento realizada pelo cliente.
/// Pode ser posteriormente efetivada como investimento real.
/// </summary>
[NotMapped]
public class SimulacaoInvestimento
{
    /// <summary>
    /// Identificador único da simulação.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identificador do cliente que realizou a simulação.
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// Identificador do produto selecionado para a simulação.
    /// </summary>
    public int ProdutoInvestimentoId { get; set; }

    /// <summary>
    /// Produto selecionado para esta simulação (propriedade de navegação).
    /// </summary>
    public ProdutoInvestimento? ProdutoInvestimento { get; set; }

    /// <summary>
    /// Valor investido na simulação.
    /// </summary>
    public decimal ValorInvestido { get; set; }

    /// <summary>
    /// Valor final estimado ao final do prazo.
    /// </summary>
    public decimal ValorFinal { get; set; }

    /// <summary>
    /// Prazo da simulação em meses.
    /// </summary>
    public int PrazoMeses { get; set; }

    /// <summary>
    /// Data e hora da simulação.
    /// </summary>
    public DateTime DataSimulacao { get; set; }

    /// <summary>
    /// Indica se essa simulação já foi transformada em um investimento real.
    /// </summary>
    public bool Efetivada { get; set; } = false;
}
