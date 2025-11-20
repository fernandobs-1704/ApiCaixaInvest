using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Domain.Models;

/// <summary>
/// Armazena o último perfil de risco calculado para o cliente.
/// Atualizado sempre que uma nova simulação é efetivada.
/// </summary>
public class PerfilCliente
{
    /// <summary>
    /// Identificador único do registro.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identificador do cliente relacionado ao perfil.
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// Perfil textual calculado (Conservador, Moderado ou Agressivo).
    /// </summary>
    public string Perfil { get; set; } = string.Empty;

    /// <summary>
    /// Pontuação utilizada no algoritmo de determinação do perfil.
    /// </summary>
    public int Pontuacao { get; set; }

    /// <summary>
    /// Momento em que o perfil foi atualizado pela última vez.
    /// </summary>
    public DateTime UltimaAtualizacao { get; set; }
}
