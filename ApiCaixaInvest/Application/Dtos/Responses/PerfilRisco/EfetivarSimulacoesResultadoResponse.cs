using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;

/// <summary>
/// Resultado da efetivação de simulações em investimentos reais,
/// incluindo o novo perfil de risco do cliente.
/// </summary>
[NotMapped]
public class EfetivarSimulacoesResultadoResponse
{
    /// <summary>
    /// Indica se houve pelo menos uma simulação efetivada com sucesso.
    /// </summary>
    public bool Sucesso { get; set; }

    /// <summary>
    /// Mensagem amigável de retorno.
    /// </summary>
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>
    /// Identificador do cliente.
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// Lista de identificadores das simulações efetivadas.
    /// </summary>
    public List<int> SimulacoesEfetivadas { get; set; } = new();

    /// <summary>
    /// Lista de identificadores das simulações já efetivadas.
    /// </summary>
    public IEnumerable<int> SimulacoesJaEfetivadas { get; set; } = new List<int>();

    /// <summary>
    /// Lista de identificadores das simulações não encontradas.
    /// </summary>
    public IEnumerable<int> SimulacoesNaoEncontradas { get; set; } = new List<int>();

    /// <summary>
    /// Perfil de risco recalculado após a efetivação.
    /// </summary>
    public PerfilRiscoResponse? PerfilRisco { get; set; }
}

public class EfetivarSimulacoesInternalResultado
{
    public List<int> Efetivadas { get; set; } = new();
    public List<int> JaEfetivadas { get; set; } = new();
    public List<int> NaoEncontradas { get; set; } = new();
}
