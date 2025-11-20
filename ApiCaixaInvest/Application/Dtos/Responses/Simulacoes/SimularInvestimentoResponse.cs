using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;

/// <summary>
/// Resposta completa da simulação de investimento, contendo produto validado e resultado financeiro.
/// </summary>
[NotMapped]
public class SimularInvestimentoResponse
{
    /// <summary>
    /// Identificador da simulação persistida no banco.
    /// </summary>
    public int SimulacaoId { get; set; }

    /// <summary>
    /// Produto que foi selecionado/validado de acordo com os parâmetros informados.
    /// </summary>
    public ProdutoResponse ProdutoValidado { get; set; } = new();

    /// <summary>
    /// Resultado da simulação financeira (valor final, rentabilidade, prazo).
    /// </summary>
    public ResultadoSimulacaoResponse ResultadoSimulacao { get; set; } = new();

    /// <summary>
    /// Data e hora em que a simulação foi realizada.
    /// </summary>
    public DateTime DataSimulacao { get; set; }
}

/// <summary>
/// Dados do produto de investimento utilizado na simulação.
/// </summary>
public class ProdutoResponse
{
    /// <summary>
    /// Identificador do produto.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome comercial do produto.
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Tipo do produto (ex.: CDB, LCI, Tesouro, Fundo).
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Rentabilidade anual do produto (ex.: 0.12 = 12% a.a.).
    /// </summary>
    public decimal Rentabilidade { get; set; }

    /// <summary>
    /// Nível de risco associado ao produto (Baixo, Médio, Alto).
    /// </summary>
    public string Risco { get; set; } = string.Empty;
}

/// <summary>
/// Resultado numérico de uma simulação de investimento.
/// </summary>
public class ResultadoSimulacaoResponse
{
    /// <summary>
    /// Valor final estimado ao término do prazo, em reais.
    /// </summary>
    public decimal ValorFinal { get; set; }

    /// <summary>
    /// Rentabilidade efetiva utilizada no cálculo (ex.: 0.12 = 12%).
    /// </summary>
    public decimal RentabilidadeEfetiva { get; set; }

    /// <summary>
    /// Prazo da aplicação em meses.
    /// </summary>
    public int PrazoMeses { get; set; }
}
