namespace ApiCaixaInvest.Application.Dtos.Responses.Produtos;

/// <summary>
/// Representa um produto de investimento recomendado com base no perfil de risco do cliente.
/// </summary>
public class ProdutoRecomendadoResponse
{
    /// <summary>
    /// Identificador único do produto de investimento.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome comercial do produto (ex.: CDB Caixa Liquidez Diária, LCI Caixa 2 Anos).
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Tipo do produto (ex.: CDB, LCI, LCA, Tesouro, Fundo).
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Rentabilidade anual projetada ou contratada (ex.: 0.12 = 12% a.a.).
    /// </summary>
    public decimal Rentabilidade { get; set; }

    /// <summary>
    /// Nível de risco do produto (Baixo, Médio ou Alto).
    /// </summary>
    public string Risco { get; set; } = string.Empty;
}
