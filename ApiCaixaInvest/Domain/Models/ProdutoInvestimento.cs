using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Domain.Models;

/// <summary>
/// Representa um produto de investimento disponível para simulação.
/// </summary>
public class ProdutoInvestimento
{
    /// <summary>
    /// Identificador único do produto.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome comercial do produto de investimento.
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Tipo do produto: CDB, LCI, LCA, Fundo, Tesouro Direto, etc.
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Rentabilidade anual (ex: 0.12 = 12% a.a.).
    /// </summary>
    public decimal RentabilidadeAnual { get; set; }

    /// <summary>
    /// Classificação de risco do produto: Baixo, Médio ou Alto.
    /// </summary>
    public string Risco { get; set; } = string.Empty;

    /// <summary>
    /// Prazo mínimo exigido pelo produto para simulação (em meses).
    /// </summary>
    public int PrazoMinimoMeses { get; set; }

    /// <summary>
    /// Tempo de liquidez do produto (dias necessários para resgate).
    /// </summary>
    public int LiquidezDias { get; set; }
}
